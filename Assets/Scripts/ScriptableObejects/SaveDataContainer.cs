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

    public void Load()
    {
        string pathToSaveFile = Path.Combine(Application.persistentDataPath, "savefile.json");
        if (File.Exists(pathToSaveFile))
        {
            string encryptedJson = File.ReadAllText(pathToSaveFile);
            try
            {
                string json = EncryptionHelper.DecryptStringAesCbc(encryptedJson);
                saveData = JsonConvert.DeserializeObject<SaveData>(json);
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to decrypt or deserialize save data: {ex.Message}");
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

    public void Save(SaveData _saveData)
    {
        saveData = _saveData;
        string pathToSaveFile = Path.Combine(Application.persistentDataPath, "savefile.json");
        string json = JsonConvert.SerializeObject(_saveData);
        string encryptedJson = EncryptionHelper.EncryptStringAesCbc(json);
        File.WriteAllText(pathToSaveFile, encryptedJson);
    }

    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        string pathToSaveFile = Path.Combine(Application.persistentDataPath, "savefile.json");
        if (File.Exists(pathToSaveFile))
        {
            File.Delete(pathToSaveFile);
        }
    }
}
