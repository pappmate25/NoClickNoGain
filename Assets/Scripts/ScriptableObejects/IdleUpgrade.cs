using UnityEngine;

[CreateAssetMenu(fileName = "IdleUpgradeDetails", menuName = "SO/Configuration/IdleUpgrade")]
public class IdleUpgradeDetails : ScriptableObject
{
    public double CurrentProgress;
    public double ProgressDuration;
    public string IdleBarText;

    public void Awake()
    {
        CurrentProgress = 0;
    }
}
