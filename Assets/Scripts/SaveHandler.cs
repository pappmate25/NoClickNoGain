using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;

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
    private PassiveSkillList passiveSkills;
    [SerializeField]
    private LargeNumber gain;
    [SerializeField]
    private LargeNumber totalGain;
    [SerializeField]
    private LargeNumber resetCoin;
    [SerializeField]
    private QuitDate quitDate;
    [SerializeField]
    private LargeNumber resetStage;

    [SerializeField]
    private GameEvent gainChangedEvent;

    [SerializeField]
    private float autoSaveInterval = 60f;

    [SerializeField]
    private float upgradeDebounceTime = 5f;
    private float? lastBoughtUpgrade;

    private bool saveUnencrypted;

    private void Awake()
    {
        saveUnencrypted = PlayerPrefs.GetInt("SaveUnencrypted", 0) == 1;

        saveDataContainer.Load(saveUnencrypted);
        LoadFromContainer();

        StartCoroutine(AutoSaveLoop());
        StartCoroutine(UpgradeSaveLoop());

#if UNITY_WEBGL && !UNITY_EDITOR
        // Initialize browser quit detection for WebGL builds
        InitBrowserQuitDetection(gameObject.name);
#endif
    }

    public void UpgradeBought()
    {
        lastBoughtUpgrade = Time.time;
    }

    private IEnumerator UpgradeSaveLoop()
    {
        while (Application.isPlaying)
        {
            yield return new WaitForSeconds(0.5f);
            if (lastBoughtUpgrade.HasValue && (Time.time - lastBoughtUpgrade.Value) >= upgradeDebounceTime)
            {
                Save();
                lastBoughtUpgrade = null;
                Debug.Log("Auto-saved after upgrade purchase.");
            }
        }
    }

    public void SaveImmediately()
    {
        Debug.Log("SaveImmediately");
        Save();
    }

    private IEnumerator AutoSaveLoop()
    {
        while (Application.isPlaying)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            Save();
        }
    }

    public void SetEncryption(bool isEncrypted)
    {
        saveUnencrypted = !isEncrypted;
        PlayerPrefs.SetInt("SaveUnencrypted", saveUnencrypted ? 1 : 0);
    }

    private void LoadFromContainer()
    {
        gain.Value = saveDataContainer.Gain;
        totalGain.Value = saveDataContainer.TotalGain;
        resetCoin.Value = saveDataContainer.ResetCoin;
        resetStage.Value = saveDataContainer.ResetStage;
        quitDate.Value = DateTime.Now - saveDataContainer.QuitDate;

        foreach (var upgrade in resetUpgrades.ResetUpgrades)
        {
            upgrade.SetPurchased(saveDataContainer.ResetUpgrades.GetValueOrDefault(upgrade.name, false));
        }
        foreach (var upgrade in passiveSkills.PassiveSkills)
        {
            upgrade.SetPurchased(saveDataContainer.PassiveSkills.GetValueOrDefault(upgrade.name, false));
        }
        foreach (var upgrade in clickUpgrades.Upgrades)
        {
            upgrade.SetLevel(saveDataContainer.ClickUpgrades.GetValueOrDefault(upgrade.name, 0));
        }
        foreach (var upgrade in idleUpgrades.Upgrades)
        {
            upgrade.SetLevel(saveDataContainer.IdleUpgrades.GetValueOrDefault(upgrade.name, 0));
            if (upgrade.IdleUpgradeDetails != null)
            {
                upgrade.IdleUpgradeDetails.CurrentProgress = saveDataContainer.IdleCurrentProgress.GetValueOrDefault(upgrade.name, 0);
            }
        }

        GameController.Instance.SetIsTutorialFinished(saveDataContainer.IsTutorialFinished);
        GameController.Instance.IsFirstIdleUnlocked = saveDataContainer.IsFirstIdleUnlocked;
    }

    public void LoadFromClipboard()
    {
        if (GUIUtility.systemCopyBuffer.Length > 0)
        {
            try
            {
                string json = GUIUtility.systemCopyBuffer;
                saveDataContainer.LoadJson(json);
                LoadFromContainer();
                gainChangedEvent.Raise(new GainChangedEventDetails
                {
                    NewGain = gain.Value, ChangeType = GainChangeType.SaveLoadFromClipboard,
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load save data from clipboard: {ex.Message}");
            }
        }
    }

    public void CopySaveToClipboard()
    {
        Save();
        string json = saveDataContainer.SaveToJson();
        GUIUtility.systemCopyBuffer = json;
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
            ResetStage = resetStage.Value,
            QuitDate = DateTime.Now,
            ClickUpgrades = clickUpgrades.Upgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.currentLevel),
            IdleUpgrades = idleUpgrades.Upgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.currentLevel),
            ResetUpgrades = resetUpgrades.ResetUpgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.isPurchased),
            PassiveSkills = passiveSkills.PassiveSkills.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.IsPurchased),
            IdleCurrentProgress = idleUpgrades.Upgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.IdleUpgradeDetails.CurrentProgress),
            IsTutorialDone = GameController.Instance.IsTutorialFinished(),
            IsFirstIdleUnlocked = GameController.Instance.IsFirstIdleUnlocked
        };

        saveDataContainer.Save(saveData, saveUnencrypted);
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void InitBrowserQuitDetection(string gameObjectName);

    public void OnBrowserQuitting(string message)
    {
        Debug.Log("Browser quit detected, saving immediately...");
        SaveImmediately();
    }
#endif

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

    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        saveDataContainer.DeleteSave();
    }
#endif

}

public struct SaveData
{
    public double Gain;
    public double TotalGain;
    public double ResetCoin;
    public double ResetStage;
    public DateTime QuitDate;
    public Dictionary<string, int> ClickUpgrades;
    public Dictionary<string, int> IdleUpgrades;
    public Dictionary<string, bool> ResetUpgrades;
    public Dictionary<string, bool> PassiveSkills;
    public Dictionary<string, double> IdleCurrentProgress;
    public bool IsTutorialDone;
    public bool IsFirstIdleUnlocked;
}
