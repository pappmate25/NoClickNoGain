using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField]
    private LargeNumber totalGain;
    [SerializeField]
    private LargeNumber resetCoin;
    [SerializeField]
    private QuitDate quitDate;

    private void Awake()
    {
        saveDataContainer.Load();

        gain.Value = saveDataContainer.Gain;
        totalGain.Value = saveDataContainer.TotalGain;
        resetCoin.Value = saveDataContainer.ResetCoin;
        quitDate.Value = DateTime.Now - saveDataContainer.QuitDate;


        foreach (var upgrade in resetUpgrades.ResetUpgrades)
        {
            upgrade.SetPurchased(saveDataContainer.ResetUpgrades.GetValueOrDefault(upgrade.name, false));
        }
        foreach (var upgrade in clickUpgrades.Upgrades)
        {
            upgrade.SetLevel(saveDataContainer.ClickUpgrades.GetValueOrDefault(upgrade.name, 0));
        }
        foreach (var upgrade in idleUpgrades.Upgrades)
        {
            upgrade.SetLevel(saveDataContainer.IdleUpgrades.GetValueOrDefault(upgrade.name, 0));
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
            TotalGain = totalGain.Value,
            ResetCoin = resetCoin.Value,
            QuitDate = DateTime.Now,
            ClickUpgrades = clickUpgrades.Upgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.currentLevel),
            IdleUpgrades = idleUpgrades.Upgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.currentLevel),
            ResetUpgrades = resetUpgrades.ResetUpgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.isPurchased),
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
    public double TotalGain;
    public double ResetCoin;
    public DateTime QuitDate;
    public Dictionary<string, int> ClickUpgrades;
    public Dictionary<string, int> IdleUpgrades;
    public Dictionary<string, bool> ResetUpgrades;
}
