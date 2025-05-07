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
    public Dictionary<string, int> ClickUpgrades => saveData.ClickUpgrades;
    public Dictionary<string, int> IdleUpgrades => saveData.IdleUpgrades;
    public Dictionary<string, int> ResetUpgrades => saveData.ResetUpgrades;

    public void Load()
    {
        string pathToSaveFile = Path.Combine(Application.persistentDataPath, "savefile.json");
        if (File.Exists(pathToSaveFile))
        {
            string json = File.ReadAllText(pathToSaveFile);
            saveData = JsonConvert.DeserializeObject<SaveData>(json);
        }
    }

    public void Save(SaveData _saveData)
    {
        saveData = _saveData;

        string pathToSaveFile = Path.Combine(Application.persistentDataPath, "savefile.json");
        string json = JsonConvert.SerializeObject(_saveData);
        File.WriteAllText(pathToSaveFile, json);
    }
}
