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
    public bool IsSFXMuted => saveData.IsSFXMuted;
    public bool IsMusicMuted => saveData.IsMusicMuted;

    private string binPath;
    private string jsonPath;

    public void OnEnable()
    {
        binPath = Path.Combine(Application.persistentDataPath, "savefile.bin");
        jsonPath = Path.Combine(Application.persistentDataPath, "savefile.json");
    }

    public void Load(bool loadUnencrypted = false)
    {
        string pathToSaveFile = loadUnencrypted ? jsonPath : binPath;
        if (File.Exists(pathToSaveFile))
        {
            try
            {
                if (loadUnencrypted)
                {
                    string json = File.ReadAllText(pathToSaveFile);
                    saveData = JsonConvert.DeserializeObject<SaveData>(json);
                }
                else
                {
                    byte[] encryptedBytes = File.ReadAllBytes(pathToSaveFile);
                    string json = EncryptionHelper.DecryptStringAesCbc(encryptedBytes);
                    saveData = JsonConvert.DeserializeObject<SaveData>(json);
                }
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load save data: {ex.Message}");
            }
        }
        PlayerPrefs.DeleteKey("Tutorial.Step");
        PlayerPrefs.DeleteKey("Tutorial.Mask");
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
            IdleCurrentProgress= new Dictionary<string, double>(),
            IsTutorialDone = false,
            IsSFXMuted = false,
            IsMusicMuted = false
        };
    }

    public void Save(SaveData _saveData, bool saveUnencrypted = false)
    {
        saveData = _saveData;
        string pathToSaveFile = saveUnencrypted ? jsonPath : binPath;
        string json = JsonConvert.SerializeObject(_saveData);
        if (saveUnencrypted)
        {
            File.WriteAllText(pathToSaveFile, json);
        }
        else
        {
            byte[] encryptedBytes = EncryptionHelper.EncryptStringAesCbc(json);
            File.WriteAllBytes(pathToSaveFile, encryptedBytes);
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
    
    public void DeleteSave(bool deletePlayerPrefs)
    {
        if (File.Exists(binPath))
        {
            File.Delete(binPath);
        }
        if (File.Exists(jsonPath))
        {
            File.Delete(jsonPath);
        }
        
        if (deletePlayerPrefs)
        {
            PlayerPrefs.DeleteAll();
        }
    }

    public void LoadJson(string json)
    {
        try
        {
            saveData = JsonConvert.DeserializeObject<SaveData>(json);
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
                IsSFXMuted = false,
                IsMusicMuted = false
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
