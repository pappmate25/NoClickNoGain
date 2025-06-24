using System;
using System.Collections.Generic;
using System.Linq;
//using System.Reflection.Emit;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
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
    private QuitDate QuitDate;
    [SerializeField]
    private LargeNumber IdleGain;
    [SerializeField]
    private GameEvent UpgradeBoughtEvent;
    [SerializeField]
    private GameEvent ResetUpgradeBoughtEvent;
    [SerializeField]
    private GameEvent GainChangedEvent;

    //[SerializeField]
    //private StringVariable GainLabelFormat;

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
    private Label resetCoinLabel;

    private UpgradeButtonInfo[] clickUpgradeButtonInfos;
    private UpgradeButtonInfo[] idleUpgradeButtonInfos;
    private UpgradeButtonInfo[] resetUpgradeButtonInfos;

    private Button resetButton;
    private bool isResetPressed = false;

    private VisualElement popup;
    private Button claimButton;
    private Button twoXButton;
    private Label idleTime;
    private Label idleGainEarned;
    public static bool isClaimed = false;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        animatedLabel = root.Q<Label>("points-label");
        idleBarsParent = root.Q<VisualElement>("idle-bars");
        idleBars = new ProgressBar[IdleUpgrades.Upgrades.Length];

        //welcome back
        //idle time
        popup = root.Q<VisualElement>("welcome-back-popup");
        popup.SetEnabled(true);
        popup.style.display = DisplayStyle.Flex;
        claimButton = root.Q<Button>("claim-button");
        claimButton.clicked += ClaimButtonClicked;
        twoXButton = root.Q<Button>("watch-ad-button");
        twoXButton.clicked += TwoXButtonClicked;

        idleTime = root.Q<Label>("idle-time");
        idleTime.text = FormatedElapsedTime(QuitDate.Value);

        //idle gain earned
        idleGainEarned = root.Q<Label>("idle-gain-earned-label");
        idleGainEarned.text = $"+{NumberFormatter.FormatNumber(IdleGain.Value)}";


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
        largeNumberConverterGroup.AddConverter((ref double gain) => NumberFormatter.FormatNumber(gain));
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

        resetCoinLabel = root.Q<Label>("reset-points-label");
        resetCoinLabel.text = $"{NumberFormatter.FormatNumber(ResetCoin.Value)}";

        clickUpgradeFoldout = root.Q<Foldout>("click-upgrade-foldout");
        clickUpgradeButtonInfos = PopulateUpgradeList(clickUpgradeFoldout, ClickUpgrades.Upgrades);
        idleUpgradeFoldout = root.Q<Foldout>("idle-upgrade-foldout");
        idleUpgradeButtonInfos = PopulateUpgradeList(idleUpgradeFoldout, IdleUpgrades.Upgrades);
        resetUpgradeFoldout = root.Q<Foldout>("reset-upgrade-foldout");
        resetUpgradeButtonInfos = PopulateResetUpgradeList(ResetUpgradesList.ResetUpgrades);
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

    public void UpdateUpgradeButton()
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

        UpdateResetButtonAvailability(resetButton, TotalGain);

        foreach (UpgradeButtonInfo clickUpgrade in clickUpgradeButtonInfos)
        {
            UpdatePriceLabel(clickUpgrade.Button, clickUpgrade.Cost);
            UpdateLevelLabel(clickUpgrade.Button, clickUpgrade.Upgrade.currentLevel);
        }

        foreach (UpgradeButtonInfo idleUpgrade in idleUpgradeButtonInfos)
        {
            UpdatePriceLabel(idleUpgrade.Button, idleUpgrade.Cost);
            UpdateLevelLabel(idleUpgrade.Button, idleUpgrade.Upgrade.currentLevel);
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


    private void ClaimButtonClicked()
    {
        Gain.Value += IdleGain.Value;
        TotalGain.Value += IdleGain.Value;
        popup.SetEnabled(false);
        popup.style.display = DisplayStyle.None;
        isClaimed = true;
    }

    private void TwoXButtonClicked()
    {
        Gain.Value += IdleGain.Value * 2;
        TotalGain.Value += IdleGain.Value;
        popup.SetEnabled(false);
        popup.style.display = DisplayStyle.None;
        isClaimed = true;
    }

    public static string FormatedElapsedTime(TimeSpan elapsed)
    {
        List<string> parts = new List<string>();

        if(elapsed.Days > 0)
        {
            parts.Add($"{elapsed.Days} day{(elapsed.Days > 1 ? "s" : "")}");
        }
        if(elapsed.Hours > 0)
        {
            parts.Add($"{elapsed.Hours} hour{(elapsed.Hours > 1 ? "s" : "")}");
        }
        if(elapsed.Minutes > 0)
        {
            parts.Add($"{elapsed.Minutes} min{(elapsed.Minutes > 1 ? "s" : "")}");
        }

        parts.Add($"{elapsed.Seconds} sec");

        return string.Join(" ", parts);
    }

    private void ResetButtonClicked()
    {
        Gain.Value = 0;
        TotalGain.Value = 0;

        GameController.Instance.Resets_Upgrades(ClickUpgrades.Upgrades);
        GameController.Instance.Resets_Upgrades(IdleUpgrades.Upgrades);

        GameController.Instance.GetResetCoin();
        resetCoinLabel.text = $"{NumberFormatter.FormatNumber(ResetCoin.Value)}";
        isResetPressed = true;
        TotalGain.Value = 0;

        SelectBuyQuantity(0);
    
        GainChangedEvent.Raise(NoDetails.Instance);
    }

    private static void UpdateResetButtonAvailability(Button button, LargeNumber totalGain)
    {
        button.SetEnabled(totalGain.Value >= 25000 && isClaimed);                            //ez cserélhető különféle komplexebb feltétel számításra
                                                                                             //isClaimed --> ne lehessen resetelni "WelcomeBack" claim előtt       
    }

    private UpgradeButtonInfo[] PopulateResetUpgradeList(ResetUpgrade[] resetUpgrades)
    {
        UpgradeButtonInfo[] buttonInfos = new UpgradeButtonInfo[resetUpgrades.Length];

        for (int i = 0; i < resetUpgrades.Length; i++)
        {
            ResetUpgrade resetUpgrade = resetUpgrades[i];

            if (resetUpgrade.isPurchased)
            {
                continue;
            }

            Button button = new Button();
            Label skillName = new Label() { text = resetUpgrade.Name };

            UpgradeButtonInfo buttonInfo = new UpgradeButtonInfo
            {
                Button = button,
                ResetUpgrade = resetUpgrade,
                Cost = resetUpgrade.Cost,
            };

            Label price = new Label()
            {
                text = $"{NumberFormatter.FormatNumber(resetUpgrade.Cost)} ResetCoin",
                name = "price",
            };
            buttonInfos[i] = buttonInfo;
            button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(ResetUpgradeButtonClicked, buttonInfo);
            button.AddToClassList("upgradeButton");
            scrollView.AddToClassList("scrollStyle"); //style is currently unused
            skillName.AddToClassList("skillNameLabel"); //style is currently unused
            price.AddToClassList("priceLabel"); //style is currently unused
            scrollView.contentContainer.Add(button);
            button.Add(skillName);
            button.Add(price);
        }

        return buttonInfos;
    }
    private UpgradeButtonInfo[] PopulateUpgradeList(Foldout foldout, Upgrade[] upgrades)
    {
        UpgradeButtonInfo[] buttonInfos = new UpgradeButtonInfo[upgrades.Length];

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
                text = $"{NumberFormatter.FormatNumber(buttonInfo.Cost)} Gain",
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
        return buttonInfos;
    }

    //On skills
    private void UpdatePriceLabel(Button myButton, double currentCost)
    {
        Label priceLabel = myButton.Q<Label>("price");
        priceLabel.text = $"{NumberFormatter.FormatNumber(currentCost)} Gain";
    }

    //On skills
    private void UpdateLevelLabel(Button mybutton, int currentLevel)
    {
        Label levelLabel = mybutton.Q<Label>("level");
        levelLabel.text = $"{currentLevel} level";
    }

    private class UpgradeButtonInfo
    {
        public Button Button;
        public Upgrade Upgrade;
        public ResetUpgrade ResetUpgrade;
        public double Cost;
        public int TargetLevel;
    }

    private void ResetUpgradeButtonClicked(ClickEvent clickEvent, UpgradeButtonInfo upgradeButtonInfo)
    {
        ResetUpgradeBought details = new()
        {
            ResetUpgrade = upgradeButtonInfo.ResetUpgrade,
        };

        ResetUpgradeBoughtEvent.Raise(details);
        resetCoinLabel.text = $"{NumberFormatter.FormatNumber(ResetCoin.Value)}";

        scrollView.contentContainer.Remove(upgradeButtonInfo.Button);
    }


    private void UpgradeButtonClicked(ClickEvent clickEvent, UpgradeButtonInfo upgradeButtonInfo)
    {
        BuyQuantity quantity = (BuyQuantity)SelectedBuyQuantity.Value;

        //Debug.Log($"Clicked upgrade {upgradeButtonInfo.Upgrade.Name} buying {upgradeButtonInfo.TargetLevel - upgradeButtonInfo.Upgrade.currentLevel} levels for {upgradeButtonInfo.Cost}");
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
            int index = Array.IndexOf(IdleUpgrades.Upgrades, upgradeButtonInfo.Upgrade);

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
        foreach (UpgradeButtonInfo upgradeButtonInfo in buttonInfos)
        {
            upgradeButtonInfo?.Button.SetEnabled(NumberFormatter.RoundCalculatedNumber(upgradeButtonInfo.Cost) <= NumberFormatter.RoundCalculatedNumber(gain.Value) && isClaimed);
                                                                                                                                                                       //isClaimed --> ne lehessen skill-t fejleszteni "WelcomeBack" claim előtt  
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
        largeNumberConverterGroup.AddConverter((ref double gain) => NumberFormatter.FormatNumber(gain));
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
