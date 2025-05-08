using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(fileName = "ResetUpgrade", menuName = "SO/Configuration/ResetUpgrade")]
public class ResetUpgrade : ScriptableObject
{
    public string Name;
    public string Description;
    public int Multiplier;
    public int Cost;

    internal bool isPurchased;

    public Upgrade Upgrade;

    public void SetPurchased(bool purchased)
    {
        Upgrade.SetMultipliedBaseValue(purchased ? Multiplier : 1);
        isPurchased = purchased;
    }
}
