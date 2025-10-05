using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class ConfigurationHandler : MonoBehaviour
{
    private static string configurationSavePath;
    public static ConfigurationData Configuration;

    private void Awake()
    {
        configurationSavePath = Path.Combine(SaveHandler.GetPersistentDataPath(), "settings.json");
        Load();
    }

    public static void Save()
    {
        string json = JsonConvert.SerializeObject(Configuration);
        try
        {
            File.WriteAllText(configurationSavePath, json);
        }
        catch
        {
            Debug.LogWarning("Could not save configuration");
        }
    }

    public static void Load()
    {
        if (configurationSavePath == null)
        {
            return;
        }

        try
        {
            string json = File.ReadAllText(configurationSavePath);
            Configuration = JsonConvert.DeserializeObject<ConfigurationData>(json);
            return;
        }
        catch
        {
            Debug.Log("Could not load configuration");
        }

        Configuration = new ConfigurationData();
    }

    public static void Delete()
    {
        try
        {
            File.Delete(configurationSavePath);
        }
        catch { }
    }
}

public struct ConfigurationData
{
    [JsonProperty("music-muted")]
    public bool MusicMuted;

    [JsonProperty("sfx-muted")]
    public bool SfxMuted;

    [JsonProperty("story-watched")]
    public bool StoryWatched;

    [JsonProperty("analytics-ack")]
    public bool AnalyticsAck;

    [JsonProperty("tutorial-step")]
    public int TutorialStep;

    [JsonProperty("tutorial-mask")]
    public int TutorialMask;
}
