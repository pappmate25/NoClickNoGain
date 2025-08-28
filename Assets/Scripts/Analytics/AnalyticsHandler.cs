using System.Collections.Generic;
using UnityEngine;

public class AnalyticsHandler : MonoBehaviour
{
    [SerializeField]
    private bool aggregateEvents = true;
    [SerializeField]
    private int moveToStorageEventCount = 50;

    private IAnalyticsEngine analyticsEngine;

    private void Awake()
    {
        if (!aggregateEvents)
        {
            analyticsEngine = new DetailedAnalyticsEngine();
        }
    }

    public void OnEvent(IGameEventDetails gameEventDetails)
    {
        analyticsEngine.LogEvent(gameEventDetails);
    }

    [ContextMenu("Print All Analytics Events In Memory")]
    public void PrintAllEvents()
    {
        foreach (var analyticsEvent in analyticsEngine.GetAllEvents())
        {
            Debug.Log(analyticsEvent.ToString());
        }
    }
}
