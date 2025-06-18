using System;
using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private FloatVariable AnimationSpeed;
    [SerializeField]
    private LargeNumber Gain;
    [SerializeField]
    private LargeNumber TotalGain;
    [SerializeField]
    private LargeNumber ResetCoin;
    [SerializeField]
    private UpgradeList ClickUpgrades;
    [SerializeField]
    private UpgradeList IdleUpgrades;
    [SerializeField]
    private ResetUpgradeList ResetUpgradesList;


    [SerializeField]
    private QuitDate QuitDate;
    [SerializeField]
    private LargeNumber IdleGain;

    [SerializeField]
    private GameEvent ClickEvent;
    [SerializeField]
    private GameEvent UpgradeBoughtEvent;
    [SerializeField]
    private GameEvent GainChangedEvent;

    public static GameController Instance { get; private set; }

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AnimationSpeed.Value = 10.0f;
        IdleGainCalc(QuitDate.Value);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Upgrade idleUpgrade in IdleUpgrades.Upgrades)
        {
            if (idleUpgrade.currentLevel == 0)
            {
                continue;
            }

            IdleUpgradeDetails idleUpgradeDetails = idleUpgrade.IdleUpgradeDetails;

            idleUpgradeDetails.CurrentProgress += Time.deltaTime / idleUpgradeDetails.ProgressDuration;
            if (idleUpgradeDetails.CurrentProgress >= 1.0f)
            {
                idleUpgradeDetails.CurrentProgress -= 1.0f;
                Gain.Value += idleUpgrade.currentEffect;
                TotalGain.Value += idleUpgrade.currentEffect;
                //Debug.Log("Gained " + IdleUpgrades.Upgrades[i].currentEffect + " points from idle upgrade " + IdleUpgrades.Upgrades[i].name);
                
                GainChangedEvent.Raise(NoDetails.Instance); 
            }
        }
    }

    private void IdleGainCalc(TimeSpan elapsed)
    {
        IdleGain.Value = 0;
        double elapsedInSeconds = elapsed.TotalSeconds;
        double idleSkillAcquiredCount;
        
        Upgrade[] upgrades = IdleUpgrades.Upgrades;       
        for (int i = 0; i < upgrades.Length; i++)
        {            
            if (Math.Floor(elapsedInSeconds / upgrades[i].IdleUpgradeDetails.ProgressDuration) >= 1)
            {
                idleSkillAcquiredCount = Math.Floor(elapsedInSeconds / upgrades[i].IdleUpgradeDetails.ProgressDuration);
                IdleGain.Value += upgrades[i].currentEffect*idleSkillAcquiredCount;
            }
        }
    }

    public void onClick()
    {
        double clickValue = 1;
        for (int i = 0; i < ClickUpgrades.Upgrades.Length; i++)
        {
            clickValue += ClickUpgrades.Upgrades[i].currentEffect;
        }
        Gain.Value += clickValue;
        TotalGain.Value += clickValue;
        Debug.Log(clickValue + " gain jött");
        
        GainChangedEvent.Raise(NoDetails.Instance);
    }

    public void OnUpgradeBought(IGameEventDetails details)
    {
        UpgradeBought upgradeBought = details as UpgradeBought;
        Upgrade upgrade = upgradeBought.Upgrade;
        double cost = upgrade.GetCumulativeCost(upgradeBought.TargetLevel);

        //kerekítés ellenőrzéskor, hogy passzoljon a kiírt értékhez és ne történhessen olyan hogy a gain 8.8m a skill 8.8M de még sem tudjuk megvásásrolni mert a háttrében kis eltérérs van
        if (NumberFormatter.RoundCalculatedNumber(cost) <= NumberFormatter.RoundCalculatedNumber(Gain.Value))
        {
            upgrade.SetLevel(upgradeBought.TargetLevel);
            Gain.Value -= NumberFormatter.RoundCalculatedNumber(cost);              //kivonás kerekítve, hogy ne legyen véletlen negatív érték a gain
            if (Gain.Value < 0)
            {
                Gain.Value = 0;
            }
            
            GainChangedEvent.Raise(NoDetails.Instance);
        }
    }

    public void OnResetUpdradeBought(IGameEventDetails details)
    {
        ResetUpgradeBought resetUpgradeBought = details as ResetUpgradeBought;
        ResetUpgrade resetUpgrade = resetUpgradeBought.ResetUpgrade;

        if (ResetCoin.Value >= resetUpgrade.Cost)
        {
            ResetCoin.Value -= resetUpgrade.Cost;
            resetUpgrade.SetPurchased(true);
        }
    }

    private void Reset()
    {
        Gain.Value = 0;
        TotalGain.Value = 0;
        
        GainChangedEvent.Raise(NoDetails.Instance);
        GameController.Instance.Resets_Upgrades(ClickUpgrades.Upgrades);
        GameController.Instance.Resets_Upgrades(IdleUpgrades.Upgrades);
    }

    public void Resets_Upgrades(Upgrade[] upgrades)
    {
        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].SetLevel(0);
        }
    }

    public void GetResetCoin()
    {
        double calc = Math.Ceiling(TotalGain.Value / 2500);

        GameController.Instance.ResetCoin.Value += calc;
    }
}
