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
            // TODO: Try sync if not append to file
            yield return new WaitForSecondsRealtime(syncInterval);
        }
    }
}
