using UnityEngine;

[CreateAssetMenu(fileName = "PassiveSkill", menuName = "SO/Configuration/PassiveSkill")]
public class PassiveSkill : ScriptableObject
{
    public string Name;
    public string Description;
    public int Price;           //costs reset coin
    public double CoolDown;
}
