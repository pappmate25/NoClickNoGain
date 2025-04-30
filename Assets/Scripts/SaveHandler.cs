using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class SaveHandler : MonoBehaviour
{
    private string pathToSaveFile = "";

    [SerializeField]
    private UpgradeList clickUpgrades;
    [SerializeField]
    private UpgradeList idleUpgrades;
    [SerializeField]
    private ResetUpgradeList resetUpgrades;
    [SerializeField]
    private LargeNumber gain;

    private void Start()
    {
        pathToSaveFile = Path.Combine(Application.persistentDataPath, "savefile.json");
        Load();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void Save()
    {
        SaveData saveData = new SaveData
        {
            Gain = gain.Value,
            ClickUpgrades = clickUpgrades.Upgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.currentLevel),
            IdleUpgrades = idleUpgrades.Upgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.currentLevel),
            ResetUpgrades = resetUpgrades.ResetUpgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.Upgrade.currentLevel)
        };

        string json = JsonConvert.SerializeObject(saveData);

        File.WriteAllText(pathToSaveFile, json);
    }

    private void Load()
    {
        if (File.Exists(pathToSaveFile))
        {
            string json = File.ReadAllText(pathToSaveFile);
            object jsonObject = JsonConvert.DeserializeObject<SaveData?>(json);

            if (jsonObject == null)
            {
                Debug.LogError("Failed to deserialize JSON.");
                return;
            }

            SaveData saveData = (SaveData)jsonObject;

            gain.Value = saveData.Gain;
            foreach (var upgrade in clickUpgrades.Upgrades)
            {
                if (saveData.ClickUpgrades.TryGetValue(upgrade.name, out int level))
                {
                    upgrade.SetLevel(level);
                }
            }
            foreach (var upgrade in idleUpgrades.Upgrades)
            {
                if (saveData.IdleUpgrades.TryGetValue(upgrade.name, out int level))
                {
                    upgrade.SetLevel(level);
                }
            }
            foreach (var upgrade in resetUpgrades.ResetUpgrades)
            {
                if (saveData.ResetUpgrades.TryGetValue(upgrade.name, out int level))
                {
                    upgrade.Upgrade.SetLevel(level);
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Save();
        }
    }
#endif

}

public struct SaveData
{
    public double Gain;
    public Dictionary<string, int> ClickUpgrades;
    public Dictionary<string, int> IdleUpgrades;
    public Dictionary<string, int> ResetUpgrades;
}
