using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class DetailedAnalyticsEngine: IAnalyticsEngine
{
    private readonly List<DetailedAnalyticsEvent> events = new List<DetailedAnalyticsEvent>();
    
    public void LogEvent(IGameEventDetails gameEventDetails)
    {
        DetailedAnalyticsEvent newEvent = gameEventDetails switch
        {
            GainChangedEventDetails gainChangedEventDetails => new DetailedAnalyticsEvent(AnalyticsEventType.GainChanged,
            new[]
            {
                ("NewGain", gainChangedEventDetails.NewGain.ToString(CultureInfo.InvariantCulture)), ("GainSource", Enum.GetName(typeof(GainChangeType), gainChangedEventDetails.ChangeType))
            }),
            PassiveSkillBought passiveSkillBought => new DetailedAnalyticsEvent(AnalyticsEventType.PassiveSkillBought, new[]
            {
                ("PassiveSkill", passiveSkillBought.PassiveSkill.Name)
            }),
            ResetEventDetails resetEventDetails => new DetailedAnalyticsEvent(AnalyticsEventType.Reset, new[]
            {
                ("GainAtTimeOfReset", resetEventDetails.GainOnReset.ToString(CultureInfo.InvariantCulture))
            }),
            ResetUpgradeBought resetUpgradeBought => new DetailedAnalyticsEvent(AnalyticsEventType.ResetUpgradeBought, new[]
            {
                ("ResetUpgrade", resetUpgradeBought.ResetUpgrade.Name)
            }),
            UpgradeBought upgradeBought => new DetailedAnalyticsEvent(AnalyticsEventType.UpgradeBought, new[]
            {
                ("Upgrade", upgradeBought.Upgrade.Name), ("TargetLevel", upgradeBought.TargetLevel.ToString())
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(gameEventDetails), "Unhandled event type")
        };

        events.Add(newEvent);
    }
    
    public IEnumerable<string> GetAllEventsAsStrings()
    {
        return events.Select(e => e.ToString());
    }
}

public readonly struct DetailedAnalyticsEvent 
{
    public readonly DateTimeOffset Timestamp;
    public readonly AnalyticsEventType EventType;
    public readonly (string, string)[] Parameters;

    public DetailedAnalyticsEvent(AnalyticsEventType eventType, (string, string)[] parameters = null)
    {
        EventType = eventType;
        Parameters = parameters ?? Array.Empty<(string, string)>();
        Timestamp = DateTimeOffset.UtcNow;
    }

    public override string ToString()
    {
        var parametersString = string.Join(", ", Array.ConvertAll(Parameters, p => $"{p.Item1}: {p.Item2}"));
        return $"[{Timestamp}] EventType: {EventType}, Parameters: {{{parametersString}}}";
    }
}
