using UnityEditor.Rendering.Universal;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
	private FloatVariable AnimationSpeed;
    [SerializeField]
	private LargeNumber Gain;
    [SerializeField]
    private FloatVariable IdlePointGainProgress;
    [SerializeField]
    private UpgradeList ClickUpgrades;
    [SerializeField]
    private UpgradeList IdleUpgrades;

    [SerializeField]
    private GameEvent ClickEvent;
    [SerializeField]
    private GameEvent UpgradeBoughtEvent;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        AnimationSpeed.Value = 10.0f;
        Reset();
        
        Gain.Value = 0;
        IdlePointGainProgress.Value = 0.0f;
	}

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < IdleUpgrades.Upgrades.Length; i++)
        {
            IdlePointGainProgress.Value += (float)IdleUpgrades.Upgrades[i].currentEffect * Time.deltaTime;
            if (IdlePointGainProgress.Value >= 1.0f)
            {
                IdlePointGainProgress.Value -= 1.0f;
                Gain.Value += IdleUpgrades.Upgrades[i].currentEffect;
                Debug.Log("Gained " + IdleUpgrades.Upgrades[i].currentEffect + " points from idle upgrade " + IdleUpgrades.Upgrades[i].name);
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
	}

    public void OnUpgradeBought(IGameEventDetails details)
    {
        UpgradeBought upgradeBought = details as UpgradeBought;
        Upgrade upgrade = upgradeBought.Upgrade;
        double cost = upgrade.GetCumilativeCost(upgradeBought.TargetLevel);
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

	private void Reset()
	{
        Gain.Value = 0;
        ResetUpgrades(ClickUpgrades.Upgrades);
        ResetUpgrades(IdleUpgrades.Upgrades);
	}

    private static void ResetUpgrades(Upgrade[] upgrades)
    {
        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].SetLevel(0);
        }
    }
}
