using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.IO;
using System.Runtime.InteropServices;
#endif


public class SaveHandler : MonoBehaviour
{
    [SerializeField]
    private SaveDataContainer saveDataContainer;

    [SerializeField] private GameState gameState;

    [SerializeField]
    private UpgradeList clickUpgrades;
    [SerializeField]
    private UpgradeList idleUpgrades;
    [SerializeField]
    private ResetUpgradeList resetUpgrades;
    [SerializeField]
    private PassiveSkillList passiveSkills;
    [SerializeField]
    private LargeNumber resetCoin;
    [SerializeField]
    private QuitDate quitDate;
    [SerializeField]
    private LargeNumber resetStage;
    
    [SerializeField]
    private BoolVariable isTutorialFinished;

    [SerializeField]
    private GameEvent gainChangedEvent;

    [SerializeField]
    private float autoSaveInterval = 60f;

    [SerializeField]
    private float upgradeDebounceTime = 5f;
    private float? lastBoughtUpgrade;

    [SerializeField]
    private bool saveEncrypted;

    [SerializeField]
    private AudioController audioController;
    [SerializeField]
    private GameController gameController;

#if UNITY_WEBGL && !UNITY_EDITOR
    private static string persistentDataPath = null;
#endif

    private void Awake()
    {
        saveDataContainer.Load(saveEncrypted);
        LoadFromContainer();

        StartCoroutine(AutoSaveLoop());
        StartCoroutine(UpgradeSaveLoop());

#if UNITY_WEBGL && !UNITY_EDITOR
        // Initialize browser quit detection for WebGL builds
        InitBrowserQuitDetection(gameObject.name);
#endif
    }

    static public string GetPersistentDataPath()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (persistentDataPath == null)
        {
            persistentDataPath = "/idbfs/noclicknogain.kritigames.com/";
            Directory.CreateDirectory(persistentDataPath);
        }
        return persistentDataPath;
#else
        return Application.persistentDataPath;
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

    private void LoadFromContainer()
    {
        gameState.LoadSave(saveDataContainer);
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

        isTutorialFinished.Value = saveDataContainer.IsTutorialFinished;
        audioController.PlayMusic(gameController.IsBeastModeBought());
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
                    NewGain = gameState.Gain, ChangeType = GainChangeType.SaveLoadFromClipboard,
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
        if (UIController.IsClaimed == false)
        {
            Debug.Log("Save skipped because reward is unclaimed.");
            return;
        }
        
        SaveData saveData = new() 
        {
            Gain = gameState.Gain,
            TotalGain = gameState.TotalGain,
            ResetCoin = resetCoin.Value,
            ResetStage = resetStage.Value,
            QuitDate = DateTime.Now,
            ClickUpgrades = clickUpgrades.Upgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.currentLevel),
            IdleUpgrades = idleUpgrades.Upgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.currentLevel),
            ResetUpgrades = resetUpgrades.ResetUpgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.isPurchased),
            PassiveSkills = passiveSkills.PassiveSkills.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.IsPurchased),
            IdleCurrentProgress = idleUpgrades.Upgrades.ToDictionary(upgrade => upgrade.name, upgrade => upgrade.IdleUpgradeDetails.CurrentProgress),
            IsTutorialDone = isTutorialFinished.Value,
        };

        saveDataContainer.Save(saveData, saveEncrypted);
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

#endif
    public void ResetSave()
    {
        saveDataContainer.DeleteSave(false);
        PlayerPrefs.DeleteKey("Tutorial.Step");
        PlayerPrefs.DeleteKey("Tutorial.Mask");
    }
    
    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        saveDataContainer.DeleteSave(true);
    }

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
}
