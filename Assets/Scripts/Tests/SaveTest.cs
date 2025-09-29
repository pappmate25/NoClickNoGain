using System;
using NUnit.Framework;
using UnityEngine;

public class SaveTest
{
    [Test]
    public void SaveMissingDictionaryIsNotNull()
    {
        string testJson = @"{""Gain"":4608652.0,""TotalGain"":4668692.0,""ResetCoin"":0.0,""ResetStage"":0.0,""QuitDate"":""2025-09-25T13:52:19.4710847+02:00"",""IsTutorialDone"":false,""IsSFXMuted"":false,""IsMusicMuted"":false}";

        var saveDataContainer = ScriptableObject.CreateInstance<SaveDataContainer>();

        saveDataContainer.LoadJson(testJson);

        Assert.IsNotNull(saveDataContainer.ClickUpgrades);
        Assert.IsNotNull(saveDataContainer.IdleUpgrades);
        Assert.IsNotNull(saveDataContainer.ResetUpgrades);
        Assert.IsNotNull(saveDataContainer.PassiveSkills);
        Assert.IsNotNull(saveDataContainer.IdleCurrentProgress);
        
        Assert.AreEqual(4608652.0, saveDataContainer.Gain);
        Assert.AreEqual(4668692.0, saveDataContainer.TotalGain);
        Assert.AreEqual(0.0, saveDataContainer.ResetCoin);
        Assert.AreEqual(0.0, saveDataContainer.ResetStage);
    }

    [Test]
    public void SaveMissingDateTime()
    {
        string saveWithoutDateTime =        
            @"{""Gain"":8367511.0,""TotalGain"":8427551.0,""ResetCoin"":0.0,""ResetStage"":0.0,""ClickUpgrades"":{""Right Technique"":10,""MealPrep"":0,""ProteinPowder"":0,""Creatine"":0,""Steroid"":0},""IdleUpgrades"":{""Training Clothes"":8,""Gym Playlist"":4,""Personal Trainer"":0,""Vitamins"":0,""PreWorkout"":0},""ResetUpgrades"":{""InsaneTechnique 1"":false,""InsaneTechnique 2"":false,""InsaneTechnique 3"":false,""HealthyMealPrep 1"":false,""HealthyMealPrep 2"":false,""HealthyMealPrep 3"":false,""CoolBrandProteinPowder 1"":false,""CoolBrandProteinPowder 2"":false,""CoolBrandProteinPowder 3"":false,""CoolBrandCreatine 1"":false,""CoolBrandCreatine 2"":false,""CoolBrandCreatine 3"":false,""BeastSteroid 1"":false,""BeastSteroid 2"":false,""BeastSteroid 3"":false,""BeastModePlaylist 1"":false,""BeastModePlaylist 2"":false,""BeastModePlaylist 3"":false,""ExpensiveTrainingClothes 1"":false,""ExpensiveTrainingClothes 2"":false,""ExpensiveTrainingClothes 3"":false,""ProfessionalPersonalTrainer 1"":false,""ProfessionalPersonalTrainer 2"":false,""ProfessionalPersonalTrainer 3"":false,""QualityVitamins 1"":false,""QualityVitamins 2"":false,""QualityVitamins 3"":false,""CoolBrandPreWorkout 1"":false,""CoolBrandPreWorkout 2"":false,""CoolBrandPreWorkout 3"":false},""PassiveSkills"":{""SharpClicker"":false,""LegDay"":false,""PRSmash"":false,""AdrenalinePump"":false,""MuscleBrainConnection"":false,""BulkPhase"":false,""PowerNap"":false,""BeautySleep"":false,""RecoveryOn"":false,""NoTimeToWaste"":false,""SponsorshipDeal"":false},""IdleCurrentProgress"":{""Training Clothes"":0.529377337777983,""Gym Playlist"":0.67689044234184792,""Personal Trainer"":0.0,""Vitamins"":0.0,""PreWorkout"":0.0},""IsTutorialDone"":false,""IsSFXMuted"":true,""IsMusicMuted"":false}";
        
        var saveDataContainer = ScriptableObject.CreateInstance<SaveDataContainer>();
        saveDataContainer.LoadJson(saveWithoutDateTime);
        
        Assert.AreEqual(8367511.0, saveDataContainer.Gain);
        Assert.AreEqual(8427551.0, saveDataContainer.TotalGain);
        Assert.AreEqual(0.0, saveDataContainer.ResetCoin);
        Assert.AreEqual(0.0, saveDataContainer.ResetStage);
        
        Assert.IsNotNull(saveDataContainer.QuitDate);
        Assert.AreNotEqual(saveDataContainer.QuitDate, new DateTime(0001, 01, 01));
    }

    [Test]
    public void SaveWithExtraFields()
    {
        string jsonWithExtraFields =
            @"{""Gain"":8367511.0,""TotalGain"":8427551.0,""ResetCoin"":0.0,""ResetStage"":0.0,""QuitDate"":""2025-09-26T12:07:30.706517+02:00"",""ClickUpgrades"":{""Right Technique"":10,""MealPrep"":0,""ProteinPowder"":0,""Creatine"":0,""Steroid"":0},""IdleUpgrades"":{""Training Clothes"":8,""Gym Playlist"":4,""Personal Trainer"":0,""Vitamins"":0,""PreWorkout"":0},""ResetUpgrades"":{""InsaneTechnique 1"":false,""InsaneTechnique 2"":false,""InsaneTechnique 3"":false,""HealthyMealPrep 1"":false,""HealthyMealPrep 2"":false,""HealthyMealPrep 3"":false,""CoolBrandProteinPowder 1"":false,""CoolBrandProteinPowder 2"":false,""CoolBrandProteinPowder 3"":false,""CoolBrandCreatine 1"":false,""CoolBrandCreatine 2"":false,""CoolBrandCreatine 3"":false,""BeastSteroid 1"":false,""BeastSteroid 2"":false,""BeastSteroid 3"":false,""BeastModePlaylist 1"":false,""BeastModePlaylist 2"":false,""BeastModePlaylist 3"":false,""ExpensiveTrainingClothes 1"":false,""ExpensiveTrainingClothes 2"":false,""ExpensiveTrainingClothes 3"":false,""ProfessionalPersonalTrainer 1"":false,""ProfessionalPersonalTrainer 2"":false,""ProfessionalPersonalTrainer 3"":false,""QualityVitamins 1"":false,""QualityVitamins 2"":false,""QualityVitamins 3"":false,""CoolBrandPreWorkout 1"":false,""CoolBrandPreWorkout 2"":false,""CoolBrandPreWorkout 3"":false},""PassiveSkills"":{""SharpClicker"":false,""LegDay"":false,""PRSmash"":false,""AdrenalinePump"":false,""MuscleBrainConnection"":false,""BulkPhase"":false,""PowerNap"":false,""BeautySleep"":false,""RecoveryOn"":false,""NoTimeToWaste"":false,""SponsorshipDeal"":false},""IdleCurrentProgress"":{""Training Clothes"":0.529377337777983,""Gym Playlist"":0.67689044234184792,""Personal Trainer"":0.0,""Vitamins"":0.0,""PreWorkout"":0.0},""IsTutorialDone"":false,""IsSFXMuted"":true,""IsMusicMuted"":false, ""ExtraField"": ""ExtraValue"", ""AnotherExtraField"": 123}";

        var saveDataContainer = ScriptableObject.CreateInstance<SaveDataContainer>();
        saveDataContainer.LoadJson(jsonWithExtraFields);
        Assert.AreEqual(8367511.0, saveDataContainer.Gain);
        Assert.AreEqual(8427551.0, saveDataContainer.TotalGain);
        Assert.AreEqual(0.0, saveDataContainer.ResetCoin);
        Assert.AreEqual(0.0, saveDataContainer.ResetStage);
    }
}
