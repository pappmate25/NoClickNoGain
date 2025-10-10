using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class AnalyticsUnsentSessionsHandler
{
    // The key is the guid of the session
    private readonly Dictionary<string, AggregatedAnalyticsPayload> unsentAggregatedPayloads = new();
    private string analyticsSaveLocation;

    public AnalyticsUnsentSessionsHandler(string analyticsSaveLocation)
    {
        this.analyticsSaveLocation = analyticsSaveLocation;

        try
        {
            string contents = File.ReadAllText(analyticsSaveLocation);
            List<AggregatedAnalyticsPayload> unsentSessions = JsonConvert.DeserializeObject<List<AggregatedAnalyticsPayload>>(contents);

            foreach (var unsentSession in unsentSessions)
            {
                unsentAggregatedPayloads[unsentSession.sessionId] = unsentSession;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Cannot deserialize analytics data, {ex}");
        }
    }

    public void SaveUnsentSession(AggregatedAnalyticsPayload payload)
    {
        unsentAggregatedPayloads[payload.sessionId] = payload;
        File.WriteAllText(analyticsSaveLocation, GetFileContents());
    }

    public void RemoveAllSentSessions()
    {
        unsentAggregatedPayloads.Clear();
        File.WriteAllText(analyticsSaveLocation, GetFileContents());
    }

    public IEnumerable<KeyValuePair<string, AggregatedAnalyticsPayload>> GetUnsentSessions() => unsentAggregatedPayloads;
    
    private string GetFileContents()
    {

        return JsonConvert.SerializeObject(unsentAggregatedPayloads.Values.ToList());
    }
}
