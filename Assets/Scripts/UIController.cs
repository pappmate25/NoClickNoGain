using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//using System.Reflection.Emit;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField] private LargeNumber gain;
    [SerializeField] private LargeNumber totalGain;
    [SerializeField] private LargeNumber resetCoin;
    [SerializeField] private UpgradeList clickUpgrades;
    [SerializeField] private UpgradeList idleUpgrades;
    [SerializeField] private ResetUpgradeList resetUpgradesList;
    [SerializeField] private QuitDate quitDate;
    [SerializeField] private LargeNumber idleGain;
    [SerializeField] private GameEvent upgradeBoughtEvent;
    [SerializeField] private GameEvent resetUpgradeBoughtEvent;
    //[SerializeField] private GameEvent GainChangedEvent;
    [SerializeField] private IntVariable selectedBuyQuantity;
    [SerializeField] private GameObject animatedGranny;
    //[SerializeField] private StringVariable GainLabelFormat;

    private VisualElement root;

    private Label animatedLabel;
    private VisualElement idleBarsParent;
    private ProgressBar[] idleBars;

    private Foldout clickUpgradeFoldout;
    private Foldout idleUpgradeFoldout;
    private Foldout resetUpgradeFoldout;
    //private ScrollView scrollView;
    private Label resetCoinLabel;

    private ScrollView clickScrollView;
    private ScrollView idleScrollView;
    private ScrollView resetScrollView;

    private Button clickUpgradeButton;
    private Button idleUpgradeButton;
    private Button resetUpgradeButton;

    private UpgradeButtonInfo[] clickUpgradeButtonInfos;
    private UpgradeButtonInfo[] idleUpgradeButtonInfos;
    private UpgradeButtonInfo[] resetUpgradeButtonInfos;

    private Button resetButton;
    private bool isResetPressed = false;

    //welcome back popup
    private VisualElement popup;
    private Button claimButton;
    private Button twoXButton;
    private Label daysLabel;
    private Label hoursLabel;
    private Label minutesLabel;
    private Label idleGainEarned;
    public static bool IsClaimed = false;
    private VisualElement blackBg;

    //for the animated upgrade-section
    private VisualElement upgradeSection;
    private Label upgradeSectionLabel;
    private Coroutine upgradePanelAnimation;
    private bool upgradePanelVisible = false;
    private string currentVisibleUpgrade = null;

    private float animationDuration = 0.25f;
    private float hiddenLeft = -440f;
    private float shownLeft = 0f;

    //for the buy quantity button
    private Button buyQuantityToggleButton;
    private int currentBuyQuantityIndex = 0;
    private Label quantityLabel;


    //autoclick
    private Button autoClickButton;
    private AutoClicker autoClicker;

    //prestige
    private Button prestigeButton;

    #region --------- Start ---------

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        upgradeSection = root.Q<VisualElement>("upgrade-section");
        upgradeSectionLabel = root.Q<Label>("upgrade-section-label");

        clickScrollView = root.Q<ScrollView>("clickScrollView");
        idleScrollView = root.Q<ScrollView>("idleScrollView");
        resetScrollView = root.Q<ScrollView>("resetScrollView");

        clickUpgradeButton = root.Q<Button>("click-btn");
        idleUpgradeButton = root.Q<Button>("idle-btn");
        resetUpgradeButton = root.Q<Button>("reset-btn");

        ShowScrollView(resetScrollView);

        //buttons pressed event handlers
        clickUpgradeButton.clicked += () =>
        {
            upgradeSection.RemoveFromClassList("idleActive");
            upgradeSection.RemoveFromClassList("resetActive");
            upgradeSection.AddToClassList("clickActive");
            upgradeSectionLabel.text = "Click Upgrades";

            ToggleUpgradePanel("click");
            ShowScrollView(clickScrollView);
        };

        idleUpgradeButton.clicked += () =>
        {
            upgradeSection.RemoveFromClassList("resetActive");
            upgradeSection.RemoveFromClassList("clickActive");
            upgradeSection.AddToClassList("idleActive");
            upgradeSectionLabel.text = "Idle Upgrades";

            ToggleUpgradePanel("idle");
            ShowScrollView(idleScrollView);
        };

        resetUpgradeButton.clicked += () =>
        {
            upgradeSection.RemoveFromClassList("idleActive");
            upgradeSection.RemoveFromClassList("clickActive");
            upgradeSection.AddToClassList("resetActive");
            upgradeSectionLabel.text = "Reset Upgrades";

            ToggleUpgradePanel("reset");
            ShowScrollView(resetScrollView);
        };

        animatedLabel = root.Q<Label>("gain-label");
        idleBarsParent = root.Q<VisualElement>("idle-bars");
        idleBars = new ProgressBar[idleUpgrades.Upgrades.Length];

        //welcome back
        //idle time
        blackBg = root.Q<VisualElement>("black-bg");
        popup = root.Q<VisualElement>("welcome-back-popup");
        popup.SetEnabled(true);
        popup.style.display = DisplayStyle.Flex;
        claimButton = root.Q<Button>("claim-button");
        twoXButton = root.Q<Button>("watch-ad-button");
        claimButton.clicked += () =>
        {
           claimButton.schedule.Execute(() => ClaimButtonClicked()).StartingIn(150);
        };
        twoXButton.clicked += () =>
        {
           twoXButton.schedule.Execute(() => TwoXButtonClicked()).StartingIn(150);
        }; 



        daysLabel = root.Q<Label>("days-label");
        hoursLabel = root.Q<Label>("hours-label");
        minutesLabel = root.Q<Label>("minutes-label");
        UpdateIdleTimeLabels(quitDate.Value);

        //idle gain earned
        idleGainEarned = root.Q<Label>("idle-gain-earned-label");
        idleGainEarned.text = $"+{NumberFormatter.FormatNumber(idleGain.Value)}";


        //autoclick
        autoClickButton = root.Q<Button>("auto-click-button");
        autoClicker = GetComponent<AutoClicker>();
        autoClickButton.clicked += autoClicker.ToggleAutoClick;

        //prestige
        prestigeButton = root.Q<Button>("prestige-button");
        prestigeButton.clicked += PrestigeButtonClicked;


        //UI felett van-e az eger
        UIInteraction.Initialize(root);

        SetupAnimatedLabelBinding();

        for (int i = 0; i < idleUpgrades.Upgrades.Length; i++)
        {
            if (idleUpgrades.Upgrades[i].currentLevel != 0)
            {
                idleBars[i] = CreateIdleBar(idleUpgrades.Upgrades[i]);
                idleBarsParent.Add(idleBars[i]);
            }
        }

        resetScrollView = root.Q<ScrollView>("resetScrollView");
        resetButton = root.Q<Button>("reset-progress-button");
        resetButton.clicked += ResetButtonClicked;

        resetCoinLabel = root.Q<Label>("reset-points-label");
        resetCoinLabel.text = NumberFormatter.FormatNumber(resetCoin.Value);

        clickUpgradeButtonInfos = PopulateUpgradeListScrollView(clickScrollView, clickUpgrades.Upgrades);
        idleUpgradeButtonInfos = PopulateUpgradeListScrollView(idleScrollView, idleUpgrades.Upgrades);
        resetUpgradeButtonInfos = PopulateResetUpgradeListScrollView(resetScrollView, resetUpgradesList.ResetUpgrades);

        UpdateUpgradeButton();

        // 1x 5x 10x 100x MAX NEXT Breakpoint
        buyQuantityToggleButton = root.Q<Button>("buy-quantity-toggle-button");
        quantityLabel = buyQuantityToggleButton.Q<Label>("quantity-lable");

        buyQuantityToggleButton.clicked += () =>
        {
            CycleBuyQuantity();
        };

        SelectBuyQuantity(currentBuyQuantityIndex);
        quantityLabel.text = GetBuyQuantityLabel((BuyQuantity)currentBuyQuantityIndex);
    }
    #endregion

    #region --------- Update ---------
    private void Update()
    {
        UpdateUpgradeButton();
    }
    #endregion

    #region --------- Logic ---------
    //public class GainChangedDetails : IGameEventDetails
    //{
    //    public double NewGainValue;
    //}

    //public class ResetUpgradeBought : IGameEventDetails
    //{
    //    public ResetUpgrade ResetUpgrade;
    //}

    private void SetupAnimatedLabelBinding()
    {
        var binding = new DataBinding
        {
            dataSource = gain,
            dataSourcePath = PropertyPath.FromName(nameof(gain.Value)),
            bindingMode = BindingMode.ToTarget,
            updateTrigger = BindingUpdateTrigger.OnSourceChanged
        };
        var largeNumberConverterGroup = new ConverterGroup("LargeNumberToString");
        largeNumberConverterGroup.AddConverter((ref double gain) => NumberFormatter.FormatNumber(gain));
        binding.ApplyConverterGroupToUI(largeNumberConverterGroup);
        animatedLabel.SetBinding(nameof(Label.text), binding);
        animatedLabel.text = NumberFormatter.FormatNumber(gain.Value);
    }

    public void UpdateUpgradeButton()
    {
        if ((BuyQuantity)selectedBuyQuantity.Value == BuyQuantity.MAX)
        {
            foreach (var clickUpgrade in clickUpgradeButtonInfos)
            {
                clickUpgrade.TargetLevel = clickUpgrade.Upgrade.GetMaxAchievableLevel(gain.Value);
                clickUpgrade.Cost = clickUpgrade.Upgrade.GetCumulativeCost(clickUpgrade.TargetLevel);
            }

            foreach (var idleUpgrade in idleUpgradeButtonInfos)
            {
                idleUpgrade.TargetLevel = idleUpgrade.Upgrade.GetMaxAchievableLevel(gain.Value);
                idleUpgrade.Cost = idleUpgrade.Upgrade.GetCumulativeCost(idleUpgrade.TargetLevel);
            }
        }

        UpdateButtonAvailability(clickUpgradeButtonInfos, gain);
        UpdateButtonAvailability(idleUpgradeButtonInfos, gain);
        UpdateResetUpgradeButtonAvailability(resetUpgradeButtonInfos);

        UpdateResetButtonAvailability(resetButton, totalGain);
        UpdatePrestigeButtonAvailability(prestigeButton);

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

        autoClickButton.SetEnabled(IsClaimed);
    }

    #region 1x; 5x; 10x; 100x; MAX; Breakpoint
    private void SelectBuyQuantity(int index)
    {
        BuyQuantity quantity = (BuyQuantity)index;
        selectedBuyQuantity.Value = index;

        for (int i = 0; i < clickUpgrades.Upgrades.Length; i++)
        {
            int targetLevel = clickUpgrades.Upgrades[i].GetTargetLevelToTarget(quantity, gain.Value);

            clickUpgradeButtonInfos[i].TargetLevel = targetLevel;
            clickUpgradeButtonInfos[i].Cost = clickUpgrades.Upgrades[i].GetCumulativeCost(targetLevel);
        }

        for (int i = 0; i < idleUpgrades.Upgrades.Length; i++)
        {
            int targetLevel = idleUpgrades.Upgrades[i].GetTargetLevelToTarget(quantity, gain.Value);

            idleUpgradeButtonInfos[i].TargetLevel = targetLevel;
            idleUpgradeButtonInfos[i].Cost = idleUpgrades.Upgrades[i].GetCumulativeCost(targetLevel);
        }

        UpdateUpgradeButton();
    }

    private void CycleBuyQuantity()
    {
        currentBuyQuantityIndex = (currentBuyQuantityIndex + 1) % Enum.GetValues(typeof(BuyQuantity)).Length;

        SelectBuyQuantity(currentBuyQuantityIndex);
        UpdateBuyQuantityButtonText();

        AudioController.Instance.PlaySound(SfxType.BuyQuantitySwap);
    }

    private void UpdateBuyQuantityButtonText()
    {
        quantityLabel.text = GetBuyQuantityLabel((BuyQuantity)currentBuyQuantityIndex);
    }

    private string GetBuyQuantityLabel(BuyQuantity quantity)
    {
        return quantity switch
        {
            BuyQuantity.ONE => "1X",
            BuyQuantity.FIVE => "5X",
            BuyQuantity.TEN => "10X",
            BuyQuantity.HUNDRED => "100X",
            BuyQuantity.MAX => "MAX",
            BuyQuantity.BREAKPOINT => "BREAKPOINT",
            _ => "?"
        };
    }
    #endregion

    private void ClaimButtonClicked()
    {
        gain.Value += idleGain.Value;
        totalGain.Value += idleGain.Value;
        popup.SetEnabled(false);
        popup.style.display = DisplayStyle.None;
        if (blackBg != null)
            blackBg.style.display = DisplayStyle.None;
        IsClaimed = true;

        AudioController.Instance.PlaySound(SfxType.WelcomeBackClaimed);
    }

    private void TwoXButtonClicked()
    {
        gain.Value += idleGain.Value * 2;
        totalGain.Value += idleGain.Value;
        popup.SetEnabled(false);
        popup.style.display = DisplayStyle.None;
        if (blackBg != null)
            blackBg.style.display = DisplayStyle.None;
        IsClaimed = true;

        AudioController.Instance.PlaySound(SfxType.WelcomeBackClaimed);
    }

    //public static string FormatedElapsedTime(TimeSpan elapsed)            --> Unused
    //{
    //    List<string> parts = new List<string>();

    //    if (elapsed.Days > 0) parts.Add($"{elapsed.Days}");
    //    if (elapsed.Hours > 0) parts.Add($"{elapsed.Hours}");
    //    if (elapsed.Minutes > 0) parts.Add($"{elapsed.Minutes}");

    //    parts.Add($"{elapsed.Seconds}");
    //    return string.Join(" ", parts);
    //}

    private void UpdateIdleTimeLabels(TimeSpan elapsed)
    {
        daysLabel.text = elapsed.Days.ToString();
        hoursLabel.text = elapsed.Hours.ToString();
        minutesLabel.text = elapsed.Minutes.ToString();
    }

    private void ResetButtonClicked()
    {
        gain.Value = 0;

        //GainChangedEvent.Raise(new GainChangedDetails
        //{
        //    NewGainValue = Gain.Value
        //});


        GameController.Instance.Resets_Upgrades(clickUpgrades.Upgrades);
        GameController.Instance.Resets_Upgrades(idleUpgrades.Upgrades);
        GameController.Instance.GetResetCoin();

        resetCoinLabel.text = $"{NumberFormatter.FormatNumber(resetCoin.Value)}";
        isResetPressed = true;

        idleBarsParent.Clear();
        for (int i = 0; i < idleBars.Length; i++)
        {
            idleBars[i] = null;
        }

        totalGain.Value = 0;
        UpdateUpgradeButton();

        animatedLabel.text = NumberFormatter.FormatNumber(gain.Value);

        // Update the labels for click and idle upgrades
        foreach (var clickUpgrade in clickUpgradeButtonInfos)
        {
            clickUpgrade.Cost = GetNextLevelsCost(clickUpgrade.Upgrade);
            UpdatePriceLabel(clickUpgrade.Button, clickUpgrade.Cost);
            UpdateLevelLabel(clickUpgrade.Button, clickUpgrade.Upgrade.currentLevel);
        }

        foreach (var idleUpgrade in idleUpgradeButtonInfos)
        {
            idleUpgrade.Cost = GetNextLevelsCost(idleUpgrade.Upgrade);
            UpdatePriceLabel(idleUpgrade.Button, idleUpgrade.Cost);
            UpdateLevelLabel(idleUpgrade.Button, idleUpgrade.Upgrade.currentLevel);
        }
        totalGain.Value = 0;

        GameController.Instance.IncreaseResetStage();
        SelectBuyQuantity(0);

    }

    private static void UpdateResetButtonAvailability(Button button, LargeNumber totalGain)
    {
        button.SetEnabled(GameController.Instance.CanReset() && IsClaimed); //isClaimed --> ne lehessen resetelni "WelcomeBack" claim előtt
        //button.SetEnabled(totalGain.Value >= 25 && IsClaimed);                //for easy reset test
    }

    private void PrestigeButtonClicked()
    {
        ResetButtonClicked();
    }

    private static void UpdatePrestigeButtonAvailability(Button button)
    {
        button.SetEnabled(GameController.Instance.CanPrestige() && IsClaimed);
    }

    private string IconClassName(string upgradeName)
    {
        if (string.IsNullOrWhiteSpace(upgradeName))
            return "default-icon";

        string key = upgradeName.ToLowerInvariant().Replace(" ", "-");

        var supportedIcons = new HashSet<string>
        {
            "right-technique", //click skills
            "meal-prep",
            "protein-powder",
            "creatine",
            "steroid",
            "training-clothes", //idle skills
            "gym-playlist",
            "personal-trainer",
            "vitamins",
            "preworkout",
            "insane-technique-1",           //click reset skills
            "insane-technique-2",           
            "insane-technique-3",
            "healthy-meal-prep-1",
            "healthy-meal-prep-2",
            "healthy-meal-prep-3",
            "cool-brand-protein-powder-1",
            "cool-brand-protein-powder-2",
            "cool-brand-protein-powder-3",
            "cool-brand-creatine-1",
            "cool-brand-creatine-2",
            "cool-brand-creatine-3",
            "beast-steroid-1",
            "beast-steroid-2",
            "beast-steroid-3",
            "expensive-training-clothes-1", //idle reset skills
            "expensive-training-clothes-2",
            "expensive-training-clothes-3",
            "beast-mode-playlist-1",
            "beast-mode-playlist-2",
            "beast-mode-playlist-3",
            "professional-personal-trainer-1",
            "professional-personal-trainer-2",
            "professional-personal-trainer-3",
            "quality-vitamins-1",
            "quality-vitamins-2",
            "quality-vitamins-3",
            "cool-brand-preworkout-1",
            "cool-brand-preworkout-2",
            "cool-brand-preworkout-3",

            // more icons can be added here
            //if the default icon loads check if u write the name correctly
        };

        return supportedIcons.Contains(key) ? key : "default-icon";
    }

    private UpgradeButtonInfo[] PopulateResetUpgradeListScrollView(ScrollView scrollView, ResetUpgrade[] resetUpgrades)
    {
        var buttonInfos = new List<UpgradeButtonInfo>();
        scrollView.contentContainer.Clear();

        for (int i = 0; i < resetUpgrades.Length; i++)
        {
            ResetUpgrade resetUpgrade = resetUpgrades[i];

            if (resetUpgrade.isPurchased)
                continue;

            Button button = new Button();
            Label skillName = new Label() { text = resetUpgrade.Name };

            UpgradeButtonInfo buttonInfo = new UpgradeButtonInfo
            {
                Button = button,
                ResetUpgrade = resetUpgrade,
                Rank = resetUpgrade.Rank,
            };

            Label price = new Label()
            {
                text = $"ResetRank {resetUpgrade.Rank}.",
                name = "price",
            };

            //mini icon next to the lvl
            VisualElement clickUpgradeIcon = new VisualElement();
            clickUpgradeIcon.AddToClassList("click-upgrade-icon");

            string iconClass = IconClassName(resetUpgrade.Name);
            clickUpgradeIcon.AddToClassList(iconClass);

            //icon next to the price


            button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(ResetUpgradeButtonClicked, buttonInfo);
            button.AddToClassList("upgradeButton");
            skillName.AddToClassList("skillNameLabel");
            price.AddToClassList("priceLabel");
            price.style.left = 113;
            price.style.bottom = 16;
            price.style.wordSpacing = -70;

            //icon here
            button.Add(clickUpgradeIcon);

            scrollView.contentContainer.Add(button);
            button.Add(skillName);
            button.Add(price);

            buttonInfos.Add(buttonInfo);
        }

        return buttonInfos.ToArray();
    }

    private UpgradeButtonInfo[] PopulateUpgradeListScrollView(ScrollView scrollView, Upgrade[] upgrades)
    {
        UpgradeButtonInfo[] buttonInfos = new UpgradeButtonInfo[upgrades.Length];
        scrollView.contentContainer.Clear();

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

            Label level = new Label()
            {
                text = $"{buttonInfo.Upgrade.currentLevel} level",
                name = "level"
            };

            Label price = new Label()
            {

                text = $"{NumberFormatter.FormatNumber(buttonInfo.Cost)}",
                name = "price",
            };

            //mini icon next to the lvl
            VisualElement clickUpgradeIcon = new VisualElement();
            clickUpgradeIcon.AddToClassList("click-upgrade-icon");

            string iconClass = IconClassName(upgrade.Name);
            clickUpgradeIcon.AddToClassList(iconClass);

            //icon next to the price
            VisualElement pricePlusIcon = new VisualElement() { };
            VisualElement priceIcon = new VisualElement() { };


            buttonInfos[i] = buttonInfo;
            button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(UpgradeButtonClicked, buttonInfo);
            button.AddToClassList("upgradeButton");
            skillName.AddToClassList("skillNameLabel");
            level.AddToClassList("levelLabel");
            price.AddToClassList("priceLabel");
            button.Add(clickUpgradeIcon);


            pricePlusIcon.AddToClassList("pricePlusIconStyle");
            priceIcon.AddToClassList("priceIconStyle");


            pricePlusIcon.Add(priceIcon);
            pricePlusIcon.Add(price);

            button.Add(skillName);
            button.Add(level);
            button.Add(pricePlusIcon);

            scrollView.contentContainer.Add(button);
        }
        return buttonInfos;
    }

    //On skills
    private void UpdatePriceLabel(Button myButton, double currentCost)
    {
        Label priceLabel = myButton.Q<Label>("price");
        priceLabel.text = $"{NumberFormatter.FormatNumber(currentCost)}";
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
        public int Rank;
    }

    private void ResetUpgradeButtonClicked(ClickEvent clickEvent, UpgradeButtonInfo upgradeButtonInfo)
    {
        ResetUpgradeBought details = new()
        {
            ResetUpgrade = upgradeButtonInfo.ResetUpgrade,
        };

        resetUpgradeBoughtEvent.Raise(details);
        resetCoinLabel.text = $"{NumberFormatter.FormatNumber(resetCoin.Value)}";

        AudioController.Instance.PlaySound(SfxType.ResetPassiveSkillBuy);

        resetScrollView.contentContainer.Remove(upgradeButtonInfo.Button);
    }

    private void UpgradeButtonClicked(ClickEvent clickEvent, UpgradeButtonInfo upgradeButtonInfo)
    {
        BuyQuantity quantity = (BuyQuantity)selectedBuyQuantity.Value;

        UpgradeBought details = new()
        {
            Upgrade = upgradeButtonInfo.Upgrade,
            TargetLevel = upgradeButtonInfo.TargetLevel,
        };
        upgradeBoughtEvent.Raise(details);
        upgradeButtonInfo.TargetLevel = upgradeButtonInfo.Upgrade.GetTargetLevelToTarget(quantity, gain.Value);
        upgradeButtonInfo.Cost = upgradeButtonInfo.Upgrade.GetCumulativeCost(upgradeButtonInfo.TargetLevel);
        UpdateUpgradeButton();

        if (upgradeButtonInfo.Upgrade.IdleUpgradeDetails != null)
        {
            int index = Array.IndexOf(idleUpgrades.Upgrades, upgradeButtonInfo.Upgrade);

            if (idleBars[index] == null)
            {
                idleBars[index] = CreateIdleBar(upgradeButtonInfo.Upgrade);
                idleBarsParent.Add(idleBars[index]);
            }
        }

        AudioController.Instance.PlaySound(SfxType.UpgradeSkills);
    }

    private double GetNextLevelsCost(Upgrade upgrade)
    {
        return upgrade.GetCumulativeCost(upgrade.currentLevel + 1);
    }

    private static void UpdateButtonAvailability(UpgradeButtonInfo[] buttonInfos, LargeNumber gain)
    {
        foreach (UpgradeButtonInfo upgradeButtonInfo in buttonInfos)
        {
            //if (upgradeButtonInfo?.Button == null) continue;
            upgradeButtonInfo?.Button.SetEnabled(NumberFormatter.RoundCalculatedNumber(upgradeButtonInfo.Cost) <= NumberFormatter.RoundCalculatedNumber(gain.Value) && IsClaimed);
            //isClaimed --> ne lehessen skill-t fejleszteni "WelcomeBack" claim elott  
        }
        //TO DO: ezt cserelni hogy a foldout helyett a scrollView elemek legyenek kezelve
    }

    private void UpdateResetUpgradeButtonAvailability(UpgradeButtonInfo[] buttoninfos)
    {
        int currentResetStage = GameController.Instance.GetResetStage();

        foreach (UpgradeButtonInfo upgradeButtoninfo in buttoninfos)
        {
            upgradeButtoninfo?.Button.SetEnabled(upgradeButtoninfo.ResetUpgrade.Rank <= currentResetStage && IsClaimed);
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
            dataSource = gain,
            dataSourcePath = PropertyPath.FromName(nameof(gain.Value)),
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

    private void ShowScrollView(ScrollView visibleScroll)
    {
        if (clickScrollView != null)
        {
            clickScrollView.style.display = visibleScroll == clickScrollView ? DisplayStyle.Flex : DisplayStyle.None;
            //upgradeSection.RemoveFromClassList("idleActive");
            //upgradeSection.RemoveFromClassList("resetActive");
            //upgradeSection.AddToClassList("clickActive");
        }

        if (idleScrollView != null)
        {
            idleScrollView.style.display = visibleScroll == idleScrollView ? DisplayStyle.Flex : DisplayStyle.None;
            //upgradeSection.RemoveFromClassList("resetActive");
            //upgradeSection.RemoveFromClassList("clickActive");
            //upgradeSection.AddToClassList("idleActive");
        }

        if (resetScrollView != null)
        {
            resetScrollView.style.display = visibleScroll == resetScrollView ? DisplayStyle.Flex : DisplayStyle.None;
            //upgradeSection.RemoveFromClassList("clickActive");
            //upgradeSection.RemoveFromClassList("idleActive");
            //upgradeSection.AddToClassList("resetActive");
        }
    }

    // Panel animacions
    private void ToggleUpgradePanel(string contentName)
    {
        if (upgradePanelAnimation != null)
            StopCoroutine(upgradePanelAnimation);

        // close panel if the same content is clicked again
        if (upgradePanelVisible && currentVisibleUpgrade == contentName)
        {
            upgradePanelAnimation = StartCoroutine(AnimateUpgradePanel(false));
            upgradePanelVisible = false;
            currentVisibleUpgrade = null;

            upgradeSection.RemoveFromClassList("clickActive");
            upgradeSection.RemoveFromClassList("idleActive");
            upgradeSection.RemoveFromClassList("resetActive");
            //upgradeSection.AddToClassList("upgradeSection");

            AudioController.Instance.PlaySound(SfxType.CloseSkills);
            return;
        }

        ShowScrollView(GetScrollViewByName(contentName));
        currentVisibleUpgrade = contentName;

        if (!upgradePanelVisible)
        {
            upgradePanelAnimation = StartCoroutine(AnimateUpgradePanel(true));
            upgradePanelVisible = true;

            AudioController.Instance.PlaySound(SfxType.OpenSkills);
        }
        else
        {
            AudioController.Instance.PlaySound(SfxType.SwapSkillTabWhileOpen);
        }
    }

    private ScrollView GetScrollViewByName(string name)
    {
        return name switch
        {
            "click" => clickScrollView,
            "idle" => idleScrollView,
            "reset" => resetScrollView,
            _ => throw new Exception($"Unexpected string {name} and for no good reason {isResetPressed}"),
        };
    }

    private IEnumerator AnimateUpgradePanel(bool show)
    {
        float start = show ? hiddenLeft : shownLeft;
        float end = show ? shownLeft : hiddenLeft;

        float t = 0f;
        while (t < animationDuration)
        {
            float x = Mathf.Lerp(start, end, t / animationDuration);
            upgradeSection.style.left = x;
            t += Time.deltaTime;
            yield return null;
        }

        upgradeSection.style.left = end;

        if (!show)
        {
            ShowScrollView(null);
        }
    }
    #endregion
}
