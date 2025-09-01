using System;
using System.Collections;
using UnityEngine;

public class AnalyticsHandler : MonoBehaviour
{
    [SerializeField]
    private bool aggregateEvents = true;
    [SerializeField]
    private float syncInterval = 60f;

    private DateTimeOffset sessionStart;
    private Guid sessionId;
    private IAnalyticsEngine analyticsEngine;

    private void Awake()
    {
        sessionStart = DateTimeOffset.UtcNow;
        sessionId = Guid.NewGuid();
        analyticsEngine = aggregateEvents ? new AggregatedAnalyticsEngine() : new DetailedAnalyticsEngine();

        StartCoroutine(Sync());
    }

    public void OnEvent(IGameEventDetails gameEventDetails)
    {
        analyticsEngine.LogEvent(gameEventDetails);
    }

    [ContextMenu("Print All Analytics Events In Memory")]
    public void PrintAllEvents()
    {
        Debug.Log($"Session ID: {sessionId}, Session Start Time: {sessionStart}");
        foreach (var analyticsEvent in analyticsEngine.GetAllEvents())
        {
            Debug.Log(analyticsEvent);
        }
    }

    private IEnumerator Sync()
    {
        while (Application.isPlaying)
        {
            var sessionEnd = DateTimeOffset.UtcNow;
            var events = analyticsEngine.GetAllEvents();
            var payload = new
            {
                sessionId = sessionId.ToString(),
                sessionStart,
                sessionEnd,
                events,
            };
            string json = JsonUtility.ToJson(payload);
            using (var request = new UnityEngine.Networking.UnityWebRequest("http://localhost:3000", "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.Log("Analytics sync successful");
                }
                else
                {
                    Debug.LogWarning($"Analytics sync failed: {request.error}");
                }
            }
            yield return new WaitForSecondsRealtime(syncInterval);
        }
    }
}
