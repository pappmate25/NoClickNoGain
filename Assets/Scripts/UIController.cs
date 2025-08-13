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
    [SerializeField] private PassiveSkillList passiveSkillsList;
    [SerializeField] private QuitDate quitDate;
    [SerializeField] private LargeNumber idleGain;
    [SerializeField] private GameEvent upgradeBoughtEvent;
    [SerializeField] private GameEvent resetUpgradeBoughtEvent;
    [SerializeField] private GameEvent passiveSkillBoughtEvent;
    //[SerializeField] private GameEvent GainChangedEvent;
    [SerializeField] private IntVariable selectedBuyQuantity;
    [SerializeField] private GameObject animatedGranny;
    //[SerializeField] private StringVariable GainLabelFormat;
    [SerializeField] private AudioController audioController;

    private VisualElement root;

    private Label animatedLabel;
    private VisualElement idleBarsParent;
    private ProgressBar[] idleBars;

    private Label resetCoinLabel;

    private ScrollView clickScrollView;
    private ScrollView idleScrollView;
    private ScrollView resetScrollView;
    private ScrollView passiveScrollView;

    private Button clickUpgradeButton;
    private Button idleUpgradeButton;
    private Button resetUpgradeButton;
    private Button passiveSkillButton;

    private UpgradeButtonInfo[] clickUpgradeButtonInfos;
    private UpgradeButtonInfo[] idleUpgradeButtonInfos;
    private UpgradeButtonInfo[] resetUpgradeButtonInfos;
    private UpgradeButtonInfo[] passiveSkillButtonInfos;

    private Button resetButton;
    private bool isResetPressed = false;

    private RevealStage revealStage = RevealStage.ClickUpgrades; // Start with click upgrades revealed

    private Texture2D[] resetRanks;

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

    private float animationDuration = 0.450f;
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

    //volume
    private int musicLevel = 4;
    private int sfxLevel = 4;
    private Button soundOnButton;
    private Button soundOffButton;


    //background
    private VisualElement desk;
    private VisualElement shelf;
    private VisualElement vitamins;
    private VisualElement pizzaBurger;
    private VisualElement protein;
    private VisualElement preworkout;
    private VisualElement creatine;
    private Dictionary<string, Action> backgroundTriggers;
    private Dictionary<string, Action> resetBackgroundTriggers;

    //background animations
    private VisualElement flower;
    private VisualElement socks;
    private VisualElement window;
    private VisualElement guy;
    private VisualElement speakerLeft;
    private VisualElement speakerRight;
    private VisualElement trainer;
    private VisualElement mealPrep;
    private VisualElement guyTraining;
    private UIAnimationController animationController;

    //debug elements
    private Button loadSaveFromClipboard;
    private Button copySaveToClipboard;
    private Button toggleSaveEncryption;
    private Button forceShowAllUiButton;
    private bool forceShowingUi = false;

    [SerializeField]
    private SaveHandler saveHandler;
    [SerializeField]
    private GameController gameController;

    #region --------- Start ---------

    void Start()
    {
        resetRanks = new Texture2D[2];
        for (int i = 0; i < resetRanks.Length; i++)
        {
            resetRanks[i] = Resources.Load<Texture2D>($"UI/Upgrade section/Reset upgrades/reset-rank-{i}");
        }

        root = GetComponent<UIDocument>().rootVisualElement;

        upgradeSection = root.Q<VisualElement>("upgrade-section");
        upgradeSectionLabel = root.Q<Label>("upgrade-section-label");

        clickScrollView = root.Q<ScrollView>("clickScrollView");
        idleScrollView = root.Q<ScrollView>("idleScrollView");
        resetScrollView = root.Q<ScrollView>("resetScrollView");
        passiveScrollView = root.Q<ScrollView>("passiveScrollView");

        clickUpgradeButton = root.Q<Button>("click-btn");
        idleUpgradeButton = root.Q<Button>("idle-btn");
        resetUpgradeButton = root.Q<Button>("reset-btn");
        passiveSkillButton = root.Q<Button>("passive-btn");

        ShowScrollView(resetScrollView);

        //buttons pressed event handlers
        clickUpgradeButton.clicked += () =>
        {
            upgradeSection.RemoveFromClassList("idleActive");
            upgradeSection.RemoveFromClassList("resetActive");
            upgradeSection.RemoveFromClassList("passiveActive");
            upgradeSection.AddToClassList("clickActive");
            upgradeSectionLabel.text = "Click Upgrades";

            ToggleUpgradePanel("click");
            ShowScrollView(clickScrollView);
        };

        idleUpgradeButton.clicked += () =>
        {
            upgradeSection.RemoveFromClassList("resetActive");
            upgradeSection.RemoveFromClassList("clickActive");
            upgradeSection.RemoveFromClassList("passiveActive");
            upgradeSection.AddToClassList("idleActive");
            upgradeSectionLabel.text = "Idle Upgrades";

            ToggleUpgradePanel("idle");
            ShowScrollView(idleScrollView);
        };

        resetUpgradeButton.clicked += () =>
        {
            upgradeSection.RemoveFromClassList("idleActive");
            upgradeSection.RemoveFromClassList("clickActive");
            upgradeSection.RemoveFromClassList("passiveActive");
            upgradeSection.AddToClassList("resetActive");
            upgradeSectionLabel.text = "Reset Upgrades";

            ToggleUpgradePanel("reset");
            ShowScrollView(resetScrollView);
        };

        passiveSkillButton.clicked += () =>
        {
            upgradeSection.RemoveFromClassList("idleActive");
            upgradeSection.RemoveFromClassList("clickActive");
            upgradeSection.RemoveFromClassList("resetActive");
            upgradeSection.AddToClassList("passiveActive");
            upgradeSectionLabel.text = "Passive Skills";

            ToggleUpgradePanel("passive");
            ShowScrollView(passiveScrollView);
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
            audioController.PlaySound(SfxType.WelcomeBackClaimed);
            claimButton.schedule.Execute(() => ClaimButtonClicked()).StartingIn(400);
        };
        twoXButton.clicked += () =>
        {
            audioController.PlaySound(SfxType.WelcomeBackClaimed);
            twoXButton.schedule.Execute(() => TwoXButtonClicked()).StartingIn(400);
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


        //background animations
        //classlist background swap
        desk = root.Q<VisualElement>("desk");
        shelf = root.Q<VisualElement>("shelf");
        vitamins = root.Q<VisualElement>("vitamins");
        pizzaBurger = root.Q<VisualElement>("pizza-burger");
        protein = root.Q<VisualElement>("protein");
        preworkout = root.Q<VisualElement>("preworkout");
        creatine = root.Q<VisualElement>("creatine");

        //anims
        animationController = new UIAnimationController();

        flower = root.Q<VisualElement>("flower");
        socks = root.Q<VisualElement>("socks");
        window = root.Q<VisualElement>("window");
        guy = root.Q<VisualElement>("guy-normal-clothes");
        guyTraining = root.Q<VisualElement>("guy-training-clothes");
        speakerLeft = root.Q<VisualElement>("speaker-left");
        speakerRight = root.Q<VisualElement>("speaker-right");
        trainer = root.Q<VisualElement>("trainer");
        mealPrep = root.Q<VisualElement>("healthy-meal-prep");


        animationController.Register("flower", "Animations/Flower", "flower", 2, 1500, flower);
        animationController.Register("socks", "Animations/Socks", "socks", 2, 800, socks);
        animationController.Register("window", "Animations/Window", "window", 7, 700, window);
        animationController.Register("guy", "Animations/Guy_normal_clothes", "guy", 2, 1000, guy);
        animationController.Register("guyTraining", "Animations/Guy_training_clothes", "guy_training_clothes", 2, 1000, guyTraining);
        animationController.Register("speakerLeft", "Animations/Speaker", "speaker", 2, 300, speakerLeft);
        animationController.Register("speakerRight", "Animations/Speaker", "speaker", 2, 300, speakerRight);
        animationController.Register("trainer", "Animations/Trainer", "trainer", 9, 500, trainer);
        animationController.Register("mealPrep", "Animations/Healthy_meal", "healthy_meal_prep", 3, 300, mealPrep);

        InitializeBackgroundTriggers();
        ResetBackgroundTriggers();
        ApplyUnlockedEffects();


        //UI felett van-e az eger
        UIInteraction.Initialize(root);

        SetupAnimatedLabelBinding();

        AddIdleBars();

        resetScrollView = root.Q<ScrollView>("resetScrollView");
        resetButton = root.Q<Button>("reset-progress-button");
        resetButton.clicked += ResetButtonClicked;

        resetCoinLabel = root.Q<Label>("reset-points-label");
        resetCoinLabel.text = NumberFormatter.FormatNumber(resetCoin.Value);

        clickUpgradeButtonInfos = PopulateUpgradeListScrollView(clickScrollView, clickUpgrades.Upgrades);
        idleUpgradeButtonInfos = PopulateUpgradeListScrollView(idleScrollView, idleUpgrades.Upgrades);
        resetUpgradeButtonInfos = PopulateResetUpgradeListScrollView(resetScrollView, resetUpgradesList.ResetUpgrades);
        passiveSkillButtonInfos = PopulatePassiveSkillListScrollView(passiveScrollView, passiveSkillsList.PassiveSkills);

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

        // options popup
        var optionsButton = root.Q<Button>("options");
        var optionsPopup = root.Q<VisualElement>("options-popup");
        var optionsExitButton = root.Q<Button>("exitButton");

        optionsPopup.style.display = DisplayStyle.None;

        optionsButton.clicked += () =>
        {
            optionsPopup.style.display = DisplayStyle.Flex;
        };

        optionsExitButton.clicked += () =>
        {
            optionsPopup.style.display = DisplayStyle.None;
        };

        SetupVolumeControls();
        SetupMuteButtons();
        StartConstantAnimations();

        //debug elements
        loadSaveFromClipboard = root.Q<Button>("load-save-debug");
        loadSaveFromClipboard.clicked += () =>
        {
            saveHandler.LoadFromClipboard();
            SelectBuyQuantity(selectedBuyQuantity.Value); // Needed to refresh the upgrade button infos.
            gameController.ResetIdleProgress();
            ClearIdleBars();
            AddIdleBars();
            resetUpgradeButtonInfos = PopulateResetUpgradeListScrollView(resetScrollView, resetUpgradesList.ResetUpgrades);
        };
        copySaveToClipboard = root.Q<Button>("copy-save-debug");
        copySaveToClipboard.clicked += () =>
        {
            saveHandler.CopySaveToClipboard();
        };

        toggleSaveEncryption = root.Q<Button>("toggle-save-encryption");
        toggleSaveEncryption.text = PlayerPrefs.GetInt("SaveUnencrypted", 0) == 0 ? "Unencrypt save" : "Encrypt save";
        toggleSaveEncryption.clicked += () =>
        {
            bool isEncrypted = PlayerPrefs.GetInt("SaveUnencrypted", 0) == 0;
            saveHandler.SetEncryption(!isEncrypted);
            toggleSaveEncryption.text = isEncrypted ? "Encrypt save" : "Unencrypt save";
        };
        
        forceShowAllUiButton = root.Q<Button>("force-show-ui");
        forceShowAllUiButton.clicked += () =>
        {
            forceShowingUi = !forceShowingUi;
            
            forceShowAllUiButton.text = forceShowingUi ? "Disable force show" : "Force show all UI";
            
            if (forceShowingUi)
            {
                resetButton.AddToClassList("force-show-feature");
                upgradeSection.AddToClassList("force-all-tabs");
            }
            else
            {
                resetButton.RemoveFromClassList("force-show-feature");
                upgradeSection.RemoveFromClassList("force-all-tabs");
            }
        };
        
        HandleFeatureReveal(true);
    }

    private void AddIdleBars()
    {
        for (int i = 0; i < idleUpgrades.Upgrades.Length; i++)
        {
            if (idleUpgrades.Upgrades[i].currentLevel != 0)
            {
                idleBars[i] = CreateIdleBar(idleUpgrades.Upgrades[i]);
                idleBarsParent.Add(idleBars[i]);
            }
        }
    }

    #endregion

    #region --------- Update ---------
    private void Update()
    {
        UpdateUpgradeButton();
        HandleFeatureReveal();
    }
    #endregion

    #region --------- Logic ---------
    private void StartConstantAnimations()
    {
        animationController.StartAnimation("flower");
        animationController.StartAnimation("socks");
        animationController.StartAnimation("window");
        animationController.StartAnimation("guy");
    }

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
        PassiveSkillButtonAvailability(passiveSkillButtonInfos, resetCoin);

        UpdateResetButtonAvailability(resetButton, totalGain);
        UpdatePrestigeButtonAvailability(prestigeButton);

        foreach (UpgradeButtonInfo clickUpgrade in clickUpgradeButtonInfos)
        {
            UpdateUpgradeLabels(clickUpgrade.Button, clickUpgrade.Cost, clickUpgrade.Upgrade, currentBuyQuantityIndex);
        }

        foreach (UpgradeButtonInfo idleUpgrade in idleUpgradeButtonInfos)
        {
            UpdateUpgradeLabels(idleUpgrade.Button, idleUpgrade.Cost, idleUpgrade.Upgrade, currentBuyQuantityIndex);
        }

        autoClickButton.SetEnabled(IsClaimed);
    }

    private void HandleFeatureReveal(bool bypassStageCheck = false)
    {
        RevealStage currentRevealStage = GetCurrentStage();

        if (revealStage != currentRevealStage || bypassStageCheck)
        {
            revealStage = currentRevealStage;

            upgradeSection.RemoveFromClassList("stage1");
            upgradeSection.RemoveFromClassList("stage2");
            upgradeSection.RemoveFromClassList("stage3");
            upgradeSection.RemoveFromClassList("stage4");

            switch (revealStage)
            {
                case RevealStage.ClickUpgrades:
                    upgradeSection.AddToClassList("stage1");
                    break;
                case RevealStage.IdleUpgrades:
                case RevealStage.ResetButton:
                    upgradeSection.AddToClassList("stage2");
                    break;
                case RevealStage.ResetUpgrades:
                    upgradeSection.AddToClassList("stage3");
                    break;
                case RevealStage.PassiveSkillUpgrades:
                    upgradeSection.AddToClassList("stage4");
                    break;
            }

            if ((int)revealStage > 2)
            {
                resetButton.RemoveFromClassList("hide-feature");
            }
            else
            {
                resetButton.AddToClassList("hide-feature");
            }
        }
    }

    private RevealStage GetCurrentStage()
    {
        if (gameController.GetResetStage() >= 3)
        {
            return RevealStage.PassiveSkillUpgrades;
        }
        if (gameController.GetResetStage() > 0)
        {
            return RevealStage.ResetUpgrades;
        }
        if (gameController.RequiredTotalGain[0] * 0.9 <= totalGain.Value)
        {
            return RevealStage.ResetButton;
        }
        if (clickUpgrades.Upgrades.Sum(upgrade => upgrade.currentLevel) >= 10) 
        {
            return RevealStage.IdleUpgrades;
        }

        return RevealStage.ClickUpgrades;
    }

    #region 1x; 5x; 10x; 100x; MAX; Breakpoint
    private void SelectBuyQuantity(int index)
    {
        currentBuyQuantityIndex = index;

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
        UpdateBuyQuantityButtonText();
    }

    private void CycleBuyQuantity()
    {
        audioController.PlaySound(SfxType.BuyQuantitySwap);

        SelectBuyQuantity((currentBuyQuantityIndex + 1) % Enum.GetValues(typeof(BuyQuantity)).Length);
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

        ClearIdleBars();

        totalGain.Value = 0;
        UpdateUpgradeButton();

        animatedLabel.text = NumberFormatter.FormatNumber(gain.Value);

        // Update the labels for click and idle upgrades
        foreach (var clickUpgrade in clickUpgradeButtonInfos)
        {
            clickUpgrade.Cost = GetNextLevelsCost(clickUpgrade.Upgrade);
            UpdateUpgradeLabels(clickUpgrade.Button, clickUpgrade.Cost, clickUpgrade.Upgrade, currentBuyQuantityIndex);
        }

        foreach (var idleUpgrade in idleUpgradeButtonInfos)
        {
            idleUpgrade.Cost = GetNextLevelsCost(idleUpgrade.Upgrade);
            UpdateUpgradeLabels(idleUpgrade.Button, idleUpgrade.Cost, idleUpgrade.Upgrade, currentBuyQuantityIndex);
        }
        totalGain.Value = 0;

        foreach (var resetUpgrade in resetUpgradeButtonInfos)
        {
            if (resetUpgrade.ResetUpgrade.isPurchased && resetScrollView.contentContainer.Contains(resetUpgrade.Button))
                resetScrollView.contentContainer.Remove(resetUpgrade.Button);
        }
        

        GameController.Instance.IncreaseResetStage();
        SelectBuyQuantity(0);
        ApplyUnlockedEffects();
    }

    private void ClearIdleBars()
    {
        idleBarsParent.Clear();
        for (int i = 0; i < idleBars.Length; i++)
        {
            idleBars[i] = null;
        }
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
            "insane-technique",           //click reset skills
            "healthy-meal-prep",
            "cool-brand-protein-powder",
            "cool-brand-creatine",
            "beast-steroid",
            "expensive-training-clothes", //idle reset skills
            "beast-mode-playlist",
            "professional-personal-trainer",
            "quality-vitamins",
            "cool-brand-preworkout",
            "sharpclicker",             //passive skills
            "leg-day",
            "pr-smash",
            "adrenaline-pump",
            "muscle-brain-connection",
            "bulk-phase",
            "power-nap",
            "beauty-sleep",
            "recovery-on",
            "no-time-to-waste",
            "sponsorship-deal"

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

            //if (resetUpgrade.isPurchased)
            //    continue;

            Button button = new Button();
            Label skillName = new Label() { text = resetUpgrade.Name };
            VisualElement showRank = new VisualElement() { name = "showRank" };

            UpgradeButtonInfo buttonInfo = new UpgradeButtonInfo
            {
                Button = button,
                ResetUpgrade = resetUpgrade,
                Rank = resetUpgrade.Rank,
            };

            //Label price = new Label()                     //later on we might use this but now the reset skills cost zero
            //{
            //    text = $"ResetRank {resetUpgrade.Rank}.",
            //    name = "price",
            //};

            //mini icon next to the lvl
            VisualElement clickUpgradeIcon = new VisualElement();
            clickUpgradeIcon.AddToClassList("click-upgrade-icon");

            string iconClass = IconClassName(resetUpgrade.Name);
            clickUpgradeIcon.AddToClassList(iconClass);


            button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(ResetUpgradeButtonClicked, buttonInfo);
            button.AddToClassList("upgradeButton");
            skillName.AddToClassList("skillNameLabel");
            //price.AddToClassList("priceLabel");
            //button.Add(price);
            showRank.AddToClassList("rank");


            button.Add(skillName);
            button.Add(clickUpgradeIcon);
            button.Add(showRank);
            buttonInfos.Add(buttonInfo);

            scrollView.contentContainer.Add(button);
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

            VisualElement levelElement = new VisualElement();
            VisualElement plusLevelElement = new VisualElement() { name = "plusLevelElement" };

            VisualElement gainIncomeElement = new VisualElement();
            VisualElement gainIncreaseElement = new VisualElement();
            VisualElement upgradeArrow = new VisualElement();
            VisualElement upgradeArrow2 = new VisualElement();

            Label levelLabel = new Label(){name = "level"};
            Label priceLabel = new Label(){name = "price"};

            Label plusLevelLabel = new Label() {name = "plusLevel"};
            Label gainIncomeLabel = new Label() {name = "gainIncome"};
            Label gainIncreaseLabel = new Label() {name = "gainIncrease" };

            //skill's icon
            VisualElement clickUpgradeIcon = new VisualElement();
            clickUpgradeIcon.AddToClassList("click-upgrade-icon");

            string iconClass = IconClassName(upgrade.Name);
            clickUpgradeIcon.AddToClassList(iconClass);

            //icon next to the price
            VisualElement pricePlusIcon = new VisualElement();
            VisualElement priceIcon = new VisualElement();


            buttonInfos[i] = buttonInfo;
            button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(UpgradeButtonClicked, buttonInfo);
            button.AddToClassList("upgradeButton");
            skillName.AddToClassList("skillNameLabel");

            levelElement.AddToClassList("levelElement");
            levelLabel.AddToClassList("levelLabel");
            plusLevelElement.AddToClassList("plusLevelElement");
            upgradeArrow.AddToClassList("upgradeArrow");            
            plusLevelLabel.AddToClassList("plusLevelLabel");

            gainIncomeElement.AddToClassList("gainIncomeElement");
            gainIncomeLabel.AddToClassList("gainIncomeLabel");
            gainIncreaseElement.AddToClassList("gainIncreaseElement");
            upgradeArrow2.AddToClassList("upgradeArrow");
            gainIncreaseLabel.AddToClassList("gainIncreaseLabel");


            levelElement.Add(levelLabel);
            plusLevelElement.Add(upgradeArrow);
            plusLevelElement.Add(plusLevelLabel);
            levelElement.Add(plusLevelElement);

            gainIncomeElement.Add(gainIncomeLabel);
            gainIncreaseElement.Add(upgradeArrow2);
            gainIncreaseElement.Add(gainIncreaseLabel);
            gainIncomeElement.Add(gainIncreaseElement);

            pricePlusIcon.AddToClassList("pricePlusIconStyle");
            priceIcon.AddToClassList("priceIconStyle");
            priceLabel.AddToClassList("priceLabel");

            pricePlusIcon.Add(priceIcon);
            pricePlusIcon.Add(priceLabel);
            button.Add(clickUpgradeIcon);
            button.Add(skillName);
            button.Add(levelElement);
            button.Add(gainIncomeElement);
            button.Add(pricePlusIcon);

            scrollView.contentContainer.Add(button);
        }
        return buttonInfos;
    }

    private UpgradeButtonInfo[] PopulatePassiveSkillListScrollView(ScrollView scrollView, PassiveSkill[] passiveSkills)
    {
        UpgradeButtonInfo[] buttonInfos = new UpgradeButtonInfo[passiveSkills.Length];
        scrollView.contentContainer.Clear();

        for (int i = 0; i < passiveSkills.Length; i++)
        {
            PassiveSkill passiveSkill = passiveSkills[i];

            if (passiveSkill.IsPurchased)
                continue;

            Button button = new Button();
            Label skillName = new Label() { text = passiveSkill.Name };

            UpgradeButtonInfo buttonInfo = new UpgradeButtonInfo
            {
                Button = button,
                PassiveSkill = passiveSkill,
                Cost = passiveSkill.Price
            };

            Label priceLabel = new Label() { name = "price", text = buttonInfo.Cost.ToString() };
            VisualElement skillInformation = new VisualElement() { name = "skillInformationPopup" };

            //skill's icon
            VisualElement passiveSkillIcon = new VisualElement();
            passiveSkillIcon.AddToClassList("click-upgrade-icon");

            string iconClass = IconClassName(passiveSkill.Name);
            passiveSkillIcon.AddToClassList(iconClass);

            //icon next to the price
            VisualElement pricePlusIcon = new VisualElement();
            VisualElement priceIcon = new VisualElement();


            buttonInfos[i] = buttonInfo;
            button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(PassiveSkillButtonClicked, buttonInfo);
            button.AddToClassList("upgradeButton");
            skillName.AddToClassList("skillNameLabel");
            skillInformation.AddToClassList("skillInformationElement");

            pricePlusIcon.AddToClassList("pricePlusIconStyle");
            priceIcon.AddToClassList("priceIconStyle");
            priceLabel.AddToClassList("priceLabel");

            pricePlusIcon.Add(priceIcon);
            pricePlusIcon.Add(priceLabel);
            button.Add(passiveSkillIcon);
            button.Add(skillName);
            button.Add(skillInformation);
            button.Add(pricePlusIcon);

            scrollView.contentContainer.Add(button);
        }
        return buttonInfos;
    }

    //On skills
    private void UpdateUpgradeLabels(Button myButton, double currentCost, Upgrade upgrade, int index)
    {
        BuyQuantity quantity = (BuyQuantity)index;

        int buyQuantitySate = upgrade.GetTargetLevelToTarget(quantity, gain.Value);

        int plusLevel = buyQuantitySate - upgrade.currentLevel;

        double gainIncrease = upgrade.GetTargetLevelIncome(upgrade.currentLevel + plusLevel) - upgrade.currentEffect;


        Label priceLabel = myButton.Q<Label>("price");
        Label levelLabel = myButton.Q<Label>("level");
        Label plusLevelLabel = myButton.Q<Label>("plusLevel");
        Label gainIncomeLabel = myButton.Q<Label>("gainIncome");
        Label gainIncreaseLabel = myButton.Q<Label>("gainIncrease");

        priceLabel.text = $"{NumberFormatter.FormatNumber(currentCost)}";
        levelLabel.text = $"level {upgrade.currentLevel}";
        plusLevelLabel.text = $"{plusLevel} lvl";
        gainIncomeLabel.text = $"{NumberFormatter.FormatNumber(upgrade.currentEffect)}/TAP";
        gainIncreaseLabel.text = $"{NumberFormatter.FormatNumber(gainIncrease)}";
    }

    private class UpgradeButtonInfo
    {
        public Button Button;
        public Upgrade Upgrade;
        public ResetUpgrade ResetUpgrade;
        public PassiveSkill PassiveSkill;
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

        audioController.PlaySound(SfxType.ResetPassiveSkillBuy);
        HandleResetUpgradeBackgroundChange(upgradeButtonInfo.ResetUpgrade);

        //resetScrollView.contentContainer.Remove(upgradeButtonInfo.Button);
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

        audioController.PlaySound(SfxType.UpgradeSkills);
        HandleBackgroundChange(upgradeButtonInfo.Upgrade);
    }

    private void PassiveSkillButtonClicked(ClickEvent clickEvent, UpgradeButtonInfo upgradeButtonInfo)
    {
        PassiveSkillBought details = new()
        {
            PassiveSkill = upgradeButtonInfo.PassiveSkill
        };

        passiveSkillBoughtEvent.Raise(details);
        resetCoinLabel.text = $"{NumberFormatter.FormatNumber(resetCoin.Value)}";
        //audioController.PlaySound(SfxType.PassiveSkillBuy);                                      -->SFX needed

        passiveScrollView.contentContainer.Remove(upgradeButtonInfo.Button);
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

        int lastUnlockedButtonIdx = buttonInfos.ToList().FindLastIndex(
            upgradeButtonInfo => upgradeButtonInfo.Upgrade.currentLevel != 0
        );

        // Hide all buttons after the next unlockable upgrade
        if (lastUnlockedButtonIdx < buttonInfos.Length - 1)
        {
            for (int i = 0; i <= lastUnlockedButtonIdx + 1; i++)
            {
                buttonInfos[i].Button.style.display = DisplayStyle.Flex;
            }
            for (int i = lastUnlockedButtonIdx + 2; i < buttonInfos.Length; i++)
            {
                buttonInfos[i].Button.style.display = DisplayStyle.None;
            }
        }

        //TO DO: ezt cserelni hogy a foldout helyett a scrollView elemek legyenek kezelve
    }

    private void UpdateResetUpgradeButtonAvailability(UpgradeButtonInfo[] buttonInfos)
    {
        int currentResetStage = GameController.Instance.GetResetStage();
        VisualElement showRank; 


        for (int i = 0; i < buttonInfos.Length; i++)
        {
            bool shouldShow = false;
            showRank = buttonInfos[i].Button.Q<VisualElement>("showRank");

            if (buttonInfos[i].ResetUpgrade.Rank == currentResetStage && !buttonInfos[i].ResetUpgrade.isPurchased)
            {
                shouldShow = true;
                showRank.AddToClassList($"rank{currentResetStage - 1}");
            }
            else
            {
                for (int j = 0; j < buttonInfos.Length; j++) 
                {
                    if (buttonInfos[j].ResetUpgrade.Rank == buttonInfos[i].ResetUpgrade.Rank + 1 && buttonInfos[j].ResetUpgrade.Name == buttonInfos[i].ResetUpgrade.Name && buttonInfos[i].ResetUpgrade.isPurchased)
                    {
                        shouldShow = true;
                        showRank.AddToClassList($"rank{currentResetStage}");
                        buttonInfos[j].Button.style.display = DisplayStyle.Flex;
                        buttonInfos[j].Button.SetEnabled(buttonInfos[j].ResetUpgrade.Rank <= currentResetStage && IsClaimed && !buttonInfos[j].ResetUpgrade.isPurchased);
                    }
                }
            }

            buttonInfos[i].Button.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            buttonInfos[i].Button.SetEnabled(buttonInfos[i].ResetUpgrade.Rank <= currentResetStage && IsClaimed && !buttonInfos[i].ResetUpgrade.isPurchased);
        }


        //foreach (UpgradeButtonInfo upgradeButtonInfo in buttoninfos)
        //{
        //    showRank = upgradeButtonInfo.Button.Q<VisualElement>("showRank");
        //    if (upgradeButtonInfo.ResetUpgrade.Rank == currentResetStage)
        //    {
        //        showRank.AddToClassList($"rank{currentResetStage-1}");
        //    }

        //    upgradeButtonInfo.Button.SetEnabled(upgradeButtonInfo.ResetUpgrade.Rank <= currentResetStage && IsClaimed);
        //    upgradeButtonInfo.Button.style.display = upgradeButtonInfo.ResetUpgrade.Rank == currentResetStage ? DisplayStyle.Flex : DisplayStyle.None;
        //}
    }

    private void PassiveSkillButtonAvailability(UpgradeButtonInfo[] buttonInfos, LargeNumber resetCoin)
    {
        foreach (UpgradeButtonInfo upgradeButtonInfo in buttonInfos)
        {
            upgradeButtonInfo?.Button.SetEnabled(upgradeButtonInfo.Cost <= NumberFormatter.RoundCalculatedNumber(resetCoin.Value));
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
        }

        if (idleScrollView != null)
        {
            idleScrollView.style.display = visibleScroll == idleScrollView ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (resetScrollView != null)
        {
            resetScrollView.style.display = visibleScroll == resetScrollView ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (passiveScrollView != null)
        {
            passiveScrollView.style.display = visibleScroll == passiveScrollView ? DisplayStyle.Flex : DisplayStyle.None;
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
            audioController.PlaySound(SfxType.CloseSkills);

            upgradePanelAnimation = StartCoroutine(AnimateUpgradePanel(false));
            upgradePanelVisible = false;
            currentVisibleUpgrade = null;

            upgradeSection.RemoveFromClassList("clickActive");
            upgradeSection.RemoveFromClassList("idleActive");
            upgradeSection.RemoveFromClassList("resetActive");
            upgradeSection.RemoveFromClassList("passiveActive");

            return;
        }

        ShowScrollView(GetScrollViewByName(contentName));
        currentVisibleUpgrade = contentName;

        if (!upgradePanelVisible)
        {
            audioController.PlaySound(SfxType.OpenSkills);

            upgradePanelAnimation = StartCoroutine(AnimateUpgradePanel(true));
            upgradePanelVisible = true;
        }
        else
        {
            audioController.PlaySound(SfxType.SwapSkillTabWhileOpen);
        }
    }

    private ScrollView GetScrollViewByName(string name)
    {
        return name switch
        {
            "click" => clickScrollView,
            "idle" => idleScrollView,
            "reset" => resetScrollView,
            "passive" => passiveScrollView,
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


    private void UpdateVolumeUI(VisualElement container, int level)
    {
        container.Clear();
        container.style.flexDirection = FlexDirection.Row;

        for (int i = 0; i < 7; i++)
        {
            var dot = new VisualElement();
            dot.AddToClassList("indicator");
            dot.AddToClassList(i < level ? "active" : "inactive");
            container.Add(dot);
        }
    }

    private void SetupVolumeControls()
    {
        var musicIndicators = root.Q<VisualElement>("music-volume-indicators");
        var decreaseMusicBtn = root.Q<Button>("decrease-music-volume");
        var increaseMusicBtn = root.Q<Button>("increase-music-volume");

        var sfxIndicators = root.Q<VisualElement>("sfx-volume-indicators");
        var decreaseSfxBtn = root.Q<Button>("decrease-sfx-volume");
        var increaseSfxBtn = root.Q<Button>("increase-sfx-volume");

        audioController.SetMusicVolume(musicLevel / 6f);
        audioController.SetSfxVolume(sfxLevel / 6f);

        UpdateVolumeUI(musicIndicators, musicLevel);
        UpdateVolumeUI(sfxIndicators, sfxLevel);

        decreaseMusicBtn.clicked += () =>
        {
            if (musicLevel > 0)
            {
                musicLevel--;
                UpdateVolumeUI(musicIndicators, musicLevel);
                if (!audioController.IsMuted())
                    audioController.SetMusicVolume(musicLevel / 6f);
            }
        };

        increaseMusicBtn.clicked += () =>
        {
            if (musicLevel < 7)
            {
                musicLevel++;
                UpdateVolumeUI(musicIndicators, musicLevel);
                if (!audioController.IsMuted())
                    audioController.SetMusicVolume(musicLevel / 6f);
            }
        };

        decreaseSfxBtn.clicked += () =>
        {
            if (sfxLevel > 0)
            {
                sfxLevel--;
                UpdateVolumeUI(sfxIndicators, sfxLevel);
                if (!audioController.IsMuted())
                    audioController.SetSfxVolume(sfxLevel / 6f);
            }
        };

        increaseSfxBtn.clicked += () =>
        {
            if (sfxLevel < 7)
            {
                sfxLevel++;
                UpdateVolumeUI(sfxIndicators, sfxLevel);
                if (!audioController.IsMuted())
                    audioController.SetSfxVolume(sfxLevel / 6f);
            }
        };
    }

    private void SetupMuteButtons()
    {
        soundOnButton = root.Q<Button>("sound-on-button");
        soundOffButton = root.Q<Button>("sound-off-button");

        Color activeColor = new Color(1f, 0.82f, 0.2f);
        Color  inactiveColor = new Color(1f, 0.91f, 0.62f);

        void UpdateMuteButtons()
        {
            bool isMuted = audioController.IsMuted();
            soundOnButton.style.backgroundColor = isMuted ? inactiveColor : activeColor;
            soundOffButton.style.backgroundColor = isMuted ? activeColor : inactiveColor;
        }

        soundOnButton.clicked += () =>
        {
            if (audioController.IsMuted())
            {
                audioController.ToggleMute(musicLevel, sfxLevel);
                UpdateMuteButtons();
            }
        };

        soundOffButton.clicked += () =>
        {
            if (!audioController.IsMuted())
            {
                audioController.ToggleMute(musicLevel, sfxLevel);
                UpdateMuteButtons();
            }
        };

        UpdateMuteButtons();
    }


    private void InitializeBackgroundTriggers()
    {
        backgroundTriggers = new Dictionary<string, Action>
        {
            //click skills
            {
                "right technique", () =>
                {
                    desk.RemoveFromClassList("desk");
                    desk.AddToClassList("deskBook");
                }
            },

            {
                "meal prep", () =>
                {
                    pizzaBurger.RemoveFromClassList("pizza");
                    pizzaBurger.AddToClassList("burger");
                }
            },

            {
                "protein powder", () =>
                {
                    protein.style.display = DisplayStyle.Flex;

                    animationController.PauseAnimation("socks");
                    animationController.SetVisibility("socks", false);
                }
            },

            {
                "creatine", () =>
                {
                    creatine.style.display = DisplayStyle.Flex;
                }
            },

            //idle skills
            {
                "training clothes", () =>
                {
                    animationController.PauseAnimation("guy");
                    animationController.SetVisibility("guy", false);

                    animationController.SetVisibility("guyTraining", true);
                    animationController.StartAnimation("guyTraining");
                    animationController.ResumeAnimation("guyTraining");
                }
            },

            {
                "gym playlist", () =>
                {
                    animationController.StartAnimation("speakerLeft");
                    animationController.StartAnimation("speakerRight");
                    animationController.ResumeAnimation("speakerLeft");
                    animationController.ResumeAnimation("speakerRight");
                }
            },

            { 
                "personal trainer", () =>
                {
                    shelf.style.display = DisplayStyle.None;

                    animationController.ResumeAnimation("trainer");

                    animationController.StartAnimation("trainer");
                    animationController.SetVisibility("trainer", true);
                }
            },

            {
                "vitamins", () =>
                {
                    vitamins.style.display = DisplayStyle.Flex;
                }
            },

            {
                "preworkout", () =>
                {
                    preworkout.style.display = DisplayStyle.Flex;
                }
            },

            //reset skill
            {
                "healthy meal prep", () =>
                {
                    pizzaBurger.style.display = DisplayStyle.None;

                    animationController.StartAnimation("mealPrep");
                }
            }
        };
    }

    private void ResetBackgroundTriggers()
    {
        resetBackgroundTriggers = new Dictionary<string, Action>
        {
            //click skills
            {
                "right technique", () =>
                {
                    desk.RemoveFromClassList("deskBook");
                    desk.AddToClassList("desk");
                }
            },

            {
                "meal prep", () =>
                {
                    pizzaBurger.RemoveFromClassList("burger");
                    pizzaBurger.AddToClassList("pizza");
                }
            },

            {
                "protein powder", () =>
                {
                    protein.style.display = DisplayStyle.None;

                    animationController.ResumeAnimation("socks");
                    animationController.SetVisibility("socks",true);
                }
            },

            {
                "creatine", () =>
                {
                    creatine.style.display = DisplayStyle.None;
                }
            },

            //idle skills
            {
                "training clothes", () =>
                {
                    animationController.PauseAnimation("guyTraining");
                    animationController.SetVisibility("guyTraining",false);

                    animationController.ResumeAnimation("guy");
                    animationController.SetVisibility("guy",true);
                }
            },

            {
                "gym playlist", () =>
                {
                    animationController.PauseAnimation("speakerLeft");
                    animationController.PauseAnimation("speakerRight");
                }
            },

            {
                "personal trainer", () =>
                {
                    animationController.PauseAnimation("trainer");
                    animationController.SetVisibility("trainer",false);
                    shelf.style.display = DisplayStyle.Flex;
                    
                }
            },

            {
                "vitamins", () =>
                {
                    vitamins.style.display = DisplayStyle.None;
                }
            },

            {
                "preworkout", () =>
                {
                    preworkout.style.display = DisplayStyle.None;
                }
            }
        };
    }

    private void HandleBackgroundChange(Upgrade upgrade)
    {
        string skillName = upgrade.Name.ToLower();

        if (upgrade.currentLevel > 0 && backgroundTriggers.TryGetValue(skillName, out var action))
        {
            action.Invoke();
        }

        if(upgrade.currentLevel == 0 && resetBackgroundTriggers.TryGetValue(skillName, out var resetAction))
        {
            resetAction.Invoke();
        }
    }

    private void HandleResetUpgradeBackgroundChange(ResetUpgrade resetUpgrade)
    {
        string resetSkillName = resetUpgrade.Name.ToLower();

        if(resetUpgrade.isPurchased && backgroundTriggers.TryGetValue(resetSkillName, out var action))
        {
            action.Invoke();
        }
    }

    //apply background change effects based on skill unlocks on game restart
    private void ApplyUnlockedEffects()
    {
        foreach (var upgrade in clickUpgrades.Upgrades)
        {
            HandleBackgroundChange(upgrade);
        }

        foreach (var upgrade in idleUpgrades.Upgrades)
        {
            HandleBackgroundChange(upgrade);
        }

        foreach (var resetUpgrade in resetUpgradesList.ResetUpgrades)
        {
            HandleResetUpgradeBackgroundChange(resetUpgrade);
        }
    }
    #endregion
}

enum RevealStage
{
    ClickUpgrades = 1,
    IdleUpgrades = 2,
    ResetButton = 3,
    ResetUpgrades = 4,
    PassiveSkillUpgrades = 5,
}
