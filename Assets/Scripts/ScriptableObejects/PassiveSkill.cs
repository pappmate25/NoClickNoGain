using UnityEngine;

[CreateAssetMenu(fileName = "PassiveSkill", menuName = "SO/Configuration/PassiveSkill")]
public class PassiveSkill : ScriptableObject
{
    public string Name;
    public string Description;
    public int Price;           //it costs reset coin
    internal bool IsPurchased;
    public double CoolDown;

    public void SetPurchased(bool purchased)
    {
        IsPurchased = purchased;
    }
}
