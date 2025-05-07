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
    
    public void Purchase()
    {
        if (isPurchased)
        {
            Debug.Log("Already purchased");
            return;
        }

        isPurchased = true;
        Upgrade.GetMultipliedBaseValue(Multiplier);
    }
}
