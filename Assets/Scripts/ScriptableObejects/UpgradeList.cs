using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeList", menuName = "SO/Configuration/UpgradeList")]
public class UpgradeList : ScriptableObject
{
    public Upgrade[] Upgrades;
}
