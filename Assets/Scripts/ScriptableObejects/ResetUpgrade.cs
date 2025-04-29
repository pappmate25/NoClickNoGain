using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(fileName = "ResetUpgrade", menuName = "SO/Configuration/ResetUpgrade")]
public class ResetUpgrade : ScriptableObject
{
    public string Name;
    public string Description;
    public int Multiplier;
    public int Cost;

    public Upgrade Upgrade;
}
