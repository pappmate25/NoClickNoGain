using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class SaveHandler : MonoBehaviour
{
    [SerializeField]
    private SaveDataContainer saveDataContainer;

    [SerializeField]
    private UpgradeList clickUpgrades;
    [SerializeField]
    private UpgradeList idleUpgrades;
    [SerializeField]
    private ResetUpgradeList resetUpgrades;
    [SerializeField]
    private LargeNumber gain;

    private void Awake()
    {
        saveDataContainer.Load();

        gain.Value = saveDataContainer.Gain;
        foreach (var upgrade in clickUpgrades.Upgrades)
        {
            upgrade.SetLevel(saveDataContainer.ClickUpgrades.GetValueOrDefault(upgrade.name, 0));
        }
        foreach (var upgrade in idleUpgrades.Upgrades)
        {
            upgrade.SetLevel(saveDataContainer.IdleUpgrades.GetValueOrDefault(upgrade.name, 0));
        }
        foreach (var upgrade in resetUpgrades.ResetUpgrades)
        {
            upgrade.Upgrade.SetLevel(saveDataContainer.ResetUpgrades.GetValueOrDefault(upgrade.name, 0));
        }
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

        saveDataContainer.Save(saveData);
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

