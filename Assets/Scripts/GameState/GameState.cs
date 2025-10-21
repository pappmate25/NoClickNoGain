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

    [Header("Events")]
    [SerializeField] private GameEvent gainChangedEvent;
    [SerializeField] private GameEvent upgradeBoughtEvent;
    [SerializeField] private GameEvent resetEvent;
    [SerializeField] private GameEvent resetUpgradeBoughtEvent;
    [SerializeField] private GameEvent passiveSkillBoughtEvent;

    public double Gain { get; private set; }
    public double TotalGain { get; private set; }
    public double IdleGainWhileAway { get; private set; }
    public double ResetCoin { get; private set; }
    public int ResetStage { get; private set; }

    public double ClickGainAmount => 1 + clickUpgrades.EffectSum;
    public bool IsFirstIdleUnlocked => idleUpgrades.IsAnyUnlocked();
    public bool IsBeastModeBought => beastModeUpgrade.currentLevel > 0;
    public bool CanReset =>
        ResetStage < RequiredTotalGain.Length && TotalGain >= RequiredTotalGain[ResetStage];

    public bool CanPrestige => clickUpgrades.LevelSum >= 1350 && ResetStage == 3;

    #region Init, Update

    public void Initialize(SaveDataContainer saveDataContainer)
    {
        Gain = saveDataContainer.Gain;
        TotalGain = saveDataContainer.TotalGain;
        ResetStage = (int)saveDataContainer.ResetStage;
        ResetCoin = saveDataContainer.ResetCoin;

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
        IdleGainWhileAway = idleUpgrades.GetIdleGainFromDate(DateTime.Now - saveDataContainer.QuitDate);
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

    #endregion

    public void Click()
    {
        AddGain(ClickGainAmount, GainChangeType.Click);
    }

    public void ClaimIdleGainWhileAway()
    {
        AddGain(IdleGainWhileAway, GainChangeType.WelcomeBackClaimed);
    }

    private void AddGain(double amount, GainChangeType reason, byte idleUpgradeIndex = 0)
    {
        Gain += amount;
        TotalGain += amount;

        gainChangedEvent.Raise(new GainChangedEventDetails
        {
            NewGain = Gain, ChangeType = reason, ChangeAmount = amount, IdleIndex = idleUpgradeIndex
        });
    }

    public void Reset()
    {
        Gain = 0;
        TotalGain = 0;
        ResetStage++;
        ResetCoin += Math.Ceiling(TotalGain / 2500);

        idleUpgrades.Reset();
        clickUpgrades.Reset();

        resetEvent.Raise(new ResetEventDetails { GainOnReset = Gain });
        gainChangedEvent.Raise(new GainChangedEventDetails
        {
            NewGain = Gain, ChangeType = GainChangeType.Reset, ChangeAmount = -Gain
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
        double roundedGain = NumberFormatter.RoundCalculatedNumber(Gain);

        if (upgradeCost > roundedGain)
        {
            return false;
        }

        boughtUpgrade.SetLevel(targetLevel);
        Gain -= upgradeCost;
        gainChangedEvent.Raise(new GainChangedEventDetails
        {
            NewGain = Gain, ChangeType = GainChangeType.UpgradeBought, ChangeAmount = -upgradeCost
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
        if (ResetCoin < passiveSkill.Price)
            return;
        
        ResetCoin -= passiveSkill.Price;
        
        passiveSkill.SetPurchased(true);

        PassiveSkillBought details = new() { PassiveSkill = passiveSkill };
        passiveSkillBoughtEvent.Raise(details);
    }

    public void ResetIdleProgress()
    {
        foreach (Upgrade upgrade in idleUpgradeList.Upgrades)
        {
            upgrade.IdleUpgradeDetails.CurrentProgress = 0;
        }
    }
}
