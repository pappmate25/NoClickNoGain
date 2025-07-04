using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(fileName = "ResetUpgrade", menuName = "SO/Configuration/ResetUpgrade")]
public class ResetUpgrade : ScriptableObject
{
    public string Name;
    public string Description;
    public double Multiplier;
    public int Cost;
    public int Rank;

    internal bool isPurchased;

    public Upgrade Upgrade;

    public void SetPurchased(bool purchased)
    {
        Upgrade.SetResetMultiplier(purchased ? Multiplier : 1, Rank);
        isPurchased = purchased;
    }
}
