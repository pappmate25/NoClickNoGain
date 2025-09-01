using System;
using System.Collections.Generic;
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
        AnalyticsEventType eventType = gameEventDetails switch
        {
            GainChangedEventDetails { ChangeType: GainChangeType.Click } => AnalyticsEventType.ClickGainChanged,
            GainChangedEventDetails { ChangeType: GainChangeType.Idle } => AnalyticsEventType.IdleGainChanged,
            GainChangedEventDetails => AnalyticsEventType.GainChanged,
            PassiveSkillBought => AnalyticsEventType.PassiveSkillBought,
            ResetEventDetails => AnalyticsEventType.Reset,
            ResetUpgradeBought => AnalyticsEventType.ResetUpgradeBought,
            UpgradeBought => AnalyticsEventType.UpgradeBought,
            _ => throw new ArgumentOutOfRangeException(nameof(gameEventDetails), "Unhandled event type")
        };

        eventCounts[eventType]++;
    }

    public IEnumerable<string> GetAllEvents()
    {
        return eventCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}");
    }
    
    public string ToJson()
    {
        return $@"{{
  ""ClickGainChanged"": {eventCounts[AnalyticsEventType.ClickGainChanged]},
  ""IdleGainChanged"": {eventCounts[AnalyticsEventType.IdleGainChanged]},
  ""GainChanged"": {eventCounts[AnalyticsEventType.GainChanged]},
  ""PassiveSkillBought"": {eventCounts[AnalyticsEventType.PassiveSkillBought]},
  ""Reset"": {eventCounts[AnalyticsEventType.Reset]},
  ""ResetUpgradeBought"": {eventCounts[AnalyticsEventType.ResetUpgradeBought]},
  ""UpgradeBought"": {eventCounts[AnalyticsEventType.UpgradeBought]}
}}";
    }
}
