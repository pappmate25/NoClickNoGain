using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "ScriptableObjects/GameState")]
public class GameState: ScriptableObject
{
    [SerializeField] private double gain;
    [SerializeField] private double totalGain;
    
    [Header("Events")]
    [SerializeField] private GameEvent gainChanged;
    [SerializeField] private GameEvent upgradeBought;
    [SerializeField] private GameEvent resetEvent;

    public double Gain => gain;
    public double TotalGain => totalGain;

    public void LoadSave(SaveDataContainer saveDataContainer)
    {
        gain = saveDataContainer.Gain;
        totalGain = saveDataContainer.TotalGain;
    }
    
    public void AddGain(double amount, GainChangeType reason)
    {
        gain += amount;
        totalGain += amount;
        
        gainChanged.Raise(new GainChangedEventDetails
        {
            NewGain = gain,
            ChangeType = reason
        });
    }

    public void Reset()
    {
        resetEvent.Raise(new ResetEventDetails
        {
            GainOnReset = gain 
        });
        
        gain = 0;
        totalGain = 0;
        
        gainChanged.Raise(new GainChangedEventDetails
        {
            NewGain = gain,
            ChangeType = GainChangeType.Reset
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
        gainChanged.Raise(new GainChangedEventDetails
        {
            NewGain = gain,
            ChangeType = GainChangeType.UpgradeBought
        });

        upgradeBought.Raise(new UpgradeBought
        {
            TargetLevel = targetLevel,
            Upgrade = boughtUpgrade
        });
        return true;

    }
}
