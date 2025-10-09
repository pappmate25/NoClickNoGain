using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Reflection.Emit;
using Unity.Properties;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private LargeNumber resetCoin;
    [SerializeField] private UpgradeList clickUpgrades;
    [SerializeField] private UpgradeList idleUpgrades;
    [SerializeField] private ResetUpgradeList resetUpgradesList;
    [SerializeField] private PassiveSkillList passiveSkillsList;
    [SerializeField] private QuitDate quitDate;
    [SerializeField] private GameEvent gainChangedEvent;
    [SerializeField] private GameEvent upgradeBoughtEvent;
    [SerializeField] private GameEvent resetUpgradeBoughtEvent;
    [SerializeField] private GameEvent passiveSkillBoughtEvent;
    [SerializeField] private GameEvent resetEvent;
    [SerializeField] private GameEvent saveLoadedFromClipboardEvent;
    //[SerializeField] private GameEvent GainChangedEvent;
    [SerializeField] private IntVariable selectedBuyQuantity;
    [SerializeField] private GameObject animatedGranny;
    [SerializeField] private AudioController audioController;
    [SerializeField] private SaveHandler saveHandler;
    [SerializeField] private GameController gameController;
    [SerializeField] private TutorialController tutorialController;

    private VisualElement root;

    private Label animatedLabel;

    public static string IdleNameFormat(string name) => name.ToLowerInvariant().Replace(" ", "-") + "-bar";

    private Label resetCoinLabel;

    private ScrollView clickScrollView;
    private ScrollView idleScrollView;
    private ScrollView resetScrollView;
    private ScrollView passiveScrollView;

    //for resetScrollview
    private VisualElement columns;
    private VisualElement leftColumn;
    private VisualElement rightColumn;

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
    public static bool IsClaimed = true;
    private VisualElement blackBg;

    //reset warning popup
    private VisualElement resetWarningPopup;
    private Button resetWarningBack;
    private Button resetWarningConfirm;
    private Label informationLabel;

    //popups
    private Button optionsButton, optionsExitButton, warningResetButton;
    private Button creditsButton, creditsBackButton, hardResetBackButton, hardResetButton;
    private VisualElement optionsPopup, warningPopup, creditsPopup;
    private Label warningLabel;

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

    //idle progress bars
    private ProgressBar[] idleProgressBars;

    //autoclick
    private Button autoClickButton;
    private AutoClicker autoClicker;

    //prestige
    private Button prestigeButton;

    //volume
    private int musicLevel = 4;
    private int sfxLevel = 4;

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
    private VisualElement debugElementsParent;
    private Button loadSaveFromClipboard;
    private Button copySaveToClipboard;
    private Button toggleSaveEncryption;
    private Button forceShowAllUiButton;
    private bool forceShowingUi = false;
    
    //analytics popup
    private VisualElement analyticsPopup;
    private Button analyticsCloseButton;
    private Label analyticsNoticeLabel;

    private bool isDirty = true;

    //Story video
    [SerializeField] private VideoPlayer storyVideoPlayer;
    [SerializeField] private RenderTexture storyVideoRT;
    private VisualElement storyPanel;
    private Button storyButton;
    private Button storySkipButton;
    

    #region --------- Start ---------
    void Start()
    {
        resetRanks = new Texture2D[2];
        for (int i = 0; i < resetRanks.Length; i++)
        {
            resetRanks[i] = Resources.Load<Texture2D>($"UI/Upgrade section/Reset upgrades/reset-rank-{i}");
        }

        root = GetComponent<UIDocument>().rootVisualElement;

        //Story video
        storyVideoPlayer.url = Path.Combine(Application.streamingAssetsPath, "story.mp4");
        storyPanel = root.Q<VisualElement>("story-panel");
        storySkipButton = root.Q<Button>("story-skip-button");
        storySkipButton.clicked += OnSkipStoryClicked;
        storyButton = root.Q<Button>("story-button");
        storyButton.clicked += () => ShowStoryVideo();
        if (PlayerPrefs.GetInt("story-watched", 0) != 1)
        {
            ShowStoryVideo();
        }
        else
        {
            storyVideoPlayer.Stop();
        }

        upgradeSection = root.Q<VisualElement>("upgrade-section");
        upgradeSectionLabel = root.Q<Label>("upgrade-section-label");

        clickScrollView = root.Q<ScrollView>("clickScrollView");
        idleScrollView = root.Q<ScrollView>("idleScrollView");
        resetScrollView = root.Q<ScrollView>("resetScrollView");
        passiveScrollView = root.Q<ScrollView>("passiveScrollView");

        //for resetScrollview
        columns = root.Q<VisualElement>("columns");
        leftColumn = root.Q<VisualElement>("left-column");
        rightColumn = root.Q<VisualElement>("right-column");

        clickUpgradeButton = root.Q<Button>("click-btn");
        idleUpgradeButton = root.Q<Button>("idle-btn");
        resetUpgradeButton = root.Q<Button>("reset-btn");
        passiveSkillButton = root.Q<Button>("passive-btn");

        ShowScrollView(resetScrollView);

        //buttons pressed event handlers
        clickUpgradeButton.clicked += () =>
        {
            if(!ClickAllowed("click-btn")) { return; }

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
            if (!ClickAllowed("idle-btn")) { return; }

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
            if (!ClickAllowed("reset-btn")) { return; }

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
            if (!ClickAllowed("passive-btn")) { return; }

            upgradeSection.RemoveFromClassList("idleActive");
            upgradeSection.RemoveFromClassList("clickActive");
            upgradeSection.RemoveFromClassList("resetActive");
            upgradeSection.AddToClassList("passiveActive");
            upgradeSectionLabel.text = "Passive Skills";

            ToggleUpgradePanel("passive");
            ShowScrollView(passiveScrollView);
        };

        animatedLabel = root.Q<Label>("gain-label");

        //welcome back
        //idle time
        blackBg = root.Q<VisualElement>("black-bg");
        popup = root.Q<VisualElement>("welcome-back-popup");
        if (gameState.IsFirstIdleUnlocked)
        {
            IsClaimed = false;
            popup.SetEnabled(true);
            popup.style.display = DisplayStyle.Flex;
            blackBg.style.display = DisplayStyle.Flex;
        }
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

        //reset warning popup
        resetWarningPopup = root.Q<VisualElement>("reset-warning-popup");
        resetWarningBack = root.Q<Button>("reset-warning-back-button");
        resetWarningConfirm = root.Q<Button>("reset-warning-confirm-button");
        informationLabel = root.Q<Label>("information-label");

        //idle gain earned
        idleGainEarned = root.Q<Label>("idle-gain-earned-label");
        idleGainEarned.text = $"+{NumberFormatter.FormatNumber(gameState.IdleGainWhileAway)}";

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


        resetButton = root.Q<Button>("reset-progress-button");
        resetButton.clicked += ShowResetWarning;
        resetWarningBack.clicked += ResetBackButtonClicked;
        resetWarningConfirm.clicked += ResetConfirmButtonClicked;

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
            if (!ClickAllowed("buy-quantity-toggle-button")) { return; }

            CycleBuyQuantity();
        };

        SelectBuyQuantity(currentBuyQuantityIndex);
        quantityLabel.text = GetBuyQuantityLabel((BuyQuantity)currentBuyQuantityIndex);

        //Popup
        SetupOptionsUI();
        OptionsPopupEvents();
        CreditsPopupEvents();
        WarningPopupEvents();
        SetupMuteButtons(); //on-off buttons
        StartConstantAnimations();

        //debug elements
        loadSaveFromClipboard = root.Q<Button>("load-save-debug");
        loadSaveFromClipboard.clicked += () =>
        {
            saveHandler.LoadFromClipboard();
            SelectBuyQuantity(selectedBuyQuantity.Value); // Needed to refresh the upgrade button infos.
            gameState.ResetIdleProgress();
            resetUpgradeButtonInfos = PopulateResetUpgradeListScrollView(resetScrollView, resetUpgradesList.ResetUpgrades);
            
            saveLoadedFromClipboardEvent.Raise(NoDetails.Instance);
        };
        copySaveToClipboard = root.Q<Button>("copy-save-debug");
        copySaveToClipboard.clicked += () =>
        {
            saveHandler.CopySaveToClipboard();
        };

        toggleSaveEncryption = root.Q<Button>("toggle-save-encryption");
        toggleSaveEncryption.clicked += () =>
        {
            Debug.Log("Disabled, sorry");
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

        debugElementsParent = root.Q<VisualElement>("debug-buttons");
        debugElementsParent.style.display = Debug.isDebugBuild ? DisplayStyle.Flex : DisplayStyle.None;

        HandleFeatureReveal(true);
        //analytics popup
        if (PlayerPrefs.GetInt("analytics-ack", 0) != 1)
        {
            bool blackBgPreviousState = blackBg.style.display == DisplayStyle.Flex;
            
            blackBg.style.display = DisplayStyle.Flex;
            analyticsPopup = root.Q<VisualElement>("analytics-popup");
            analyticsPopup.style.display = PlayerPrefs.GetInt("analytics-ack", 0) != 1 ? DisplayStyle.Flex : DisplayStyle.None;
            analyticsCloseButton = root.Q<Button>("understand-button");
            analyticsNoticeLabel = root.Q<Label>("analytics-notice-text");
            analyticsNoticeLabel.text =
                "WE COLLECT <color=#FFD133>ANONYMOUS</color> SESSION DATA\n" +
                "DURING YOUR PLAY. THIS DATA <color=#FFD133>CANNOT</color>\n" +
                "BE USED TO <color=#FFD133>IDENTIFY</color> YOU IN ANY WAY\n" +
                "AS IT IS TIED TO EACH SESSION AND\n" +
                "IMPRECISE.";

            analyticsCloseButton.clicked += () =>
            {
                analyticsPopup.style.display = DisplayStyle.None;
                PlayerPrefs.SetInt("analytics-ack", 1);
                tutorialController.StartTutorialAfterAnalytics();
                blackBg.style.display = blackBgPreviousState ? DisplayStyle.Flex : DisplayStyle.None;
            };
        }
    }
    #endregion

    #region --------- Update ---------
    private void Update()
    {
        if (isDirty)
        {
            animatedLabel.text = NumberFormatter.FormatNumber(gameState.Gain);
            UpdateUpgradeButton();
            HandleFeatureReveal();
            isDirty = false;
            
        }
    }
    #endregion

    #region --------- Logic ---------
    public void SetUiDirty()
    {
        isDirty = true;
    }
    private bool wasMusicMuted;
    private void ShowStoryVideo()
    {
        PlayerPrefs.SetInt("story-watched", 0);
        storyPanel.style.display = DisplayStyle.Flex;
        storyVideoPlayer.Stop();
        storyVideoPlayer.Play();
        storyVideoPlayer.loopPointReached += OnStoryVideoFinished;

        wasMusicMuted = audioController.IsMusicMuted();
        audioController.MuteMusicTemporarily(true);
    }

    private void OnStoryVideoFinished(VideoPlayer vp)
    {
        storyPanel.style.display = DisplayStyle.None;
        storyVideoPlayer.Stop();
        storyVideoPlayer.loopPointReached -= OnStoryVideoFinished;
        PlayerPrefs.SetInt("story-watched", 1);

        audioController.MuteMusicTemporarily(wasMusicMuted);
    }

    private void OnSkipStoryClicked()
    {
        storyPanel.style.display = DisplayStyle.None;
        storyVideoPlayer.Stop();
        storyVideoPlayer.loopPointReached -= OnStoryVideoFinished;
        PlayerPrefs.SetInt("story-watched", 1);

        audioController.MuteMusicTemporarily(wasMusicMuted);
    }

    private void StartConstantAnimations()
    {
        animationController.StartAnimation("flower");
        animationController.StartAnimation("socks");
        animationController.StartAnimation("window");
        animationController.StartAnimation("guy");
    }

    private void SetupAnimatedLabelBinding()
    {
        // TODO: might not work
        /*var binding = new DataBinding
        {
            dataSource = gameState.Gain,
            bindingMode = BindingMode.ToTarget,
            updateTrigger = BindingUpdateTrigger.OnSourceChanged
        };
        var largeNumberConverterGroup = new ConverterGroup("LargeNumberToString");
        largeNumberConverterGroup.AddConverter((ref double gain) => NumberFormatter.FormatNumber(gain));
        binding.ApplyConverterGroupToUI(largeNumberConverterGroup);
        // TODO: possibly change binding to update manually
        animatedLabel.SetBinding(nameof(Label.text), binding);
        animatedLabel.text = NumberFormatter.FormatNumber(gameState.Gain);*/
    }

    public void UpdateUpgradeButton()
    {
        if ((BuyQuantity)selectedBuyQuantity.Value == BuyQuantity.MAX)
        {
            foreach (var clickUpgrade in clickUpgradeButtonInfos)
            {
                clickUpgrade.TargetLevel = clickUpgrade.Upgrade.GetMaxAchievableLevel(gameState.Gain);
                clickUpgrade.Cost = clickUpgrade.Upgrade.GetCumulativeCost(clickUpgrade.TargetLevel);
            }

            foreach (var idleUpgrade in idleUpgradeButtonInfos)
            {
                idleUpgrade.TargetLevel = idleUpgrade.Upgrade.GetMaxAchievableLevel(gameState.Gain);
                idleUpgrade.Cost = idleUpgrade.Upgrade.GetCumulativeCost(idleUpgrade.TargetLevel);
            }
        }

        UpdateButtonAvailability(clickUpgradeButtonInfos, gameState.Gain);
        UpdateButtonAvailability(idleUpgradeButtonInfos, gameState.Gain);
        UpdateResetUpgradeButtonAvailability(resetUpgradeButtonInfos);
        PassiveSkillButtonAvailability(passiveSkillButtonInfos, resetCoin);

        UpdateResetButtonAvailability(resetButton);
        UpdatePrestigeButtonAvailability(prestigeButton);

        foreach (UpgradeButtonInfo clickUpgrade in clickUpgradeButtonInfos)
        {
            UpdateUpgradeLabels(clickUpgrade, clickUpgrade.Cost, clickUpgrade.Upgrade, currentBuyQuantityIndex);
        }

        foreach (UpgradeButtonInfo idleUpgrade in idleUpgradeButtonInfos)
        {
            UpdateUpgradeLabels(idleUpgrade, idleUpgrade.Cost, idleUpgrade.Upgrade, currentBuyQuantityIndex);
        }


        HandleIdleBar(idleUpgrades.Upgrades);

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
        if (gameController.RequiredTotalGain[0] * 0.9 <= gameState.TotalGain)
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
    public void SelectBuyQuantity(int index)
    {
        currentBuyQuantityIndex = index;

        BuyQuantity quantity = (BuyQuantity)index;
        selectedBuyQuantity.Value = index;

        for (int i = 0; i < clickUpgrades.Upgrades.Length; i++)
        {
            int targetLevel = clickUpgrades.Upgrades[i].GetTargetLevelToTarget(quantity, gameState.Gain);

            clickUpgradeButtonInfos[i].TargetLevel = targetLevel;
            clickUpgradeButtonInfos[i].Cost = clickUpgrades.Upgrades[i].GetCumulativeCost(targetLevel);
        }

        for (int i = 0; i < idleUpgrades.Upgrades.Length; i++)
        {
            int targetLevel = idleUpgrades.Upgrades[i].GetTargetLevelToTarget(quantity, gameState.Gain);

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
        gameState.AddGain(gameState.IdleGainWhileAway, GainChangeType.WelcomeBackClaimed);
        
        popup.SetEnabled(false);
        popup.style.display = DisplayStyle.None;
        blackBg.style.display = DisplayStyle.None;
        IsClaimed = true;
    }

    private void TwoXButtonClicked()
    {
        popup.SetEnabled(false);
        popup.style.display = DisplayStyle.None;
        blackBg.style.display = DisplayStyle.None;
        IsClaimed = true;
        
        gameState.AddGain(gameState.IdleGainWhileAway, GainChangeType.WelcomeBackClaimed);
    }


    private void UpdateIdleTimeLabels(TimeSpan elapsed)
    {
        daysLabel.text = elapsed.Days.ToString();
        hoursLabel.text = elapsed.Hours.ToString();
        minutesLabel.text = elapsed.Minutes.ToString();
    }

    private void ShowResetWarning()
    {
        resetWarningPopup.style.display = DisplayStyle.Flex;
        resetWarningPopup.SetEnabled(true);
        informationLabel.text =
            "Are you sure you want to reset?\n " +
            "You’ll <color=#FFD133>start over from the beginning</color>,\n" +
            "but you’ll keep your earned bonuses.\n" +
            "<color=#FFD133>Progress will be wiped,</color>\n" +
            "but the journey starts stronger!";
    }

    private void ResetBackButtonClicked()
    {
        audioController.PlaySound(SfxType.MenuButtons);
        resetWarningPopup.style.display = DisplayStyle.None;
    }

    private void ResetConfirmButtonClicked()
    {
        audioController.PlaySound(SfxType.MenuButtons);
        resetWarningPopup.style.display = DisplayStyle.None;

        ResetButtonClicked();
    }

    private void ResetButtonClicked()
    {
        gameState.Reset();
        
        GameController.Instance.GetResetCoin();

        resetCoinLabel.text = $"{NumberFormatter.FormatNumber(resetCoin.Value)}";
        isResetPressed = true;

        UpdateUpgradeButton();

        animatedLabel.text = NumberFormatter.FormatNumber(gameState.Gain);

        // Update the labels for click and idle upgrades
        foreach (var clickUpgrade in clickUpgradeButtonInfos)
        {
            clickUpgrade.Cost = GetNextLevelsCost(clickUpgrade.Upgrade);
            UpdateUpgradeLabels(clickUpgrade, clickUpgrade.Cost, clickUpgrade.Upgrade, currentBuyQuantityIndex);
        }

        foreach (var idleUpgrade in idleUpgradeButtonInfos)
        {
            idleUpgrade.Cost = GetNextLevelsCost(idleUpgrade.Upgrade);
            UpdateUpgradeLabels(idleUpgrade, idleUpgrade.Cost, idleUpgrade.Upgrade, currentBuyQuantityIndex);
        }

        foreach (var resetUpgrade in resetUpgradeButtonInfos)
        {
            var parent = resetUpgrade.Button.parent;
            if (resetUpgrade.ResetUpgrade.isPurchased && resetScrollView.contentContainer.Contains(resetUpgrade.Button))
                parent.Remove(resetUpgrade.Button);
        }

        GameController.Instance.IncreaseResetStage();
        SelectBuyQuantity(0);
        ApplyUnlockedEffects();
    }

    private static void UpdateResetButtonAvailability(Button button)
    {
        button.SetEnabled(GameController.Instance.CanReset() && IsClaimed);
    }

    private void PrestigeButtonClicked()
    {
        ResetButtonClicked();
    }

    private void UpdatePrestigeButtonAvailability(Button button)
    {
        button.SetEnabled(gameState.CanPrestige() && IsClaimed);
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
                ShowRank = showRank,
            };

            //Label price = new Label()                     //later on we might use this but now the reset skills cost zero
            //{
            //    text = $"ResetRank {resetUpgrade.Rank}.",
            //    name = "price",
            //};

            //mini icon next to the lvl
            VisualElement clickUpgradeIcon = new VisualElement();
            clickUpgradeIcon.AddToClassList("reset-upgrade-icon");

            string iconClass = IconClassName(resetUpgrade.Name);
            clickUpgradeIcon.AddToClassList(iconClass);


            button.RegisterCallback<ClickEvent, UpgradeButtonInfo>(ResetUpgradeButtonClicked, buttonInfo);
            //columns.AddToClassList("columnsStyle");
            //leftColumn.AddToClassList("leftColumnStyle");
            //rightColumn.AddToClassList("rightColumnStyle");
            button.AddToClassList("resetUpgradeButton");
            skillName.AddToClassList("resetSkillNameLabel");
            //price.AddToClassList("priceLabel");
            //button.Add(price);
            showRank.AddToClassList("rank");

            columns.Add(leftColumn);
            columns.Add(rightColumn);

            button.Add(skillName);
            button.Add(clickUpgradeIcon);
            button.Add(showRank);

            if (resetUpgrade.Upgrade.IsClickUpgrade)
            {
                leftColumn.Add(button);
            }
            else
            {
                rightColumn.Add(button);
            }

            buttonInfos.Add(buttonInfo);

        }
        scrollView.contentContainer.Add(columns);

        return buttonInfos.ToArray();
    }

    private UpgradeButtonInfo[] PopulateUpgradeListScrollView(ScrollView scrollView, Upgrade[] upgrades)
    {
        UpgradeButtonInfo[] buttonInfos = new UpgradeButtonInfo[upgrades.Length];
        scrollView.contentContainer.Clear();

        for (int i = 0; i < upgrades.Length; i++)
        {
            Upgrade upgrade = upgrades[i];
            Button button = new Button() { name = upgrade.name.ToLower().Replace(" ", "-") };
            Label skillName = new Label() { text = upgrade.Name };
            
            VisualElement levelElement = new VisualElement();
            VisualElement plusLevelElement = new VisualElement() { name = "plusLevelElement" };

            VisualElement gainIncomeElement = new VisualElement();
            VisualElement gainIncreaseElement = new VisualElement();
            VisualElement upgradeArrow = new VisualElement();
            VisualElement upgradeArrow2 = new VisualElement();

            Label levelLabel = new Label() { name = "level" };
            Label priceLabel = new Label() { name = "price" };

            Label plusLevelLabel = new Label() { name = "plusLevel" };
            Label gainIncomeLabel = new Label() { name = "gainIncome" };
            Label gainIncreaseLabel = new Label() { name = "gainIncrease" };

            UpgradeButtonInfo buttonInfo = new UpgradeButtonInfo
            {
                Button = button,
                Upgrade = upgrade,
                Cost = GetNextLevelsCost(upgrade),
                
                LevelLabel = levelLabel,
                PlusLevelLabel = plusLevelLabel,
                GainIncomeLabel = gainIncomeLabel,
                GainIncreaseLabel = gainIncreaseLabel,
                PriceLabel = priceLabel,
            };

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
    private void UpdateUpgradeLabels(UpgradeButtonInfo buttonInfo, double currentCost, Upgrade upgrade, int index)
    {
        BuyQuantity quantity = (BuyQuantity)index;

        int buyQuantitySate = upgrade.GetTargetLevelToTarget(quantity, gameState.Gain);

        int plusLevel = buyQuantitySate - upgrade.currentLevel;

        double gainIncrease = upgrade.GetTargetLevelIncome(upgrade.currentLevel + plusLevel) - upgrade.currentEffect;

        buttonInfo.PriceLabel.text = $"{NumberFormatter.FormatNumber(currentCost)}";
        buttonInfo.LevelLabel.text = $"level {upgrade.currentLevel}";
        buttonInfo.PlusLevelLabel.text = $"{plusLevel} lvl";
        if (upgrade.IsClickUpgrade)
        {
            buttonInfo.GainIncomeLabel.text = $"{NumberFormatter.FormatNumber(upgrade.currentEffect)}/TAP";
        }
        else
        {
            double progressTime = upgrade.IdleUpgradeDetails.ProgressDuration / 60;

            buttonInfo.GainIncomeLabel.text = $"{NumberFormatter.FormatNumber(upgrade.currentEffect)}/{(progressTime >=1 ? progressTime + " MIN" : progressTime*60 + " SEC")}";
        }
        buttonInfo.GainIncreaseLabel.text = $"{NumberFormatter.FormatNumber(gainIncrease)}";
    }

    private class UpgradeButtonInfo
    {
        public Button Button;
        
        public Label PriceLabel;
        public Label LevelLabel;
        public Label PlusLevelLabel;
        public Label GainIncomeLabel;
        public Label GainIncreaseLabel;

        public VisualElement ShowRank;
        
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

    }

    private void UpgradeButtonClicked(ClickEvent clickEvent, UpgradeButtonInfo upgradeButtonInfo)
    {
        if (!ClickAllowed(upgradeButtonInfo.Button.name)) {  return; }

        BuyQuantity quantity = (BuyQuantity)selectedBuyQuantity.Value;

        if (!gameState.BuyUpgrade(upgradeButtonInfo.Upgrade, upgradeButtonInfo.TargetLevel))
        {
            return;
        }

        upgradeButtonInfo.TargetLevel = upgradeButtonInfo.Upgrade.GetTargetLevelToTarget(quantity, gameState.Gain);
        upgradeButtonInfo.Cost = upgradeButtonInfo.Upgrade.GetCumulativeCost(upgradeButtonInfo.TargetLevel);
        
        UpdateUpgradeButton();

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
        //audioController.PlaySound(SfxType.PassiveSkillBuy);                       ----------------->SFX needed

        passiveScrollView.contentContainer.Remove(upgradeButtonInfo.Button);
    }

    private double GetNextLevelsCost(Upgrade upgrade)
    {
        return upgrade.GetCumulativeCost(upgrade.currentLevel + 1);
    }

    private static void UpdateButtonAvailability(UpgradeButtonInfo[] buttonInfos, double gain)
    {
        foreach (UpgradeButtonInfo upgradeButtonInfo in buttonInfos)
        {
            //if (upgradeButtonInfo?.Button == null) continue;
            upgradeButtonInfo?.Button.SetEnabled(NumberFormatter.RoundCalculatedNumber(upgradeButtonInfo.Cost) <= NumberFormatter.RoundCalculatedNumber(gain) && IsClaimed);
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

        foreach (UpgradeButtonInfo button in buttonInfos)
        {
            bool shouldShow = false;

            if (button.ResetUpgrade.Rank == currentResetStage && !button.ResetUpgrade.isPurchased)
            {
                shouldShow = true;
                button.ShowRank.AddToClassList($"rank{currentResetStage - 1}");
            }
            else
            {
                for (int j = 0; j < buttonInfos.Length; j++)
                {
                    if (buttonInfos[j].ResetUpgrade.Rank == button.ResetUpgrade.Rank + 1 && buttonInfos[j].ResetUpgrade.Name == button.ResetUpgrade.Name && button.ResetUpgrade.isPurchased)
                    {
                        shouldShow = true;
                        button.ShowRank.AddToClassList($"rank{currentResetStage}");
                        buttonInfos[j].Button.style.display = DisplayStyle.Flex;
                        buttonInfos[j].Button.SetEnabled(buttonInfos[j].ResetUpgrade.Rank <= currentResetStage && IsClaimed && !buttonInfos[j].ResetUpgrade.isPurchased);
                    }
                }
            }

            button.Button.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            button.Button.SetEnabled(button.ResetUpgrade.Rank <= currentResetStage && IsClaimed && !button.ResetUpgrade.isPurchased);
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

    public void CloseAllTabs() //for tutorial
    {
        if (upgradePanelVisible)
        {
            audioController.PlaySound(SfxType.CloseSkills);

            upgradePanelAnimation = StartCoroutine(CloseInstant(false));
            upgradePanelVisible = false;
            currentVisibleUpgrade = null;

            upgradeSection.RemoveFromClassList("clickActive");
            upgradeSection.RemoveFromClassList("idleActive");
            upgradeSection.RemoveFromClassList("resetActive");
            upgradeSection.RemoveFromClassList("passiveActive");

            return;
        }
    }
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

    private IEnumerator CloseInstant(bool show) //for tutorial
    {
        float start = show ? hiddenLeft : shownLeft;
        float end = show ? shownLeft : hiddenLeft;

        float t = 0f;
        while (t < 0)
        {
            float x = Mathf.Lerp(start, end, t / 0);
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


    private void UpdateVolumeUI(VisualElement container, int level)     //------------> unused
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

    private void SetupMuteButtons()
    {
        Button sfxButton = root.Q<Button>("sfx-button");
        Button musicButton = root.Q<Button>("music-button");

        void UpdateSfxButtonText()
        {
            bool isMuted = audioController.IsSfxMuted();
            sfxButton.text = audioController.IsSfxMuted() ? "Off" : "On";

            sfxButton.RemoveFromClassList("on-switch");
            sfxButton.RemoveFromClassList("off-switch");
            sfxButton.AddToClassList(isMuted ? "off-switch" : "on-switch");
        }

        void UpdateMusicButtonText()
        {
            bool isMuted = audioController.IsMusicMuted();
            musicButton.text = audioController.IsMusicMuted() ? "Off" : "On";

            musicButton.RemoveFromClassList("on-switch");
            musicButton.RemoveFromClassList("off-switch");
            musicButton.AddToClassList(isMuted ? "off-switch" : "on-switch");
        }

        sfxButton.clicked += () =>
        {
            audioController.PlaySound(SfxType.MenuButtons);
            audioController.ToggleSfxMute(sfxLevel);
            UpdateSfxButtonText();
        };

        musicButton.clicked += () =>
        {
            audioController.PlaySound(SfxType.MenuButtons);
            audioController.ToggleMusicMute(musicLevel);
            UpdateMusicButtonText();
        };

        UpdateSfxButtonText();
        UpdateMusicButtonText();
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

        if (upgrade.currentLevel == 0 && resetBackgroundTriggers.TryGetValue(skillName, out var resetAction))
        {
            resetAction.Invoke();
        }
    }

    private void HandleResetUpgradeBackgroundChange(ResetUpgrade resetUpgrade)
    {
        string resetSkillName = resetUpgrade.Name.ToLower();

        if (resetUpgrade.isPurchased && backgroundTriggers.TryGetValue(resetSkillName, out var action))
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

    #region IdleBar
    private void HandleIdleBar(Upgrade[] upgrades)
    {
        idleProgressBars = new ProgressBar[upgrades.Length];

        for (int i = 0; i < upgrades.Length; i++)
        {
            Upgrade upgrade = upgrades[i];
            ProgressBar progressBar = idleProgressBars[i];

            progressBar = root.Q<ProgressBar>(IdleNameFormat(upgrade.Name));

            if (upgrade.currentLevel > 0 && IdleNameFormat(upgrade.Name) == progressBar.name)
            {
                progressBar.style.display = DisplayStyle.Flex;
            }
            else
            {
                progressBar.style.display = DisplayStyle.None;
            }

            DataBinding idleProgressBinding = new DataBinding
            {
                dataSource = upgrade.IdleUpgradeDetails,
                dataSourcePath = PropertyPath.FromName(nameof(IdleUpgradeDetails.CurrentProgress)),
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.OnSourceChanged
            };

            progressBar.SetBinding(nameof(ProgressBar.value), idleProgressBinding);
        }
    }
    #endregion

    #region Options/Warning/Credits UI popups
        private void SetPopupVisible(VisualElement ve, bool visible)
        {
            if (ve == null) return;
            ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

            bool anyPopupVisible = optionsPopup?.style.display == DisplayStyle.Flex ||
                warningPopup?.style.display == DisplayStyle.Flex ||
                creditsPopup?.style.display == DisplayStyle.Flex ||
                !IsClaimed;
            
            blackBg.style.display = anyPopupVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void HideAllPopups()
        {
            SetPopupVisible(optionsPopup, false);
            SetPopupVisible(warningPopup, false);
            SetPopupVisible(creditsPopup, false);
        }

        private void SetupOptionsUI()
        {
            optionsButton       = root.Q<Button>("options");
            optionsPopup        = root.Q<VisualElement>("options-popup");
            optionsExitButton   = root.Q<Button>("exitButton");

            warningPopup        = root.Q<VisualElement>("warning-hard-reset-popup");
            warningLabel        = root.Q<Label>("warning-label");
            warningResetButton  = root.Q<Button>("warning-reset-button");

            creditsButton       = root.Q<Button>("credits-button");
            creditsPopup        = root.Q<VisualElement>("credits-popup");
            creditsBackButton   = root.Q<Button>("credits-back-button");

            hardResetBackButton = root.Q<Button>("hardReset-back-button");
            hardResetButton     = root.Q<Button>("hard-reset-button");

            HideAllPopups();
        }

        private void OptionsPopupEvents()
        {
            optionsButton.clicked += () =>
                {
                    if (!ClickAllowed("options")) { return; }

                    audioController.PlaySound(SfxType.MenuButtons);
                    HideAllPopups();
                    SetPopupVisible(optionsPopup, true);
                };

                optionsExitButton.clicked += () =>
                {
                    audioController.PlaySound(SfxType.MenuButtons);
                    SetPopupVisible(optionsPopup, false);
                };
        }

        private void CreditsPopupEvents()
        {
            creditsButton.clicked += () =>
            {
                audioController.PlaySound(SfxType.MenuButtons);
                HideAllPopups();
                SetPopupVisible(creditsPopup, true);
            };

            creditsBackButton.clicked += () =>
            {
                audioController.PlaySound(SfxType.MenuButtons);
                HideAllPopups();
                SetPopupVisible(optionsPopup, true);
            };
        }

        private void WarningPopupEvents()
        {
            warningResetButton.clicked += () =>
            {
                audioController.PlaySound(SfxType.MenuButtons);
                HideAllPopups();
                SetPopupVisible(warningPopup, true);

                if (warningLabel == null)
                {
                    Debug.LogError("'Warning label' is null.");
                }
                else
                {
                    warningLabel.enableRichText = true;
                    warningLabel.text =
                        "Are you sure you want to start over?\n" +
                        "This will <color=#FFD133>erase all your progress</color>,\n" +
                        "items, and achievements.\n" +
                        "Everything will be lost permanently.\n" +
                        "This action <color=#FFD133>cannot be undone</color>.";
                    warningLabel.style.display = DisplayStyle.Flex;
                }
            };

            hardResetButton.clicked += () =>
            {
                audioController.PlaySound(SfxType.MenuButtons);
                Debug.Log("Hard reset in progress...");
                saveHandler.ResetSave();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            };

            hardResetBackButton.clicked += () =>
            {
                audioController.PlaySound(SfxType.MenuButtons);
                HideAllPopups();
                SetPopupVisible(optionsPopup, true);
            };
        }
    #endregion

    //block the necessary clicks while the tutorial is active
    public static bool ClickAllowed(string elementName)
    {
        return !TutorialController.IsTutorialActive || TutorialController.CurrentHighlightID == elementName;
    }
}
#endregion

enum RevealStage
{
    ClickUpgrades = 1,
    IdleUpgrades = 2,
    ResetButton = 3,
    ResetUpgrades = 4,
    PassiveSkillUpgrades = 5,
}
