using System;
using System.Collections.Generic;
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
        saveData = new SaveData
        {
            Gain = 0,
            TotalGain = 0,
            ResetCoin = 0,
            ResetStage = 0,
            ClickUpgrades = new Dictionary<string, int>(),
            IdleUpgrades = new Dictionary<string, int>(),
            ResetUpgrades = new Dictionary<string, bool>()
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
    }

    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        if (File.Exists(binPath))
        {
            File.Delete(binPath);
        }
        if (File.Exists(jsonPath))
        {
            File.Delete(jsonPath);
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
                ClickUpgrades = new Dictionary<string, int>(),
                IdleUpgrades = new Dictionary<string, int>(),
                ResetUpgrades = new Dictionary<string, bool>()
            };
        }
    }

    public string SaveToJson()
    {
        return JsonConvert.SerializeObject(saveData);
    }
}
