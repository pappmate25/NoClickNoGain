using System;
using System.Linq;
using UnityEngine;

[Serializable]
public struct UpgradeListContainer
{
    [SerializeField]
    private Upgrade[] upgrades;

    public UpgradeListContainer(Upgrade[] upgrades)
    {
        this.upgrades = upgrades;
    }

    public bool IsAnyUnlocked()
    {
        return upgrades.Any(upgrade => upgrade.currentLevel > 0);
    }

    public double GetIdleGainFromDate(TimeSpan elapsed)
    {
        if (elapsed.TotalSeconds < 0)
        {
            return 0;
        }

        double totalIdleGain = 0;
        double elapsedInSeconds = elapsed.TotalSeconds;

        foreach (Upgrade upgrade in upgrades)
        {
            double idleSkillAcquiredCount = Math.Floor(elapsedInSeconds / upgrade.IdleUpgradeDetails.ProgressDuration);
            totalIdleGain += upgrade.currentEffect * idleSkillAcquiredCount;
        }

        return totalIdleGain;
    }

    public void Reset()
    {
        foreach (var upgrade in upgrades)
        {
            upgrade.SetLevel(0);

            if (!upgrade.IsClickUpgrade)
            {
                upgrade.IdleUpgradeDetails.CurrentProgress = 0;
            }
        }
    }
    
    public double EffectSum => upgrades.Sum(upgrade => upgrade.currentEffect);
    public int LevelSum => upgrades.Sum(upgrade => upgrade.currentLevel);

    /// <summary>
    /// Progresses the idle upgrades and adds gain when an upgrade completes its progress.
    /// </summary>
    /// <returns>An array where each element belongs to one idle skill and describes the amount of gain that it provided.</returns>
    public double[] ProgressIdleState()
    {
        double[] idleGains = new double[upgrades.Length];
        
        for (int index = 0; index < upgrades.Length; index++)
        {
            Upgrade idleUpgrade = upgrades[index];
            if (idleUpgrade.currentLevel == 0)
            {
                continue;
            }

            IdleUpgradeDetails idleUpgradeDetails = idleUpgrade.IdleUpgradeDetails;

            idleUpgradeDetails.CurrentProgress += Time.deltaTime / idleUpgradeDetails.ProgressDuration;
            if (idleUpgradeDetails.CurrentProgress >= 1.0f)
            {
                idleUpgradeDetails.CurrentProgress -= 1.0f;
                idleGains[index] = idleUpgrade.currentEffect;
            }
        }
        
        return idleGains;
    }
}
