using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private ClickerProperties properties;
    [SerializeField]
    private GameObject animatedGranny;

    private VisualElement root;

    private Label animatedLabel;

    private Button fillSpeedButton;
    private Button clickAmountButton;

    private Foldout clickUpgradeFoldout;
    private Foldout idleUpgradeFoldout;

    private readonly string[] clickUpgradeNames = { "Click Upgrade 1", "Click Upgrade 2", "Click Upgrade 3" };
    private readonly string[] idleUpgradeNames = { "Idle Upgrade 1", "Idle Upgrade 2", "Idle Upgrade 3" };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        animatedLabel = root.Q<Label>("points-label");
        fillSpeedButton = root.Q<Button>("fill-speed-button");
        clickAmountButton = root.Q<Button>("click-amount-button");

        fillSpeedButton.RegisterCallback<ClickEvent>(FillSpeedButtonClicked);
        clickAmountButton.RegisterCallback<ClickEvent>(ClickAmountButtonClicked);

        animatedLabel.dataSource = animatedGranny.GetComponent<TimeClickController>();
        animatedLabel.SetBinding(nameof(Label.text), new DataBinding { dataSourcePath = PropertyPath.FromName(nameof(TimeClickController.counter)) });

        clickUpgradeFoldout = root.Q<Foldout>("click-upgrade-foldout");
        idleUpgradeFoldout = root.Q<Foldout>("idle-upgrade-foldout");

        foreach (string upgrade in clickUpgradeNames)
        {
            Button result = new Button()
            {
                text = upgrade
            };
            result.RegisterCallback<ClickEvent, UpgradeButtonInfo>(UpgradeButtonClicked, new UpgradeButtonInfo()
            {
                IsIdleUpgrade = false,
                UpgradeName = upgrade,
            });

            clickUpgradeFoldout.Add(result);
        }

        foreach (string upgrade in idleUpgradeNames)
        {
            Button result = new Button()
            {
                text = upgrade
            };
            result.RegisterCallback<ClickEvent, UpgradeButtonInfo>(UpgradeButtonClicked, new UpgradeButtonInfo
            {
                IsIdleUpgrade = true,
                UpgradeName = upgrade
            });
            idleUpgradeFoldout.Add(result);
        }
    }

    private struct UpgradeButtonInfo
    {
        public bool IsIdleUpgrade;
        public string UpgradeName;
    }

    private static void UpgradeButtonClicked(ClickEvent clickEvent, UpgradeButtonInfo upgradeButtonInfo)
    {
        Debug.Log($"Clicked {(upgradeButtonInfo.IsIdleUpgrade ? "idle" : "click")} upgrade {upgradeButtonInfo.UpgradeName}");
    }

    private void ClickAmountButtonClicked(ClickEvent evt)
    {
        properties.timeAddedPerClick *= 2;
        properties.valueChanged = true;
    }

    private void FillSpeedButtonClicked(ClickEvent evt)
    {
        properties.timeToFill /= 2;
        properties.valueChanged = true;
    }
}
