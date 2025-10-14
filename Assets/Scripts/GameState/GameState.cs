using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "SO/GameState")]
public class GameState : ScriptableObject
{
    public static readonly long[] RequiredTotalGain = { 30000000, 20000000000, 235000000000000 };

    [SerializeField] private UpgradeList idleUpgradeList;
    [SerializeField] private UpgradeList clickUpgradeList;
    [SerializeField] private ResetUpgradeList resetUpgradeList;
    [SerializeField] private PassiveSkillList passiveSkillList;
    
    private UpgradeListContainer idleUpgrades;
    private UpgradeListContainer clickUpgrades;
    
    [SerializeField] private QuitDate quitDate;
    [SerializeField] private Upgrade beastModeUpgrade;
    [SerializeField] private BoolVariable isTutorialFinished;

    private double gain;
    private double totalGain;
    private double idleGainWhileAway;
    private double resetCoin;
    private int resetStage;

    [Header("Events")]
    [SerializeField] private GameEvent gainChangedEvent;
    [SerializeField] private GameEvent upgradeBoughtEvent;
    [SerializeField] private GameEvent resetEvent;
    [SerializeField] private GameEvent resetUpgradeBoughtEvent;
    [SerializeField] private GameEvent passiveSkillBoughtEvent;

    public double Gain => gain;
    public double TotalGain => totalGain;
    public double IdleGainWhileAway => idleGainWhileAway;
    public double ClickGainAmount => 1 + clickUpgrades.EffectSum;
    public bool IsFirstIdleUnlocked => idleUpgrades.IsAnyUnlocked();

    public bool IsBeastModeBought
        => beastModeUpgrade.currentLevel > 0;

    public bool CanPrestige => clickUpgrades.LevelSum >= 1350 && resetStage == 3;
    public double ResetCoin => resetCoin;
    public int ResetStage => resetStage;

    public void Initialize(SaveDataContainer saveDataContainer)
    {
        gain = saveDataContainer.Gain;
        totalGain = saveDataContainer.TotalGain;
        resetStage = (int)saveDataContainer.ResetStage;
        resetCoin = saveDataContainer.ResetCoin;

        foreach (var upgrade in resetUpgradeList.ResetUpgrades)
        {
            upgrade.SetPurchased(saveDataContainer.ResetUpgrades.GetValueOrDefault(upgrade.name, false));
        }

        foreach (var upgrade in passiveSkillList.PassiveSkills)
        {
            upgrade.SetPurchased(saveDataContainer.PassiveSkills.GetValueOrDefault(upgrade.name, false));
        }

        foreach (var upgrade in clickUpgradeList.Upgrades)
        {
            upgrade.SetLevel(saveDataContainer.ClickUpgrades.GetValueOrDefault(upgrade.name, 0));
        }

        foreach (var upgrade in idleUpgradeList.Upgrades)
        {
            upgrade.SetLevel(saveDataContainer.IdleUpgrades.GetValueOrDefault(upgrade.name, 0));
            if (upgrade.IdleUpgradeDetails != null)
            {
                upgrade.IdleUpgradeDetails.CurrentProgress =
                    saveDataContainer.IdleCurrentProgress.GetValueOrDefault(upgrade.name, 0);
            }
        }

        idleUpgrades = new UpgradeListContainer(idleUpgradeList.Upgrades);
        clickUpgrades = new UpgradeListContainer(clickUpgradeList.Upgrades);
        
        quitDate.Value = DateTime.Now - saveDataContainer.QuitDate;
        isTutorialFinished.Value = saveDataContainer.IsTutorialFinished;
        idleGainWhileAway = idleUpgrades.GetIdleGainFromDate(DateTime.Now - saveDataContainer.QuitDate);
    }

    public void Update()
    {
        double[] result = idleUpgrades.ProgressIdleState();

        for (byte i = 0; i < result.Length; i++)
        {
            if (result[i] > 0)
            {
                AddGain(result[i], GainChangeType.Idle, idleUpgradeIndex: i);
            }
        }
    }

    public void Click()
    {
        AddGain(ClickGainAmount, GainChangeType.Click);
    }

    public void AddGain(double amount, GainChangeType reason, byte idleUpgradeIndex = 0)
    {
        gain += amount;
        totalGain += amount;

        gainChangedEvent.Raise(new GainChangedEventDetails
        {
            NewGain = gain, ChangeType = reason, ChangeAmount = amount, IdleIndex = idleUpgradeIndex
        });
    }

    public void Reset()
    {
        gain = 0;
        totalGain = 0;
        resetStage++;
        GetResetCoin();

        idleUpgrades.Reset();
        clickUpgrades.Reset();

        resetEvent.Raise(new ResetEventDetails { GainOnReset = gain });
        gainChangedEvent.Raise(new GainChangedEventDetails
        {
            NewGain = gain, ChangeType = GainChangeType.Reset, ChangeAmount = -gain
        });
    }

    /// <summary>
    /// Tries to buy the specified upgrade and level it up to the target level if successful.
    /// </summary>
    /// <param name="boughtUpgrade">The upgrade that should be leveled up.</param>
    /// <param name="targetLevel">The level to upgrade to.</param>
    /// <returns>Whether buying the upgrade was successful.</returns>
    public bool BuyUpgrade(Upgrade boughtUpgrade, int targetLevel)
    {
        double upgradeCost = NumberFormatter.RoundCalculatedNumber(boughtUpgrade.GetCumulativeCost(targetLevel));
        double roundedGain = NumberFormatter.RoundCalculatedNumber(gain);

        if (upgradeCost > roundedGain)
        {
            return false;
        }

        boughtUpgrade.SetLevel(targetLevel);
        gain -= upgradeCost;
        gainChangedEvent.Raise(new GainChangedEventDetails
        {
            NewGain = gain, ChangeType = GainChangeType.UpgradeBought, ChangeAmount = -upgradeCost
        });

        upgradeBoughtEvent.Raise(new UpgradeBought { TargetLevel = targetLevel, Upgrade = boughtUpgrade });
        return true;
    }

    public void BuyResetUpgrade(ResetUpgrade resetUpgrade)
    {
        resetUpgrade.SetPurchased(true);

        ResetUpgradeBought details = new() { ResetUpgrade = resetUpgrade, };

        resetUpgradeBoughtEvent.Raise(details);
    }

    public void BuyPassiveSkill(PassiveSkill passiveSkill)
    {
        if (resetCoin >= passiveSkill.Price)
        {
            resetCoin -= passiveSkill.Price;
        }

        passiveSkill.SetPurchased(true);

        PassiveSkillBought details = new() { PassiveSkill = passiveSkill };

        passiveSkillBoughtEvent.Raise(details);
    }

    public void GetResetCoin()
    {
        double calc = Math.Ceiling(totalGain / 2500);

        resetCoin += calc;
    }

    public void ResetIdleProgress()
    {
        foreach (Upgrade upgrade in idleUpgradeList.Upgrades)
        {
            upgrade.IdleUpgradeDetails.CurrentProgress = 0;
        }
    }

    public bool CanReset()
    {
        int currentResetStage = resetStage;

        if (currentResetStage >= RequiredTotalGain.Length)
            return false;

        return totalGain >= RequiredTotalGain[currentResetStage];
    }
}
