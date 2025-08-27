using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class AnalyticsHandler : MonoBehaviour
{
    private List<AnalyticsEvent> events;
    
    private void Awake()
    {
        events = new List<AnalyticsEvent>();
    }

    public void OnEvent(IGameEventDetails gameEventDetails)
    {
        Debug.Log(gameEventDetails.GetType().Name);
        
        AnalyticsEvent? newEvent = gameEventDetails switch
        {
            GainChangedEventDetails gainChangedEventDetails => new AnalyticsEvent(EventType.GainChanged,
            new[]
            {
                ("NewGain", gainChangedEventDetails.NewGain.ToString(CultureInfo.InvariantCulture)),
                ("GainSource", Enum.GetName(typeof(GainChangeType), gainChangedEventDetails.ChangeType))
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
                ("Upgrade", upgradeBought.Upgrade.Name),
                ("TargetLevel", upgradeBought.TargetLevel.ToString())
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(gameEventDetails), "Unhandled event type")
        };

        if (newEvent == null) return;

        var savedEvent = newEvent.Value;
        events.Add(savedEvent);
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
    public readonly Enum EventType;
    public readonly (string, string)[] Parameters;

    public AnalyticsEvent(EventType eventType, (string, string)[] parameters)
    {
        EventType = eventType;
        Parameters = parameters;
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
    GainChanged,
    PassiveSkillBought,
    Reset,
    ResetUpgradeBought,
    UpgradeBought
}
