using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class AnalyticsUnsentSessionsHandler
{
    // The key is the guid of the session
    private readonly Dictionary<string, AggregatedAnalyticsPayload> unsentAggregatedPayloads = new();
    private readonly string analyticsSaveLocation = Application.persistentDataPath + "/unsent.analytics";

    public AnalyticsUnsentSessionsHandler()
    {
        if (!File.Exists(analyticsSaveLocation))
        {
            File.WriteAllText(analyticsSaveLocation, "");
            return;
        }
        
        string contents = string.Join("", File.ReadAllLines(analyticsSaveLocation));
        string[] sections = contents.Split("---");
        
        if (sections.Length == 0 || string.IsNullOrWhiteSpace(sections[0]))
        {
            return;
        }

        foreach (string section in sections)
        {
            var unsentPayload = JsonConvert.DeserializeObject<AggregatedAnalyticsPayload>(section);
            
            unsentAggregatedPayloads[unsentPayload.sessionId] = unsentPayload;
        }
    }

    public void SaveUnsentSession(AggregatedAnalyticsPayload payload)
    {
        unsentAggregatedPayloads[payload.sessionId] = payload;
        File.WriteAllText(analyticsSaveLocation, fileContents);
    }

    public void RemoveSentSession(string sessionGuid)
    {
        if (unsentAggregatedPayloads.Remove(sessionGuid))
        {
            File.WriteAllText(analyticsSaveLocation, fileContents);
        }
    }

    public IEnumerable<KeyValuePair<string, AggregatedAnalyticsPayload>> GetUnsentSessions() => unsentAggregatedPayloads;
    
    private string fileContents { get => string.Join("\n---\n", unsentAggregatedPayloads.Select(kv => JsonConvert.SerializeObject(kv.Value))); }
}
