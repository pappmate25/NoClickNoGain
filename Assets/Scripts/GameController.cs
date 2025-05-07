using System;
using System.IO.IsolatedStorage;
using UnityEditor.Rendering.Universal;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private FloatVariable AnimationSpeed;
    [SerializeField]
    private LargeNumber Gain;
    [SerializeField]
    private LargeNumber TotalGain;
    [SerializeField]
    private LargeNumber ResetCoin;
    [SerializeField]
    private UpgradeList ClickUpgrades;
    [SerializeField]
    private UpgradeList IdleUpgrades;
    [SerializeField]
    private ResetUpgradeList ResetUpgradesList;

    [SerializeField]
    private GameEvent ClickEvent;
    [SerializeField]
    private GameEvent UpgradeBoughtEvent;


    public static GameController Instance { get; private set; }

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AnimationSpeed.Value = 10.0f;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Upgrade idleUpgrade in IdleUpgrades.Upgrades)
        {
            if (idleUpgrade.currentLevel == 0)
            {
                continue;
            }

            IdleUpgradeDetails idleUpgradeDetails = idleUpgrade.IdleUpgradeDetails;
            
            idleUpgradeDetails.CurrentProgress += Time.deltaTime / idleUpgradeDetails.ProgressDuration;
            if (idleUpgradeDetails.CurrentProgress >= 1.0f)
            {
                idleUpgradeDetails.CurrentProgress -= 1.0f;
                Gain.Value += idleUpgrade.currentEffect;
                TotalGain.Value += idleUpgrade.currentEffect;
                //Debug.Log("Gained " + IdleUpgrades.Upgrades[i].currentEffect + " points from idle upgrade " + IdleUpgrades.Upgrades[i].name);
            }
        }
    }

    public void onClick()
    {
        double clickValue = 1;
        for (int i = 0; i < ClickUpgrades.Upgrades.Length; i++)
        {
            clickValue += ClickUpgrades.Upgrades[i].currentEffect;
        }
        Gain.Value += clickValue;
        TotalGain.Value += clickValue;
        Debug.Log(clickValue + " gain jött");
    }

    public void OnUpgradeBought(IGameEventDetails details)
    {
        UpgradeBought upgradeBought = details as UpgradeBought;
        Upgrade upgrade = upgradeBought.Upgrade;
        double cost = upgrade.GetCumulativeCost(upgradeBought.TargetLevel);
        if (cost <= Gain.Value)
        {
            upgrade.SetLevel(upgradeBought.TargetLevel);
            Gain.Value -= cost;
            if (Gain.Value < 0)
            {
                Gain.Value = 0;
            }
        }
    }

    public void OnResetUpdradeBought(IGameEventDetails details)
    {
        ResetUpgradeBought resetUpgradeBought = details as ResetUpgradeBought;
        ResetUpgrade resetUpgrade = resetUpgradeBought.ResetUpgrade;

        if (ResetCoin.Value >= resetUpgrade.Cost)
        {
            ResetCoin.Value -= resetUpgrade.Cost;
            resetUpgrade.Upgrade.GetMultipliedBaseValue(resetUpgrade.Multiplier);
        }
    }

    private void Reset()
    {
        Gain.Value = 0;
        TotalGain.Value = 0;
        GameController.Instance.ResetUpgrade(ClickUpgrades.Upgrades);
        GameController.Instance.ResetUpgrade(IdleUpgrades.Upgrades);
    }

    public void ResetUpgrade(Upgrade[] upgrades)
    {
        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].SetLevel(0);
        }
    }

    public void GetResetCoin()
    {
        double calc = Math.Ceiling(TotalGain.Value / 2500);

        GameController.Instance.ResetCoin.Value += calc;
    }
}
