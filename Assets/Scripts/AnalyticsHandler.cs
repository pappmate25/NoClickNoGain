using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class AnalyticsHandler : MonoBehaviour
{
    private List<AnalyticsEvent> events;
    
    public void OnEvent(IGameEventDetails gameEventDetails)
    {
        var newEvent = gameEventDetails switch
        {
            GainChangedEventDetails gainChangedEventDetails => new AnalyticsEvent
            {
                EventType = EventType.GainChanged,
                Parameters = new[]
                {
                    ("GainChangeSource", Enum.GetName(typeof(GainChangeType), gainChangedEventDetails.ChangeType)),
                    ("NewGain", gainChangedEventDetails.NewGain.ToString(CultureInfo.InvariantCulture))
                }
            },
            PassiveSkillBought passiveSkillBought => throw new NotImplementedException(),
            ResetEventDetails resetEventDetails => throw new NotImplementedException(),
            ResetUpgradeBought resetUpgradeBought => throw new NotImplementedException(),
            UpgradeBought upgradeBought => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(gameEventDetails), "Unhandled event type")
        };
        
        newEvent.Timestamp = DateTimeOffset.Now;
        
        events.Add(newEvent);
    }
}

public struct AnalyticsEvent
{
    public DateTimeOffset Timestamp;
    public Enum EventType;
    public (string, string)[] Parameters;
    
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
