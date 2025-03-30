using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private LargeNumber Gain;
    [SerializeField]
    private UpgradeList ClickUpgrades;
    [SerializeField]
    private UpgradeList IdleUpgrades;
    [SerializeField]
    private GameEvent UpgradeBoughtEvent;

	[SerializeField]
    private GameObject animatedGranny;

    private VisualElement root;

    private Label animatedLabel;

    private Foldout clickUpgradeFoldout;
    private Foldout idleUpgradeFoldout;

	private UpgradeButtonInfo[] clickUpgradeButtonInfos;
	private UpgradeButtonInfo[] idleUpgradeButtonInfos;

    private int LevelsToBuy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		root = GetComponent<UIDocument>().rootVisualElement;
        animatedLabel = root.Q<Label>("points-label");

        animatedLabel.dataSource = Gain;
        animatedLabel.SetBinding(nameof(Label.text), new DataBinding { dataSourcePath = PropertyPath.FromName(nameof(Gain.Value)) });

		LevelsToBuy = 1;

		clickUpgradeFoldout = root.Q<Foldout>("click-upgrade-foldout");
        clickUpgradeButtonInfos = PopulateUpgradeList(clickUpgradeFoldout, false, ClickUpgrades.Upgrades);
		idleUpgradeFoldout = root.Q<Foldout>("idle-upgrade-foldout");
		idleUpgradeButtonInfos = PopulateUpgradeList(idleUpgradeFoldout, true, IdleUpgrades.Upgrades);
        UpdateButtonAvailability();

	}

	private void Update()
	{
		UpdateButtonAvailability();
	}

    private void UpdateButtonAvailability()
    {
		UpdateButtonAvailability(clickUpgradeButtonInfos, Gain);
		UpdateButtonAvailability(idleUpgradeButtonInfos, Gain);
	}

	private UpgradeButtonInfo[] PopulateUpgradeList(Foldout foldout, bool isIdleUpgrade, Upgrade[] upgrades)
    {
		UpgradeButtonInfo[] buttonInfos = new UpgradeButtonInfo[upgrades.Length];
        for (int i = 0; i < upgrades.Length; i++) {
            Upgrade upgrade = upgrades[i];
			Button button = new Button()
			{
				text = upgrade.Name,
			};
			UpgradeButtonInfo buttonInfo = new UpgradeButtonInfo
			{
                Button = button,
                Upgrade = upgrade,
                Cost = GetNextLevelsCost(upgrade),
			};
            buttonInfos[i] = buttonInfo;
			button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(UpgradeButtonClicked, buttonInfo);
			foldout.Add(button);
		}
        return buttonInfos;
	}

	private class UpgradeButtonInfo
    {
        public Button Button;
        public Upgrade Upgrade;
        public double Cost;
    }
     
    private void UpgradeButtonClicked(ClickEvent clickEvent, UpgradeButtonInfo upgradeButtonInfo)
    {
        Debug.Log($"Clicked upgrade {upgradeButtonInfo.Upgrade.Name} buying {LevelsToBuy} levels for {upgradeButtonInfo.Cost}");
        UpgradeBought details = new()
        {
            Upgrade = upgradeButtonInfo.Upgrade,
            TargetLevel = upgradeButtonInfo.Upgrade.currentLevel + LevelsToBuy,
        };
        UpgradeBoughtEvent.Raise(details);
		upgradeButtonInfo.Cost = GetNextLevelsCost(upgradeButtonInfo.Upgrade);
		UpdateButtonAvailability();
	}

	private double GetNextLevelsCost(Upgrade upgrade)
    {
        return upgrade.GetCumilativeCost(upgrade.currentLevel + LevelsToBuy);
    }

    private static void UpdateButtonAvailability(UpgradeButtonInfo[] buttonInfos, LargeNumber gain)
    {
        for (int i = 0; i < buttonInfos.Length; i++)
        {
            UpgradeButtonInfo info = buttonInfos[i];
            info.Button.SetEnabled(info.Cost <= gain.Value);
        }
    }
}
