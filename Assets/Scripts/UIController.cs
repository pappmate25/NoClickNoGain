using System;
using System.Collections.Generic;
using System.Linq;
//using System.Reflection.Emit;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

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

    private VisualElement buyQuantityButtonsParent;

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

    private VisualElement popup;
    private Button claimButton;
    private Button twoXButton;
    private Label idleTime;
    private Label idleGainEarned;
    public static bool isClaimed = false;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        clickScrollView = root.Q<ScrollView>("clickScrollView");
        idleScrollView = root.Q<ScrollView>("idleScrollView");
        resetScrollView = root.Q<ScrollView>("resetScrollView");

        clickUpgradeButton = root.Q<Button>("click-btn");
        idleUpgradeButton = root.Q<Button>("idle-btn");
        resetUpgradeButton = root.Q<Button>("reset-btn");
        resetButton = root.Q<Button>("reset-progress-button");

        clickUpgradeButton.clicked += () => ShowScrollView(clickScrollView);
        idleUpgradeButton.clicked += () => ShowScrollView(idleScrollView);
        resetUpgradeButton.clicked += () => ShowScrollView(resetScrollView);
        resetButton.clicked += ResetButtonClicked;

        animatedLabel = root.Q<Label>("points-label");
        idleBarsParent = root.Q<VisualElement>("idle-bars");
        idleBars = new ProgressBar[IdleUpgrades.Upgrades.Length];

        popup = root.Q<VisualElement>("welcome-back-popup");
        popup.SetEnabled(true);
        popup.style.display = DisplayStyle.Flex;
        claimButton = root.Q<Button>("claim-button");
        twoXButton = root.Q<Button>("watch-ad-button");
        claimButton.clicked += ClaimButtonClicked;
        twoXButton.clicked += TwoXButtonClicked;

        idleTime = root.Q<Label>("idle-time");
        idleTime.text = FormatedElapsedTime(QuitDate.Value);
        idleGainEarned = root.Q<Label>("idle-gain-earned-label");
        idleGainEarned.text = $"+{NumberFormatter.FormatNumber(IdleGain.Value)}";

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

        resetCoinLabel = root.Q<Label>("reset-points-label");
        resetCoinLabel.text = NumberFormatter.FormatNumber(ResetCoin.Value);

        clickUpgradeButtonInfos = PopulateUpgradeListScrollView(clickScrollView, ClickUpgrades.Upgrades);
        idleUpgradeButtonInfos = PopulateUpgradeListScrollView(idleScrollView, IdleUpgrades.Upgrades);
        resetUpgradeButtonInfos = PopulateResetUpgradeListScrollView(resetScrollView, ResetUpgradesList.ResetUpgrades);

        buyQuantityButtonsParent = root.Q<VisualElement>("upgrade-amount-buttons");
        foreach (var button in buyQuantityButtonsParent.Children().Select(((element, i) => (element, i))))
        {
            button.element.RegisterCallback<ClickEvent, int>((_, buttonIndex) => SelectBuyQuantity(buttonIndex), button.i);
        }

        SelectBuyQuantity(0);
    }

    private void Update()
    {
        UpdateUpgradeButton();
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
        var converter = new ConverterGroup("LargeNumberToString");
        converter.AddConverter((ref double gain) => NumberFormatter.FormatNumber(gain));
        binding.ApplyConverterGroupToUI(converter);
        animatedLabel.SetBinding(nameof(Label.text), binding);
    }

    public void UpdateUpgradeButton()
    {
        if (clickUpgradeButtonInfos == null || idleUpgradeButtonInfos == null || resetUpgradeButtonInfos == null)
        {
            Debug.LogWarning("One or more UpgradeButtonInfos are null – skipping UpdateUpgradeButton.");
            return;
        }

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
            if (clickUpgradeButtonInfos == null || clickUpgradeButtonInfos[i] == null) continue;
            int targetLevel = ClickUpgrades.Upgrades[i].GetTargetLevelToTarget(quantity, Gain.Value);
            clickUpgradeButtonInfos[i].TargetLevel = targetLevel;
            clickUpgradeButtonInfos[i].Cost = ClickUpgrades.Upgrades[i].GetCumulativeCost(targetLevel);
        }

        for (int i = 0; i < IdleUpgrades.Upgrades.Length; i++)
        {
            if (idleUpgradeButtonInfos == null || idleUpgradeButtonInfos[i] == null) continue;
            int targetLevel = IdleUpgrades.Upgrades[i].GetTargetLevelToTarget(quantity, Gain.Value);
            idleUpgradeButtonInfos[i].TargetLevel = targetLevel;
            idleUpgradeButtonInfos[i].Cost = IdleUpgrades.Upgrades[i].GetCumulativeCost(targetLevel);
        }

        UpdateUpgradeButton();
    }

    //... ScrollView és Button elemek kezelése
    //ScrollView megjelenítése
    private void ShowScrollView(ScrollView visibleScroll)
    {
        clickScrollView.style.display = visibleScroll == clickScrollView ? DisplayStyle.Flex : DisplayStyle.None;
        idleScrollView.style.display = visibleScroll == idleScrollView ? DisplayStyle.Flex : DisplayStyle.None;
        resetScrollView.style.display = visibleScroll == resetScrollView ? DisplayStyle.Flex : DisplayStyle.None;
    }

    //ScrollView és Button elemek
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

            if (buttonInfo != null)
                buttonInfos[i] = buttonInfo;

            button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(UpgradeButtonClicked, buttonInfo);
            button.AddToClassList("upgradeButton");
            skillName.AddToClassList("skillNameLabel");
            price.AddToClassList("priceLabel");

            scrollView.contentContainer.Add(button);
            button.Add(skillName);
            button.Add(price);
            button.Add(level);
        }

        return buttonInfos;
    }


    private UpgradeButtonInfo[] PopulateResetUpgradeListScrollView(ScrollView scrollView, ResetUpgrade[] resetUpgrades)
    {
        scrollView.contentContainer.Clear();
        List<UpgradeButtonInfo> buttonInfos = new();

        //background colors for buttons
        string[] backgrounds  = { "bg-red", "bg-blue", "bg-purple", "bg-green", "bg-orange" };
        int visibleIndex = 0;

        foreach (var resetUpgrade in resetUpgrades)
        {
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

            button.AddToClassList("upgradeButton");
            string styleClass = backgrounds[visibleIndex % backgrounds.Length];
            button.AddToClassList(styleClass);
            visibleIndex++;

            button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(ResetUpgradeButtonClicked, buttonInfo);
            skillName.AddToClassList("skillNameLabel");
            price.AddToClassList("priceLabel");

            scrollView.contentContainer.Add(button);
            button.Add(skillName);
            button.Add(price);

            buttonInfos.Add(buttonInfo);
        }

        return buttonInfos.ToArray();
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
        List<string> parts = new();
        if (elapsed.Days > 0) parts.Add($"{elapsed.Days} day{(elapsed.Days > 1 ? "s" : "")}");
        if (elapsed.Hours > 0) parts.Add($"{elapsed.Hours} hour{(elapsed.Hours > 1 ? "s" : "")}");
        if (elapsed.Minutes > 0) parts.Add($"{elapsed.Minutes} min{(elapsed.Minutes > 1 ? "s" : "")}");
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
        if (buttonInfos == null)
        {
            Debug.LogError("buttonInfos is NULL");
            return;
        }

        for (int i = 0; i < buttonInfos.Length; i++)
        {
            var b = buttonInfos[i];
            if (b == null)
            {
                Debug.LogWarning($"buttonInfos[{i}] is NULL");
                continue;
            }
            if (b.Button == null)
            {
                Debug.LogWarning($"UpgradeButtonInfo at index {i} has NULL Button for Upgrade: '{b.Upgrade?.Name}'");
                continue;
            }

            b.Button.SetEnabled(
                NumberFormatter.RoundCalculatedNumber(b.Cost) <= NumberFormatter.RoundCalculatedNumber(gain.Value)
                && isClaimed
            );
        }
        //isClaimed --> ne lehessen skill-t fejleszteni "WelcomeBack" claim előtt  
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
