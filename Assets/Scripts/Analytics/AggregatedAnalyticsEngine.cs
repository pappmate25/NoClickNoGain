using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class AggregatedAnalyticsEngine : IAnalyticsEngine
{
    private readonly Dictionary<AnalyticsEventType, int> eventCounts;

    public AggregatedAnalyticsEngine()
    {
        eventCounts = new Dictionary<AnalyticsEventType, int>();

        foreach (AnalyticsEventType eventType in Enum.GetValues(typeof(AnalyticsEventType)))
        {
            eventCounts[eventType] = 0;
        }
    }

    public void LogEvent(IGameEventDetails gameEventDetails)
    {
        AnalyticsEventType eventType;
        switch (gameEventDetails)
        {
            case GainChangedEventDetails { ChangeType: GainChangeType.Click }:
                eventType = AnalyticsEventType.ClickGainChanged;
                break;
            case GainChangedEventDetails { ChangeType: GainChangeType.Idle }:
                eventType = AnalyticsEventType.IdleGainChanged;
                break;
            case GainChangedEventDetails:
                eventType = AnalyticsEventType.GainChanged;
                break;
            case PassiveSkillBought:
                eventType = AnalyticsEventType.PassiveSkillBought;
                break;
            case ResetEventDetails:
                eventType = AnalyticsEventType.Reset;
                break;
            case ResetUpgradeBought:
                eventType = AnalyticsEventType.ResetUpgradeBought;
                break;
            case UpgradeBought:
                eventType = AnalyticsEventType.UpgradeBought;
                break;
            default:
                return;
        }

        eventCounts[eventType]++;
    }
    
    public AggregatedAnalyticsPayload ToPayload(Guid sessionId, DateTime sessionStart, DateTime sessionEnd)
    {
        return new AggregatedAnalyticsPayload
        {
            sessionId = sessionId.ToString(),
            sessionStart = sessionStart.ToString("o", CultureInfo.InvariantCulture),
            sessionEnd = sessionEnd.ToString("o", CultureInfo.InvariantCulture),
            clickGainChange = eventCounts[AnalyticsEventType.ClickGainChanged],
            idleGainChange = eventCounts[AnalyticsEventType.IdleGainChanged],
            otherGainChange = eventCounts[AnalyticsEventType.GainChanged],
            passiveSkillBought = eventCounts[AnalyticsEventType.PassiveSkillBought],
            reset = eventCounts[AnalyticsEventType.Reset],
            resetUpgradeBought = eventCounts[AnalyticsEventType.ResetUpgradeBought],
            regularUpgradeBought = eventCounts[AnalyticsEventType.UpgradeBought],
        };
    }

    public IEnumerable<string> GetAllEventsAsStrings()
    {
        return eventCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}");
    }
}

[Serializable]
public struct AggregatedAnalyticsPayload
{
    public string sessionId;
    public string sessionStart;
    public string sessionEnd;
    public int clickGainChange;
    public int idleGainChange;
    public int otherGainChange;
    public int passiveSkillBought;
    public int reset;
    public int resetUpgradeBought;
    public int regularUpgradeBought;
}
