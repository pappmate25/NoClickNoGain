using System.Collections;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class InGameTests
{
    public static string PreviousClipboardContents;
    
    [UnitySetUp]
    public IEnumerator Setup()
    {
        SceneManager.LoadScene("MainScene");
        yield return null;
        
        SaveHandler saveHandler = GameObject.Find("SaveHandler").GetComponent<SaveHandler>();
        
        Debug.Log(saveHandler);
        
        var testData = new SaveData()
        {
            Gain = 0,
            TotalGain = 0,
            ResetCoin = 0,
            ResetStage = 0,
            QuitDate = DateTime.Now,
            ClickUpgrades = new Dictionary<string, int>(),
            IdleUpgrades = new Dictionary<string, int> { { "", 0 } },
            ResetUpgrades = new Dictionary<string, bool>(),
            PassiveSkills = new Dictionary<string, bool>(),
            IdleCurrentProgress = new Dictionary<string, double>(),
            IsTutorialDone = false,
        };
        
        string json = JsonConvert.SerializeObject(testData);
        
        PreviousClipboardContents = GUIUtility.systemCopyBuffer;
        GUIUtility.systemCopyBuffer = json; 
        
        Debug.Log(json);
        
        saveHandler.LoadFromClipboard();
        GUIUtility.systemCopyBuffer = PreviousClipboardContents;
        
        Debug.Log("Setup complete with test save data.");
    }

    [UnityTest]
    public IEnumerator TestTest()
    {
        GameObject.Find("SaveHandler").GetComponent<SaveHandler>().CopySaveToClipboard();
        Debug.Log(GUIUtility.systemCopyBuffer);
        
        Assert.AreEqual(1, 1);
        yield return null;
    }
    

    [UnityOneTimeTearDown]
    public void Teardown()
    {
    }
}
