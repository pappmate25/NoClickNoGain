using System;
using System.Collections;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class AnalyticsHandler : MonoBehaviour
{
    private const string analyticsEndpoint = "http://localhost:3000/submit";
    
    [SerializeField]
    private BoolVariable enableAnalytics;

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

    private void Awake()
    {
        if (!enableAnalytics.Value) return;
        
        eventListenerObject.GetComponents<GameEventListener>()
            .ToList()
            .ForEach(listener => listener.Response.AddListener(OnEvent));
        
        sessionStart = DateTimeOffset.UtcNow;
        sessionId = Guid.NewGuid();
        analyticsEngine = aggregateEvents ? new AggregatedAnalyticsEngine() : new DetailedAnalyticsEngine();

        if (aggregateEvents)
        {
            StartCoroutine(SyncLoop());
        }

        unsentSessionsHandler = new AnalyticsUnsentSessionsHandler();
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

    private IEnumerator SyncLoop()
    {
        if (!aggregateEvents) throw new NotImplementedException();

        var aggregatedEngine = (AggregatedAnalyticsEngine)analyticsEngine;
        
        using var syncWebRequest = new UnityWebRequest(analyticsEndpoint, "POST", null, null); 
        syncWebRequest.timeout = 30;
        syncWebRequest.SetRequestHeader("Content-Type", "application/json");

        while (Application.isPlaying)
        {
            float syncInterval = Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork
                ? lanSyncInterval
                : mobileDataSyncInterval;
            
            yield return new WaitForSecondsRealtime(syncInterval);

            var sessionEnd = DateTimeOffset.UtcNow;
            var payload = aggregatedEngine.ToPayload(sessionId, sessionStart.UtcDateTime, sessionEnd.UtcDateTime);
            unsentSessionsHandler.SaveUnsentSession(payload);
            
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                var unsentSessions = unsentSessionsHandler.GetUnsentSessions().ToList();
                
                foreach (var (sendableSessionId, unsentPayload) in unsentSessions)
                {
                    byte[] unsentBodyRaw = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(unsentPayload));
                    syncWebRequest.uploadHandler = new UploadHandlerRaw(unsentBodyRaw);
                    yield return syncWebRequest.SendWebRequest();

                    if (syncWebRequest.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Analytics sync successful");
                        unsentSessionsHandler.RemoveSentSession(sendableSessionId);
                    }
                    else
                    {
                        Debug.LogWarning($"Analytics sync failed: {syncWebRequest.error}. Unsent session count: {unsentSessions.Count}");
                        break;
                    }
                }
            }
        }
    }
}
