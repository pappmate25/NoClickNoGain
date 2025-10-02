using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class AnalyticsHandler : MonoBehaviour
{
    private const string analyticsEndpoint = "http://localhost:3000/submit";
    
    [SerializeField]
    private bool enableAnalytics;

    [SerializeField]
    private bool aggregateEvents = true;
    [SerializeField]
    private float lanSyncInterval = 60f;
    [SerializeField]
    private float mobileDataSyncInterval = 300f;

    [SerializeField]
    private GameObject eventListenerObject;

    private DateTimeOffset sessionStart;
    private Guid sessionId;
    private IAnalyticsEngine analyticsEngine;
    private AnalyticsUnsentSessionsHandler unsentSessionsHandler;

    private string csrfKey = null;

    private void Awake()
    {
        if (!enableAnalytics) return;
        
        eventListenerObject.GetComponents<GameEventListener>()
            .ToList()
            .ForEach(listener => listener.Response.AddListener(OnEvent));
        
        sessionStart = DateTimeOffset.UtcNow;
        sessionId = Guid.NewGuid();
        analyticsEngine = aggregateEvents ? new AggregatedAnalyticsEngine() : new DetailedAnalyticsEngine();
        unsentSessionsHandler = new AnalyticsUnsentSessionsHandler();

        if (aggregateEvents)
        {
            StartCoroutine(SyncLoopAggregated((AggregatedAnalyticsEngine) analyticsEngine));
        }
        else
        {
            Debug.LogError("Non aggregated analytics engine is not supported");
        }
    }

    private void OnEvent(IGameEventDetails gameEventDetails)
    {
        analyticsEngine.LogEvent(gameEventDetails);
    }

    [ContextMenu("Print All Analytics Events In Memory")]
    public void PrintAllEvents()
    {
        Debug.Log($"Session ID: {sessionId}, Session Start Time: {sessionStart}");
        foreach (var analyticsEvent in analyticsEngine.GetAllEventsAsStrings())
        {
            Debug.Log(analyticsEvent);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator SyncLoopAggregated(AggregatedAnalyticsEngine aggregatedAnalyticsEngine)
    {
        Debug.Log($"Aggregated Analytics Engine: {aggregatedAnalyticsEngine}");

        while (Application.isPlaying)
        {
            var sessionEnd = DateTimeOffset.UtcNow;
            var payload = aggregatedAnalyticsEngine.ToPayload(sessionId, sessionStart.UtcDateTime, sessionEnd.UtcDateTime);
            unsentSessionsHandler.SaveUnsentSession(payload);
            
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                if (csrfKey == null)
                {
                    DownloadHandler handler = new DownloadHandlerBuffer();
                    
                    using var csrfRequest = UnityWebRequest.Get(analyticsEndpoint);
                    csrfRequest.timeout = 30;
                    csrfRequest.downloadHandler = handler;
                    yield return csrfRequest.SendWebRequest();

                    if (csrfRequest.result == UnityWebRequest.Result.Success)
                    {
                        csrfKey = handler.text;
                        Debug.Log("CSRF token obtained.");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to obtain CSRF token: {csrfRequest.error}");
                        yield return new WaitForSecondsRealtime(10f);
                        continue;
                    }
                }
                
                var unsentSessions = unsentSessionsHandler.GetUnsentSessions().ToList();

                SessionsSubmit sessionSubmit = new()
                {
                    sessions = unsentSessions.Select((kvp) => kvp.Value).ToArray(),
                    version = SessionsSubmit.OBJECT_VERSION
                };
                
                string postData = JsonConvert.SerializeObject(sessionSubmit);
                using var syncWebRequest = UnityWebRequest.Post(analyticsEndpoint, postData, "application/json");
                syncWebRequest.timeout = 30;
                syncWebRequest.SetRequestHeader("X-CSRF-Token", csrfKey);
                yield return syncWebRequest.SendWebRequest();

                if (syncWebRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Analytics sync successful");
                    unsentSessionsHandler.RemoveAllSentSessions();
                }
                else
                {
                    Debug.LogWarning($"Analytics sync failed: {syncWebRequest.error}. Unsent session count: {unsentSessions.Count}");
                }
            }

            float syncInterval = Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork
                ? lanSyncInterval
                : mobileDataSyncInterval;

            yield return new WaitForSecondsRealtime(syncInterval);
        }
    }
}

[Serializable]
public struct SessionsSubmit
{
    public const int OBJECT_VERSION = 1;

    public int version; 
    public AggregatedAnalyticsPayload[] sessions;
}
