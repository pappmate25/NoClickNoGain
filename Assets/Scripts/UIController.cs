using System;
using System.Collections.Generic;
using System.Linq;
//using System.Reflection.Emit;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class UIController : MonoBehaviour
{
    [SerializeField] private LargeNumber Gain;
    [SerializeField] private LargeNumber TotalGain;
    [SerializeField] private LargeNumber ResetCoin;
    [SerializeField] private UpgradeList ClickUpgrades;
    [SerializeField] private UpgradeList IdleUpgrades;
    [SerializeField] private ResetUpgradeList ResetUpgradesList;
    [SerializeField] private QuitDate QuitDate;
    [SerializeField] private LargeNumber IdleGain;
    [SerializeField] private GameEvent UpgradeBoughtEvent;
    [SerializeField] private GameEvent ResetUpgradeBoughtEvent;
    [SerializeField] private GameEvent GainChangedEvent;
    [SerializeField] private IntVariable SelectedBuyQuantity;
    [SerializeField] private GameObject animatedGranny;
    //[SerializeField] private StringVariable GainLabelFormat;

    private VisualElement root;

    private Label animatedLabel;
    private VisualElement idleBarsParent;
    private ProgressBar[] idleBars;

    private Foldout clickUpgradeFoldout;
    private Foldout idleUpgradeFoldout;
    private Foldout resetUpgradeFoldout;
    private ScrollView scrollView;
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
    public static bool isClaimed = false;
    private VisualElement blackBg;

    //for the animated upgrade-section
    private VisualElement upgradeSection;
    private Coroutine upgradePanelAnimation;
    private bool upgradePanelVisible = false;
    private string currentVisibleUpgrade = null;

    private float animationDuration = 0.25f;
    private float hiddenLeft = -440f;
    private float shownLeft = 0f;

    //for the buy quantity button
    private Button buyQuantityToggleButton;
    private int currentBuyQuantityIndex = 0;


    #region --------- Start ---------
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        upgradeSection = root.Q<VisualElement>("upgrade-section");

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
            ToggleUpgradePanel("click");
            ShowScrollView(clickScrollView);
        };

        idleUpgradeButton.clicked += () =>
        {
            ToggleUpgradePanel("idle");
            ShowScrollView(idleScrollView);
        };

        resetUpgradeButton.clicked += () =>
        {
            ToggleUpgradePanel("reset");
            ShowScrollView(resetScrollView);
        };

        animatedLabel = root.Q<Label>("points-label");
        idleBarsParent = root.Q<VisualElement>("idle-bars");
        idleBars = new ProgressBar[IdleUpgrades.Upgrades.Length];

        //welcome back
        //idle time
        blackBg = root.Q<VisualElement>("black-bg");
        popup = root.Q<VisualElement>("welcome-back-popup");
        popup.SetEnabled(true);
        popup.style.display = DisplayStyle.Flex;
        claimButton = root.Q<Button>("claim-button");
        twoXButton = root.Q<Button>("watch-ad-button");
        claimButton.clicked += ClaimButtonClicked;
        twoXButton.clicked += TwoXButtonClicked;

        daysLabel = root.Q<Label>("days-label");
        hoursLabel = root.Q<Label>("hours-label");
        minutesLabel = root.Q<Label>("minutes-label");
        UpdateIdleTimeLabels(QuitDate.Value);

        //idle gain earned
        idleGainEarned = root.Q<Label>("idle-gain-earned-label");
        idleGainEarned.text = $"+{NumberFormatter.FormatNumber(IdleGain.Value)}";

        //UI felett van-e az eger
        UIInteraction.Initialize(root);

        SetupAnimatedLabelBinding();

        for (int i = 0; i < IdleUpgrades.Upgrades.Length; i++)
        {
            if (IdleUpgrades.Upgrades[i].currentLevel != 0)
            {
                idleBars[i] = CreateIdleBar(IdleUpgrades.Upgrades[i]);
                idleBarsParent.Add(idleBars[i]);
            }
        }

        resetScrollView = root.Q<ScrollView>("resetScrollView");
        resetButton = root.Q<Button>("reset-progress-button");
        resetButton.clicked += ResetButtonClicked;

        resetCoinLabel = root.Q<Label>("reset-points-label");
        resetCoinLabel.text = NumberFormatter.FormatNumber(ResetCoin.Value);

        clickUpgradeButtonInfos = PopulateUpgradeListScrollView(clickScrollView, ClickUpgrades.Upgrades);
        idleUpgradeButtonInfos = PopulateUpgradeListScrollView(idleScrollView, IdleUpgrades.Upgrades);
        resetUpgradeButtonInfos = PopulateResetUpgradeListScrollView(resetScrollView, ResetUpgradesList.ResetUpgrades);

        UpdateUpgradeButton();

        // 1x 5x 10x 100x MAX NEXT Breakpoint
        buyQuantityToggleButton = root.Q<Button>("buy-quantity-toggle-button");

        buyQuantityToggleButton.clicked += () =>
        {
            CycleBuyQuantity();
        };

        SelectBuyQuantity(currentBuyQuantityIndex);
        buyQuantityToggleButton.text = GetBuyQuantityLabel((BuyQuantity)currentBuyQuantityIndex);
    }
    #endregion

    #region --------- Update ---------
    private void Update()
    {
        UpdateUpgradeButton();
    }
    #endregion

    #region --------- Logic ---------
    public class GainChangedDetails : IGameEventDetails
    {
        public double NewGainValue;
    }

    public class ResetUpgradeBought : IGameEventDetails
    {
        public ResetUpgrade ResetUpgrade;
    }

    private void SetupAnimatedLabelBinding()
    {
        var binding = new DataBinding
        {
            dataSource = Gain,
            dataSourcePath = PropertyPath.FromName(nameof(Gain.Value)),
            bindingMode = BindingMode.ToTarget,
            updateTrigger = BindingUpdateTrigger.OnSourceChanged
        };
        var largeNumberConverterGroup = new ConverterGroup("LargeNumberToString");
        largeNumberConverterGroup.AddConverter((ref double gain) => NumberFormatter.FormatNumber(gain));
        binding.ApplyConverterGroupToUI(largeNumberConverterGroup);
        animatedLabel.SetBinding(nameof(Label.text), binding);
        animatedLabel.text = NumberFormatter.FormatNumber(Gain.Value);
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

        UpdateResetButtonAvailability(resetUpgradeButton, TotalGain);

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

    #region 1x; 5x; 10x; 100x; MAX; Breakpoint
    private void SelectBuyQuantity(int index)
    {
        BuyQuantity quantity = (BuyQuantity)index;
        SelectedBuyQuantity.Value = index;

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

    private void CycleBuyQuantity()
    {
        currentBuyQuantityIndex = (currentBuyQuantityIndex + 1) % Enum.GetValues(typeof(BuyQuantity)).Length;

        SelectBuyQuantity(currentBuyQuantityIndex);
        UpdateBuyQuantityButtonText();
    }

    private void UpdateBuyQuantityButtonText()
    {
        buyQuantityToggleButton.text = GetBuyQuantityLabel((BuyQuantity)currentBuyQuantityIndex);
    }

    private string GetBuyQuantityLabel(BuyQuantity quantity)
    {
        return quantity switch
        {
            BuyQuantity.ONE => "1x",
            BuyQuantity.FIVE => "5x",
            BuyQuantity.TEN => "10x",
            BuyQuantity.HUNDRED => "100x",
            BuyQuantity.MAX => "MAX",
            BuyQuantity.BREAKPOINT => "BREAKPOINT",
            _ => "?"
        };
    }
    #endregion

    private void ClaimButtonClicked()
    {
        Gain.Value += IdleGain.Value;
        TotalGain.Value += IdleGain.Value;
        popup.SetEnabled(false);
        popup.style.display = DisplayStyle.None;
        if (blackBg != null)
            blackBg.style.display = DisplayStyle.None;
        isClaimed = true;
    }

    private void TwoXButtonClicked()
    {
        Gain.Value += IdleGain.Value * 2;
        TotalGain.Value += IdleGain.Value;
        popup.SetEnabled(false);
        popup.style.display = DisplayStyle.None;
        if (blackBg != null)
            blackBg.style.display = DisplayStyle.None;
        isClaimed = true;
    }

    public static string FormatedElapsedTime(TimeSpan elapsed)
    {
        List<string> parts = new List<string>();

        if (elapsed.Days > 0) parts.Add($"{elapsed.Days}");
        if (elapsed.Hours > 0) parts.Add($"{elapsed.Hours}");
        if (elapsed.Minutes > 0) parts.Add($"{elapsed.Minutes}");

        parts.Add($"{elapsed.Seconds}");
        return string.Join(" ", parts);
    }

    private void UpdateIdleTimeLabels(TimeSpan elapsed)
    {
        daysLabel.text = elapsed.Days.ToString();
        hoursLabel.text = elapsed.Hours.ToString();
        minutesLabel.text = elapsed.Minutes.ToString();
    }

    private void ResetButtonClicked()
    {
        Gain.Value = 0;

        GainChangedEvent.Raise(new GainChangedDetails
        {
            NewGainValue = Gain.Value
        });


        GameController.Instance.Resets_Upgrades(ClickUpgrades.Upgrades);
        GameController.Instance.Resets_Upgrades(IdleUpgrades.Upgrades);
        GameController.Instance.GetResetCoin();

        resetCoinLabel.text = $"{NumberFormatter.FormatNumber(ResetCoin.Value)}";
        isResetPressed = true;

        idleBarsParent.Clear();
        for (int i = 0; i < idleBars.Length; i++)
        {
            idleBars[i] = null;
        }

        TotalGain.Value = 0;
        UpdateUpgradeButton();

        animatedLabel.text = NumberFormatter.FormatNumber(Gain.Value);

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
    }

    private static void UpdateResetButtonAvailability(Button button, LargeNumber totalGain)
    {
        button.SetEnabled(totalGain.Value >= 25000 && isClaimed);                            //ez cserelheto kulonfele komplexebb feltetel szamitasra
                                                                                             //isClaimed --> ne lehessen resetelni "WelcomeBack" claim elott       
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
                Cost = resetUpgrade.Cost,
            };

            Label price = new Label()
            {
                text = $"{NumberFormatter.FormatNumber(resetUpgrade.Cost)} ResetCoin",
                name = "price",
            };

            button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(ResetUpgradeButtonClicked, buttonInfo);

            button.AddToClassList("upgradeButton");
            skillName.AddToClassList("skillNameLabel");
            price.AddToClassList("priceLabel");
            //icon here

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
            VisualElement clickIcon = new VisualElement();
            clickIcon.AddToClassList("clickIcon");

            //big icon in the button
            VisualElement clickUpgradeIcon = new VisualElement();
            clickUpgradeIcon.AddToClassList("clickUpgradeIcon");

            buttonInfos[i] = buttonInfo;
            button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(UpgradeButtonClicked, buttonInfo);
            button.AddToClassList("upgradeButton");
            skillName.AddToClassList("skillNameLabel");
            level.AddToClassList("levelLabel");
            price.AddToClassList("priceLabel");
            button.Add(clickUpgradeIcon);
            //icon here

            scrollView.contentContainer.Add(button);
            button.Add(skillName);
            button.Add(clickIcon);
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
            if (upgradeButtonInfo?.Button == null) continue;

            upgradeButtonInfo.Button.SetEnabled(
                NumberFormatter.RoundCalculatedNumber(upgradeButtonInfo.Cost) <=
                NumberFormatter.RoundCalculatedNumber(gain.Value) &&
                isClaimed
            );
            //isClaimed --> ne lehessen skill-t fejleszteni "WelcomeBack" claim előtt  
        }
        //TO DO: ezt cserélni hogy a foldout helyett a scrollView elemek legyenek kezelve
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

    private void ShowScrollView(ScrollView visibleScroll)
    {
        if (clickScrollView != null)
            clickScrollView.style.display = visibleScroll == clickScrollView ? DisplayStyle.Flex : DisplayStyle.None;

        if (idleScrollView != null)
            idleScrollView.style.display = visibleScroll == idleScrollView ? DisplayStyle.Flex : DisplayStyle.None;

        if (resetScrollView != null)
            resetScrollView.style.display = visibleScroll == resetScrollView ? DisplayStyle.Flex : DisplayStyle.None;
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
            return;
        }

        ShowScrollView(GetScrollViewByName(contentName));
        currentVisibleUpgrade = contentName;

        if (!upgradePanelVisible)
        {
            upgradePanelAnimation = StartCoroutine(AnimateUpgradePanel(true));
            upgradePanelVisible = true;
        }
    }

    private ScrollView GetScrollViewByName(string name)
    {
        return name switch
        {
            "click" => clickScrollView,
            "idle" => idleScrollView,
            "reset" => resetScrollView
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