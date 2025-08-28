using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class AnalyticsHandler : MonoBehaviour
{
    [SerializeField]
    private bool aggregateClickGainEvents = true;

    private List<AnalyticsEvent> events;

    private void Awake()
    {
        events = new List<AnalyticsEvent>();
        events.Add(new AnalyticsEvent(EventType.SessionStart));
    }

    public void OnEvent(IGameEventDetails gameEventDetails)
    {
        bool mayAggregate = false;

        if (gameEventDetails is GainChangedEventDetails ev)
        {
            mayAggregate = ev.ChangeType == GainChangeType.Click && aggregateClickGainEvents;
        }

        AnalyticsEvent newEvent = gameEventDetails switch
        {
            GainChangedEventDetails gainChangedEventDetails => new AnalyticsEvent(EventType.GainChanged,
            new[]
            {
                ("NewGain", gainChangedEventDetails.NewGain.ToString(CultureInfo.InvariantCulture)), ("GainSource", Enum.GetName(typeof(GainChangeType), gainChangedEventDetails.ChangeType))
            }),
            PassiveSkillBought passiveSkillBought => new AnalyticsEvent(EventType.PassiveSkillBought, new[]
            {
                ("PassiveSkill", passiveSkillBought.PassiveSkill.Name)
            }),
            ResetEventDetails resetEventDetails => new AnalyticsEvent(EventType.Reset, new[]
            {
                ("GainAtTimeOfReset", resetEventDetails.GainOnReset.ToString(CultureInfo.InvariantCulture))
            }),
            ResetUpgradeBought resetUpgradeBought => new AnalyticsEvent(EventType.ResetUpgradeBought, new[]
            {
                ("ResetUpgrade", resetUpgradeBought.ResetUpgrade.Name)
            }),
            UpgradeBought upgradeBought => new AnalyticsEvent(EventType.UpgradeBought, new[]
            {
                ("Upgrade", upgradeBought.Upgrade.Name), ("TargetLevel", upgradeBought.TargetLevel.ToString())
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(gameEventDetails), "Unhandled event type")
        };

        if (mayAggregate && events[^1].EventType is EventType.ClickGainChangedAggregated or EventType.GainChanged && newEvent.EventType == EventType.GainChanged)
        {
            // Aggregate click gain events
            events[^1] = new AnalyticsEvent(EventType.ClickGainChangedAggregated,
            new[]
            {
                ("PreviousGain", events[^1].Parameters[0].Item2), ("NewGain", newEvent.Parameters[0].Item2), ("StartTime", events[^1].Timestamp.ToString("o")),
            });
            return;
        }

        events.Add(newEvent);
    }

    [ContextMenu("Print All Analytics Events")]
    public void PrintAllEvents()
    {
        foreach (var analyticsEvent in events)
        {
            Debug.Log(analyticsEvent.ToString());
        }
    }
}

public struct AnalyticsEvent
{
    public readonly DateTimeOffset Timestamp;
    public readonly EventType EventType;
    public readonly (string, string)[] Parameters;

    public AnalyticsEvent(EventType eventType, (string, string)[] parameters = null)
    {
        EventType = eventType;
        Parameters = parameters ?? Array.Empty<(string, string)>();
        Timestamp = DateTimeOffset.Now;
    }

    public override string ToString()
    {
        var parametersString = string.Join(", ", Array.ConvertAll(Parameters, p => $"{p.Item1}: {p.Item2}"));
        return $"[{Timestamp}] EventType: {EventType}, Parameters: {{{parametersString}}}";
    }
}

public enum EventType
{
    SessionStart,
    GainChanged,
    ClickGainChangedAggregated,
    PassiveSkillBought,
    Reset,
    ResetUpgradeBought,
    UpgradeBought
}
