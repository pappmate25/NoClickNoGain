using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveDataContainer", menuName = "SO/SaveDataContainer")]
public class SaveDataContainer : ScriptableObject
{
    private SaveData saveData;

    public double Gain => saveData.Gain;
    public double TotalGain => saveData.TotalGain;
    public double ResetCoin => saveData.ResetCoin;
    public double ResetStage => saveData.ResetStage;
    public DateTime QuitDate => saveData.QuitDate;
    public Dictionary<string, int> ClickUpgrades => saveData.ClickUpgrades;
    public Dictionary<string, int> IdleUpgrades => saveData.IdleUpgrades;
    public Dictionary<string, bool> ResetUpgrades => saveData.ResetUpgrades;
    public Dictionary<string, bool> PassiveSkills => saveData.PassiveSkills;
    public Dictionary<string, double> IdleCurrentProgress => saveData.IdleCurrentProgress;
    public bool IsTutorialFinished => saveData.IsTutorialDone;

    private string binPath;
    private string jsonPath;

    public void OnEnable()
    {
        binPath = Path.Combine(SaveHandler.GetPersistentDataPath(), "savefile.bin");
        jsonPath = Path.Combine(SaveHandler.GetPersistentDataPath(), "savefile.json");
    }

    public void Load(bool loadEncrypted)
    {
        string pathToSaveFile = loadEncrypted ? binPath : jsonPath;
        if (File.Exists(pathToSaveFile))
        {
            try
            {
                if (loadEncrypted)
                {
                    byte[] encryptedBytes = File.ReadAllBytes(pathToSaveFile);
                    string json = EncryptionHelper.DecryptStringAesCbc(encryptedBytes);
                    LoadJson(json);
                }
                else
                {
                    string json = File.ReadAllText(pathToSaveFile);
                    LoadJson(json);
                }
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load save data: {ex.Message}");
            }
        }
        TutorialController.ResetTutorialSteps();
        ConfigurationHandler.Save();
        saveData = new SaveData
        {
            Gain = 0,
            TotalGain = 0,
            ResetCoin = 0,
            ResetStage = 0,
            QuitDate = DateTime.Now,
            ClickUpgrades = new Dictionary<string, int>(),
            IdleUpgrades = new Dictionary<string, int>(),
            ResetUpgrades = new Dictionary<string, bool>(),
            PassiveSkills = new Dictionary<string, bool>(),
            IdleCurrentProgress = new Dictionary<string, double>(),
            IsTutorialDone = false,
        };
    }

    public void Save(SaveData _saveData, bool saveEncrypted)
    {
        saveData = _saveData;
        string pathToSaveFile = saveEncrypted ? binPath: jsonPath;
        string json = JsonConvert.SerializeObject(_saveData);
        if (saveEncrypted)
        {
            byte[] encryptedBytes = EncryptionHelper.EncryptStringAesCbc(json);
            File.WriteAllBytes(pathToSaveFile, encryptedBytes);
        }
        else
        {
            File.WriteAllText(pathToSaveFile, json);
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        // https://gamedev.stackexchange.com/questions/184369/file-saved-to-indexeddb-lost-unless-we-change-scenes
        //flush our changes to IndexedDB
        SyncDB();
#endif
    }

    [ContextMenu("Delete Save")]
    public void DeleteSaveContextMenu()
    {
        DeleteSave(true);
    }

    public void DeleteSave(bool deleteConfiguration)
    {
        File.Delete(binPath);
        File.Delete(jsonPath);

        if (deleteConfiguration)
        {
            ConfigurationHandler.Delete();
        }
    }

    public void LoadJson(string json)
    {
        try
        {
            saveData = JsonConvert.DeserializeObject<SaveData>(json);
            
            saveData.ClickUpgrades ??= new Dictionary<string, int>();
            saveData.IdleUpgrades ??= new Dictionary<string, int>();
            saveData.ResetUpgrades ??= new Dictionary<string, bool>();
            saveData.PassiveSkills ??= new Dictionary<string, bool>();
            saveData.IdleCurrentProgress ??= new Dictionary<string, double>();
            
            if (saveData.QuitDate == default)
            {
                saveData.QuitDate = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to deserialize JSON: {ex.Message}");
            saveData = new SaveData
            {
                Gain = 0,
                TotalGain = 0,
                ResetCoin = 0,
                ResetStage = 0,
                QuitDate = DateTime.Now,
                ClickUpgrades = new Dictionary<string, int>(),
                IdleUpgrades = new Dictionary<string, int>(),
                ResetUpgrades = new Dictionary<string, bool>(),
                PassiveSkills = new Dictionary<string, bool>(),
                IdleCurrentProgress = new Dictionary<string, double>(),
                IsTutorialDone = false,
            };
        }
    }

    public string SaveToJson()
    {
        return JsonConvert.SerializeObject(saveData);
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SyncDB();
#endif
}
