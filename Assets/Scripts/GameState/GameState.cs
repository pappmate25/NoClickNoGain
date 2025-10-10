using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "SO/GameState")]
public class GameState : ScriptableObject
{
    [SerializeField] private UpgradeList idleUpgradeList;
    [SerializeField] private UpgradeList clickUpgradeList;
    private UpgradeListContainer idleUpgrades;
    private UpgradeListContainer clickUpgrades;

    private double gain;
    private double totalGain;
    private double idleGainWhileAway;

    [SerializeField] private LargeNumber resetStage;

    [Header("Events")] 
    [SerializeField] private GameEvent gainChanged;
    [SerializeField] private GameEvent upgradeBought;
    [SerializeField] private GameEvent resetEvent;

    public double Gain => gain;
    public double TotalGain => totalGain;
    public double IdleGainWhileAway => idleGainWhileAway;
    public double ClickGainAmount => 1 + clickUpgrades.EffectSum;
    public bool IsFirstIdleUnlocked => idleUpgrades.IsAnyUnlocked();
    public bool CanPrestige => clickUpgrades.LevelSum >= 1350 && resetStage.Value == 3;

    public void Initialize(SaveDataContainer saveDataContainer)
    {
        gain = saveDataContainer.Gain;
        totalGain = saveDataContainer.TotalGain;

        idleUpgrades = new UpgradeListContainer(idleUpgradeList.Upgrades);
        clickUpgrades = new UpgradeListContainer(clickUpgradeList.Upgrades);

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

        gainChanged.Raise(new GainChangedEventDetails
        {
            NewGain = gain, ChangeType = reason, ChangeAmount = amount, IdleIndex = idleUpgradeIndex
        });
    }

    public void Reset()
    {
        gain = 0;
        totalGain = 0;

        idleUpgrades.Reset();
        clickUpgrades.Reset();

        resetEvent.Raise(new ResetEventDetails { GainOnReset = gain });
        gainChanged.Raise(new GainChangedEventDetails { NewGain = gain, ChangeType = GainChangeType.Reset, ChangeAmount = -gain });
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
        gainChanged.Raise(new GainChangedEventDetails
        {
            NewGain = gain, ChangeType = GainChangeType.UpgradeBought, ChangeAmount = -upgradeCost
        });

        upgradeBought.Raise(new UpgradeBought { TargetLevel = targetLevel, Upgrade = boughtUpgrade });
        return true;
    }

    public void ResetIdleProgress()
    {
        foreach (Upgrade upgrade in idleUpgradeList.Upgrades)
        {
            upgrade.IdleUpgradeDetails.CurrentProgress = 0;
        }
    }
}
