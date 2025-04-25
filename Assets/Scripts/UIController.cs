using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private LargeNumber Gain;
    [SerializeField]
    private LargeNumber ResetCoin;
    [SerializeField]
    private UpgradeList ClickUpgrades;
    [SerializeField]
    private UpgradeList IdleUpgrades;
    [SerializeField]
    private UpgradeList ResetUpgrades;
    [SerializeField]
    private GameEvent UpgradeBoughtEvent;

    [SerializeField]
    private StringVariable GainLabelFormat;

    [SerializeField]
    private IntVariable SelectedBuyQuantity;

    [SerializeField]
    private GameObject animatedGranny;

    private VisualElement root;

    private Label animatedLabel;
    private VisualElement idleBarsParent;
    private ProgressBar[] idleBars;

    private VisualElement buyQuantityButtonsParent;

    private Foldout clickUpgradeFoldout;
    private Foldout idleUpgradeFoldout;
    private Foldout resetUpgradeFoldout;
    private ScrollView scrollView;

    private UpgradeButtonInfo[] clickUpgradeButtonInfos;
    private UpgradeButtonInfo[] idleUpgradeButtonInfos;
    private UpgradeButtonInfo[] resetUpgradeButtonInfos;

    private Button resetButton;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        animatedLabel = root.Q<Label>("points-label");
        idleBarsParent = root.Q<VisualElement>("idle-bars");
        idleBars = new ProgressBar[IdleUpgrades.Upgrades.Length];

        //UI f�l�tt van-e az eg�r
        UIInteraction.Initialize(root);

        var animatedLabelBinding = new DataBinding
        {
            dataSource = Gain,
            dataSourcePath = PropertyPath.FromName(nameof(Gain.Value)),
            bindingMode = BindingMode.ToTarget,
            updateTrigger = BindingUpdateTrigger.OnSourceChanged
        };
        var largeNumberConverterGroup = new ConverterGroup("LargeNumberToString");
        largeNumberConverterGroup.AddConverter((ref double gain) => gain.ToString(GainLabelFormat.Value));
        animatedLabelBinding.ApplyConverterGroupToUI(largeNumberConverterGroup);
        animatedLabel.SetBinding(nameof(Label.text), animatedLabelBinding);

        for (int i = 0; i < IdleUpgrades.Upgrades.Length; i++)
        {
            if (IdleUpgrades.Upgrades[i].currentLevel != 0)
            {
                idleBars[i] = CreateIdleBar(IdleUpgrades.Upgrades[i]);
                idleBarsParent.Add(idleBars[i]);
            }
        }

        scrollView = root.Q<ScrollView>("reset-skills-scroll-view");
        resetButton = root.Q<Button>("reset-progress-button");
        resetButton.clicked += ResetButtonClicked;

        clickUpgradeFoldout = root.Q<Foldout>("click-upgrade-foldout");
        clickUpgradeButtonInfos = PopulateUpgradeList(clickUpgradeFoldout, false, ClickUpgrades.Upgrades);
        idleUpgradeFoldout = root.Q<Foldout>("idle-upgrade-foldout");
        idleUpgradeButtonInfos = PopulateUpgradeList(idleUpgradeFoldout, false, IdleUpgrades.Upgrades);
        resetUpgradeFoldout = root.Q<Foldout>("reset-upgrade-foldout");
        resetUpgradeButtonInfos = PopulateUpgradeList(resetUpgradeFoldout, true, ResetUpgrades.Upgrades);
        UpdateUpgradeButton();

        buyQuantityButtonsParent = root.Q<VisualElement>("upgrade-amount-buttons");
        foreach (var button in buyQuantityButtonsParent.Children().Select(((element, i) => (element, i))))
        {
            button.element.RegisterCallback<ClickEvent, int>((_, buttonIndex) =>
            {
                SelectBuyQuantity(buttonIndex);
            }, button.i);
        }
        SelectBuyQuantity(0);
    }

    private void Update()
    {
        UpdateUpgradeButton();
    }

    private void UpdateUpgradeButton()
    {
        if ((BuyQuantity)SelectedBuyQuantity.Value == BuyQuantity.MAX)
        {
            foreach (var clickUpgrade in clickUpgradeButtonInfos)
            {
                clickUpgrade.TargetLevel = clickUpgrade.Upgrade.GetMaxAchievableLevel(Gain.Value);
                clickUpgrade.Cost = clickUpgrade.Upgrade.GetCumulativeCost(clickUpgrade.TargetLevel);
            }
            
            foreach (var idleUpgrade in idleUpgradeButtonInfos)
            {
                idleUpgrade.TargetLevel = idleUpgrade.Upgrade.GetMaxAchievableLevel(Gain.Value);
                idleUpgrade.Cost = idleUpgrade.Upgrade.GetCumulativeCost(idleUpgrade.TargetLevel);
            }
        }

        UpdateButtonAvailability(clickUpgradeButtonInfos, Gain);
        UpdateButtonAvailability(idleUpgradeButtonInfos, Gain);
        UpdateButtonAvailability(resetUpgradeButtonInfos, ResetCoin);

        foreach (UpgradeButtonInfo clickUpgrade in clickUpgradeButtonInfos)
        {
            UpdatePriceLabel(clickUpgrade.Button, clickUpgrade.Cost, false);
            UpdateLevelLabel(clickUpgrade.Button, clickUpgradeButtonInfos[i].Upgrade.currentLevel);
        }
        
        foreach (UpgradeButtonInfo idleUpgrade in idleUpgradeButtonInfos)
        {
            UpdatePriceLabel(idleUpgrade.Button, idleUpgrade.Cost, false);
            UpdateLevelLabel(idleUpgrade.Button, idleUpgradeButtonInfos[i].Upgrade.currentLevel);
        }

        foreach (UpgradeButtonInfo resetButtonInfo in resetUpgradeButtonInfos)
        {
            UpdatePriceLabel(resetButtonInfo.Button, resetButtonInfo.Cost, true);
        }
    }

    private void SelectBuyQuantity(int index)
    {
        BuyQuantity quantity = (BuyQuantity)index;

        SelectedBuyQuantity.Value = index;
        foreach (var (button, i) in buyQuantityButtonsParent.Children().Select(((element, i) => (element, i))))
        {
            ((Button)button).SetEnabled(index != i);
        }

        for (int i = 0; i < ClickUpgrades.Upgrades.Length; i++)
        {
            int targetLevel = ClickUpgrades.Upgrades[i].GetTargetLevelToTarget(quantity, Gain.Value);

            clickUpgradeButtonInfos[i].TargetLevel = targetLevel;
            clickUpgradeButtonInfos[i].Cost = ClickUpgrades.Upgrades[i].GetCumulativeCost(targetLevel);
        }

        for (int i = 0; i < IdleUpgrades.Upgrades.Length; i++)
        {
            int targetLevel = IdleUpgrades.Upgrades[i].GetTargetLevelToTarget(quantity, Gain.Value);

            idleUpgradeButtonInfos[i].TargetLevel = targetLevel;
            idleUpgradeButtonInfos[i].Cost = IdleUpgrades.Upgrades[i].GetCumulativeCost(targetLevel);
        }

        UpdateUpgradeButton();
    }

    private void ResetButtonClicked()
    {
        Gain.Value = 0;
        GameController.ResetUpgrade(ClickUpgrades.Upgrades);
        GameController.ResetUpgrade(IdleUpgrades.Upgrades);
        UpdateUpgradeButton();
    }
    
    private UpgradeButtonInfo[] PopulateUpgradeList(Foldout foldout, bool isResetUpgrade, Upgrade[] upgrades)
    {
        UpgradeButtonInfo[] buttonInfos = new UpgradeButtonInfo[upgrades.Length];
        if (!isResetUpgrade)
        {
            for (int i = 0; i < upgrades.Length; i++)
            {
                Upgrade upgrade = upgrades[i];
                Button button = new Button();
                Label skillName = new Label() { text = upgrade.Name };



                UpgradeButtonInfo buttonInfo = new UpgradeButtonInfo
                {
                    Button = button,
                    Upgrade = upgrade,
                    Cost = GetNextLevelsCost(upgrade),
                };

                Label price = new Label()
                {
                    text = $"{buttonInfo.Cost} Gain",
                    name = "price",
                };

                Label level = new Label()
                {
                    text = $"{buttonInfo.Upgrade.currentLevel} level",
                    name = "level"
                };

                buttonInfos[i] = buttonInfo;
                button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(UpgradeButtonClicked, buttonInfo);
                button.AddToClassList("upgradeButton");
                skillName.AddToClassList("skillNameLabel"); //style is currently unused
                price.AddToClassList("priceLabel"); //style is currently unused
                foldout.Add(button);
                button.Add(skillName);
                button.Add(price);
                button.Add(level);
            }
        }
        else
        {
            for (int i = 0; i < upgrades.Length; i++)
            {
                Upgrade upgrade = upgrades[i];
                Button button = new Button();
                Label skillName = new Label() { text = upgrade.Name };



                UpgradeButtonInfo buttonInfo = new UpgradeButtonInfo
                {
                    Button = button,
                    Upgrade = upgrade,
                    Cost = GetNextLevelsCost(upgrade),
                };

                Label price = new Label()
                {
                    text = $"{buttonInfo.Cost} ResetCoin",
                    name = "price",
                };
                buttonInfos[i] = buttonInfo;
                button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(UpgradeButtonClicked, buttonInfo);
                button.AddToClassList("upgradeButton");
                scrollView.AddToClassList("scrollStyle"); //style is currently unused
                skillName.AddToClassList("skillNameLabel"); //style is currently unused
                price.AddToClassList("priceLabel"); //style is currently unused
                scrollView.contentContainer.Add(button);
                button.Add(skillName);
                button.Add(price);
            }

        }       
        return buttonInfos;
    }

    private void UpdatePriceLabel(Button myButton, double currentCost, bool isResetSkill)
    {
        if (!isResetSkill)
        {
            Label priceLabel = myButton.Q<Label>("price");
            priceLabel.text = $"{currentCost} Gain";
        }
        else
        {
            Label priceLabel = myButton.Q<Label>("price");
            priceLabel.text = $"{currentCost} ResetCoin";
        }       
    }

    private void UpdateLevelLabel(Button mybutton, int currentLevel)
    {
        Label levelLabel = mybutton.Q<Label>("level");
        levelLabel.text = $"{currentLevel} level";
    }

    private class UpgradeButtonInfo
    {
        public Button Button;
        public Upgrade Upgrade;
        public double Cost;
        public int TargetLevel;
    }

    private void UpgradeButtonClicked(ClickEvent clickEvent, UpgradeButtonInfo upgradeButtonInfo)
    {
        BuyQuantity quantity = (BuyQuantity)SelectedBuyQuantity.Value;

        Debug.Log($"Clicked upgrade {upgradeButtonInfo.Upgrade.Name} buying {upgradeButtonInfo.TargetLevel - upgradeButtonInfo.Upgrade.currentLevel} levels for {upgradeButtonInfo.Cost}");
        UpgradeBought details = new()
        {
            Upgrade = upgradeButtonInfo.Upgrade,
            TargetLevel = upgradeButtonInfo.TargetLevel,
        };
        UpgradeBoughtEvent.Raise(details);
        upgradeButtonInfo.TargetLevel = upgradeButtonInfo.Upgrade.GetTargetLevelToTarget(quantity, Gain.Value);
        upgradeButtonInfo.Cost = upgradeButtonInfo.Upgrade.GetCumulativeCost(upgradeButtonInfo.TargetLevel);
        UpdateUpgradeButton();

        if (upgradeButtonInfo.Upgrade.IdleUpgradeDetails != null)
        {
            int index = System.Array.IndexOf(IdleUpgrades.Upgrades, upgradeButtonInfo.Upgrade);

            if (idleBars[index] == null)
            {
                idleBars[index] = CreateIdleBar(upgradeButtonInfo.Upgrade);
                idleBarsParent.Add(idleBars[index]);
            }
        }
    }

    private double GetNextLevelsCost(Upgrade upgrade)
    {
        return upgrade.GetCumulativeCost(upgrade.currentLevel + 1);
    }

    private static void UpdateButtonAvailability(UpgradeButtonInfo[] buttonInfos, LargeNumber gain)
    {
        for (int i = 0; i < buttonInfos.Length; i++)
        {
            UpgradeButtonInfo info = buttonInfos[i];
            info.Button.SetEnabled(info.Cost <= gain.Value);
        }
    }

    private ProgressBar CreateIdleBar(Upgrade upgrade)
    {
        ProgressBar progressBar = new ProgressBar()
        {
            name = upgrade.name,
            value = 0,
            lowValue = 0,
            highValue = 1,
            title = upgrade.IdleUpgradeDetails.IdleBarText,
        };

        progressBar.style.color = Color.black;

        var animatedLabelBinding = new DataBinding
        {
            dataSource = Gain,
            dataSourcePath = PropertyPath.FromName(nameof(Gain.Value)),
            bindingMode = BindingMode.ToTarget,
            updateTrigger = BindingUpdateTrigger.OnSourceChanged
        };
        var largeNumberConverterGroup = new ConverterGroup("LargeNumberToString");
        largeNumberConverterGroup.AddConverter((ref double gain) => gain.ToString(GainLabelFormat.Value));
        animatedLabelBinding.ApplyConverterGroupToUI(largeNumberConverterGroup);
        animatedLabel.SetBinding(nameof(Label.text), animatedLabelBinding);

        var idleProgressBinding = new DataBinding
        {
            dataSource = upgrade.IdleUpgradeDetails,
            dataSourcePath = PropertyPath.FromName(nameof(IdleUpgradeDetails.CurrentProgress)),
            bindingMode = BindingMode.ToTarget,
            updateTrigger = BindingUpdateTrigger.OnSourceChanged
        };

        progressBar.SetBinding(nameof(ProgressBar.value), idleProgressBinding);

        return progressBar;
    }
}
