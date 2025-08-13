using System;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public readonly long[] RequiredTotalGain = { 30000000, 20000000000, 235000000000000 };

    [SerializeField]
    private FloatVariable animationSpeed;
    [SerializeField]
    private LargeNumber gain;
    [SerializeField]
    private LargeNumber totalGain;
    [SerializeField]
    private LargeNumber resetCoin;
    [SerializeField]
    private UpgradeList clickUpgrades;
    [SerializeField]
    private UpgradeList idleUpgrades;
    [SerializeField]
    private ResetUpgradeList resetUpgradesList;

    [SerializeField]
    private Upgrade beastModeUpgrade;

    [SerializeField]
    private QuitDate quitDate;
    [SerializeField]
    private LargeNumber idleGain;


    [SerializeField]
    private LargeNumber resetStage;

    [SerializeField]
    private GameEvent clickEvent;
    [SerializeField]
    private GameEvent upgradeBoughtEvent;

    public static GameController Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
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
        animationSpeed.Value = 10.0f;
        IdleGainCalc(quitDate.Value);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Upgrade idleUpgrade in idleUpgrades.Upgrades)
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
                gain.Value += idleUpgrade.currentEffect;
                totalGain.Value += idleUpgrade.currentEffect;
                //Debug.Log("Gained " + IdleUpgrades.Upgrades[i].currentEffect + " points from idle upgrade " + IdleUpgrades.Upgrades[i].name);
            }
        }
    }

    public bool IsBeastModeBought()
        => beastModeUpgrade.currentLevel > 0;

    private void IdleGainCalc(TimeSpan elapsed)
    {
        idleGain.Value = 0;
        double elapsedInSeconds = elapsed.TotalSeconds;
        double idleSkillAcquiredCount;

        Upgrade[] upgrades = idleUpgrades.Upgrades;
        for (int i = 0; i < upgrades.Length; i++)
        {
            if (Math.Floor(elapsedInSeconds / upgrades[i].IdleUpgradeDetails.ProgressDuration) >= 1)
            {
                idleSkillAcquiredCount = Math.Floor(elapsedInSeconds / upgrades[i].IdleUpgradeDetails.ProgressDuration);
                idleGain.Value += upgrades[i].currentEffect * idleSkillAcquiredCount;
            }
        }
    }

    public void onClick()
    {
        double clickValue = 1;
        for (int i = 0; i < clickUpgrades.Upgrades.Length; i++)
        {
            clickValue += clickUpgrades.Upgrades[i].currentEffect;
        }
        gain.Value += clickValue;
        totalGain.Value += clickValue;
        Debug.Log(clickValue + " gain jött");
    }

    public void OnUpgradeBought(IGameEventDetails details)
    {
        UpgradeBought upgradeBought = details as UpgradeBought;
        Upgrade upgrade = upgradeBought.Upgrade;
        double cost = upgrade.GetCumulativeCost(upgradeBought.TargetLevel);

        //kerekítés ellenőrzéskor, hogy passzoljon a kiírt értékhez és ne történhessen olyan hogy a gain 8.8m a skill 8.8M de még sem tudjuk megvásásrolni mert a háttrében kis eltérérs van
        if (NumberFormatter.RoundCalculatedNumber(cost) <= NumberFormatter.RoundCalculatedNumber(gain.Value))
        {
            upgrade.SetLevel(upgradeBought.TargetLevel);
            gain.Value -= NumberFormatter.RoundCalculatedNumber(cost);              //kivonás kerekítve, hogy ne legyen véletlen negatív érték a gain
            if (gain.Value < 0)
            {
                gain.Value = 0;
            }
        }
    }

    public void OnResetUpdradeBought(IGameEventDetails details)
    {
        if (details is not ResetUpgradeBought resetUpgradeBought || resetUpgradeBought.ResetUpgrade == null)
        {
            Debug.LogWarning("ResetUpgradeBought event received with invalid or null data.");
            return;
        }
        ResetUpgrade resetUpgrade = resetUpgradeBought.ResetUpgrade;

        //if (ResetCoin.Value >= resetUpgrade.Cost)
        //{
        //    ResetCoin.Value -= resetUpgrade.Cost;
        //    resetUpgrade.SetPurchased(true);
        //}
        resetUpgrade.SetPurchased(true);
    }

    public void OnPassiveSkillBought(IGameEventDetails details)
    {
        PassiveSkillBought passiveSkillBought = details as PassiveSkillBought;
        PassiveSkill passiveSkill = passiveSkillBought.PassiveSkill;

        if(resetCoin.Value >= passiveSkill.Price)
        {
            resetCoin.Value -= passiveSkill.Price;
        }

        passiveSkill.SetPurchased(true);
        Debug.Log("lefutott a passive buy");
    }

    private void Reset()
    {
        gain.Value = 0;
        totalGain.Value = 0;

        GameController.Instance.Resets_Upgrades(clickUpgrades.Upgrades);
        GameController.Instance.Resets_Upgrades(idleUpgrades.Upgrades);
    }

    public void ResetIdleProgress()
    {
        foreach (var idleUpgrade in idleUpgrades.Upgrades)
        {
            idleUpgrade.IdleUpgradeDetails.CurrentProgress = 0;
        }
    }

    public void Resets_Upgrades(Upgrade[] upgrades)
    {
        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].SetLevel(0);
        }
    }

    public void GetResetCoin() //passzív skillekre lehet majd költeni
    {
        double calc = Math.Ceiling(totalGain.Value / 2500);

        GameController.Instance.resetCoin.Value += calc;
    }

    public bool CanReset()
    {
        int currentResetStage = GetResetStage();

        if (currentResetStage >= RequiredTotalGain.Length)
            return false;

        return totalGain.Value >= RequiredTotalGain[currentResetStage];
    }

    public void IncreaseResetStage()
    {
        resetStage.Value++;
    }

    public int GetResetStage()
    {
        return Convert.ToInt32(resetStage.Value);
    }

    public bool CanPrestige()
    {
        int totalLevel = clickUpgrades.Upgrades.Sum(upg => upg.currentLevel);

        if (totalLevel >= 1350 && resetStage.Value == 3)
        {
            return true;
        }

        return false;
    }

    public double GetClickValue()
    {
        double clickValue = 1;

        for (int i = 0; i < clickUpgrades.Upgrades.Length; i++)
        {
            clickValue += clickUpgrades.Upgrades[i].currentEffect;
        }

        return clickValue;
    }
}
