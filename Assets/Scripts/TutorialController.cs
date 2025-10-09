using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialController : MonoBehaviour
{
    private enum TutorialElementTypes
    {
        Default,
        NoGuy,
        ShiftUp
    }
    private enum ClickMode 
    { 
        None, 
        Next, 
        Done 
    }

    private struct HighlightAction
    {
        public string ElementName;
        public bool IsForceClick;
        public ClickMode ClickMode;
        public int BackgroundIndex;
        public bool ResetQuantityButton;
        public bool CloseAllTabs;
    }

    private enum Step
    {
        Welcome,
        RightTechnique,
        BuyQuantity,
        CloseToIdle,
        TrainingClothes,
        AfterTrainingClothes,
        CloseToReset,
        Reset,
        BuyResetSkills,
        Done
    }

    private struct StepInfo
    {
        public string[] GuideDescriptions;        
        public Func<bool> RequirementForNextStep;
        public Dictionary<int, HighlightAction> Highlights;
        public TutorialElementTypes TutoElementTypes;
    }

    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private LargeNumber totalGain;
    [SerializeField] private LargeNumber gain;
    [SerializeField] private UpgradeList clickUpgrades;
    [SerializeField] private UpgradeList idleUpgrades;
    [SerializeField] private ResetUpgradeList resetUpgrades;
    [SerializeField] private UIController controller;
    [SerializeField] private AudioController audioController;

    public static bool IsTutorialActive { get; private set; }
    public static string CurrentHighlightID { get; private set; }

    //words by characters 'anim'
    private float charsPerSecond = 40f;
    private Coroutine typingJob;
    private bool isTyping;

    private Step step;
    private int currentPage;
    private bool isTutorialOpen;
    private Dictionary<Step, StepInfo> steps;


    private VisualElement root;
    private VisualElement tutorialRoot;
    private VisualElement tutorialMask;
    private Label guideText;
    private Button nextButton;
    private Button doneButton;
    private EventCallback<ClickEvent> skipTypingClickCB;

    //for forceclick target highlight
    private VisualElement highlightTarget;
    private EventCallback<ClickEvent> highlightCB;
    private Texture2D[] backgrounds;
    private int maxBackgrounds = 7;

    [SerializeField]
    private BoolVariable isTutorialFinished;

    //save steps progress
    private int completedMask;

    public static void ResetTutorialSteps()
    {
        ConfigurationHandler.Configuration.TutorialStep = 0;
        ConfigurationHandler.Configuration.TutorialMask = 0;
    }

    private void Start()
    {
        step = (Step)ConfigurationHandler.Configuration.TutorialStep;
        completedMask = ConfigurationHandler.Configuration.TutorialMask;
        BuildStepTable();

        if (ConfigurationHandler.Configuration.AnalyticsAck)
        {
            StartTutorial();
        }
    }

    private void Update()
    {
        if (!isTutorialFinished.Value && !isTutorialOpen)
        {
            if (steps.TryGetValue(step, out StepInfo stepInfo) && stepInfo.RequirementForNextStep != null && stepInfo.RequirementForNextStep())
            {
                Advance();
            }
        }
    }

    public void StartTutorialAfterAnalytics()
    {
        if (ConfigurationHandler.Configuration.AnalyticsAck)
        {
            StartTutorial();
        }
    }
    private void StartTutorial()
    {
        backgrounds = new Texture2D[maxBackgrounds];
        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i] = Resources.Load<Texture2D>($"UI/Tutorial/Backgrounds/background {i}");
        }
        if (!isTutorialFinished.Value)
        {
            root = uiDocument.rootVisualElement;
            tutorialRoot = root.Q<VisualElement>("tutorial");
            tutorialMask = root.Q<VisualElement>("tutorial-mask");

            guideText = root.Q<Label>("description");

            nextButton = root.Q<Button>("next-button");
            doneButton = root.Q<Button>("done-button");

            nextButton.clicked += OnNextClick;
            doneButton.clicked += OnDoneClick;

            skipTypingClickCB = evt => {
                if (evt.target == nextButton || evt.target == doneButton)
                    return;

                if (isTyping)
                {
                    SkipTyping();
                    evt.StopPropagation();
                }
            };
            ShowStepUI();
        }
    }

    private bool IsOverlayDismissed(Step step)
    {
        int bit = 1 << (int)step;
        return (completedMask & bit) != 0;
    }
    private void MarkOverlayDismissed(Step step)
    {
        int bit = 1 << (int)step;
        completedMask |= bit;
    }

    private void SaveMask()
    {
        ConfigurationHandler.Configuration.TutorialMask = completedMask;
    }


    private void OnNextClick()
    {
        audioController.PlaySound(SfxType.TutorialDoneNext);
        currentPage++;
        RefreshPage();
    }
    private void OnDoneClick()
    {
        audioController.PlaySound(SfxType.TutorialDoneNext);
        audioController.StopTyping(); //in case if the highlighted element is pressed before the typing anim ended
        MarkOverlayDismissed(step);
        SaveMask();
        ToggleOverlay(false);
        ClearHighlight();
    }

    private void Advance()
    {
        if (step < Step.Done)
        {
            completedMask &= ~(1 << (int)step);
            step++;
            SaveMask();
        }

        if(step == Step.Done)
        {
            isTutorialFinished.Value = true;
            ResetTutorialSteps();
        }

        ConfigurationHandler.Configuration.TutorialStep = (int)step;
        ConfigurationHandler.Save();

        currentPage = 0;
        ShowStepUI();
    }

    public void ShowStepUI()
    {
        if(IsOverlayDismissed(step))
        {
            isTutorialOpen = false;
            return;
        }

        bool hasPages = steps.TryGetValue(step, out StepInfo stepInfo) && stepInfo.GuideDescriptions != null && stepInfo.GuideDescriptions.Length > 0;

        if (!hasPages)
        {
            isTutorialOpen = false; 
            return; 
        }

        tutorialRoot.RegisterCallback(skipTypingClickCB);

        ApplySkin(stepInfo.TutoElementTypes);
        ToggleOverlay(true);
        RefreshPage();
    }

    private void RefreshPage()
    {
        ClearHighlight();

        string[] pages = steps[step].GuideDescriptions;
        string sentence = pages[currentPage];
        StartTyping(sentence);


        if (steps.TryGetValue(step, out StepInfo stepInfo) &&
            stepInfo.Highlights != null &&
            stepInfo.Highlights.TryGetValue(currentPage, out HighlightAction highlightAction))
        {
            ApplyHighlight(highlightAction);
        }

        UpdateNavButtons();
    }

    private void UpdateNavButtons()
    {
        if (isTyping || highlightTarget != null) 
        {
            nextButton.style.display = DisplayStyle.None;
            doneButton.style.display = DisplayStyle.None;
            return;
        }

        bool last = currentPage == steps[step].GuideDescriptions.Length - 1;
        nextButton.style.display = last ? DisplayStyle.None : DisplayStyle.Flex;
        doneButton.style.display = last ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ToggleOverlay(bool show)
    {
        isTutorialOpen = show;
        IsTutorialActive = show;
        tutorialRoot.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

        if (!show && skipTypingClickCB != null)
            tutorialRoot.UnregisterCallback(skipTypingClickCB);
    }

    private void ApplySkin(TutorialElementTypes skin)
    {
        tutorialRoot.RemoveFromClassList("tutorialDefault");
        tutorialRoot.RemoveFromClassList("tutorialNoGuy");
        tutorialRoot.RemoveFromClassList("tutorialTransition");

        guideText.RemoveFromClassList("tutorialDefaultLabel");
        nextButton.RemoveFromClassList("tutorialDefaultButtons");
        doneButton.RemoveFromClassList("tutorialDefaultButtons");

        guideText.RemoveFromClassList("tutorialNoGuyLabel");
        nextButton.RemoveFromClassList("tutorialNoGuyButtons");
        doneButton.RemoveFromClassList("tutorialNoGuyButtons");

        switch (skin)
        {
            case TutorialElementTypes.NoGuy:
                tutorialRoot.AddToClassList("tutorialNoGuy");
                guideText.AddToClassList("tutorialNoGuyLabel");
                nextButton.AddToClassList("tutorialNoGuyButtons");
                doneButton.AddToClassList("tutorialNoGuyButtons");
                break;

            case TutorialElementTypes.ShiftUp:
                tutorialRoot.AddToClassList("tutorialNoGuy");
                tutorialRoot.AddToClassList("tutorialTransition");
                guideText.AddToClassList("tutorialNoGuyLabel");
                nextButton.AddToClassList("tutorialNoGuyButtons");
                doneButton.AddToClassList("tutorialNoGuyButtons");
                break;

            default:
                tutorialRoot.AddToClassList("tutorialDefault");
                guideText.AddToClassList("tutorialDefaultLabel");
                nextButton.AddToClassList("tutorialDefaultButtons");
                doneButton.AddToClassList("tutorialDefaultButtons");
                break;
        }
    }


    private void ApplyHighlight(in HighlightAction highlightAction)
    {
        tutorialRoot.pickingMode = PickingMode.Ignore;

        highlightTarget = root.Q<VisualElement>(highlightAction.ElementName);
        if (highlightTarget == null)
        {
            return;
        }

        CurrentHighlightID = highlightAction.ElementName;

        //set BuyQuantity button to 1x
        if (highlightAction.ResetQuantityButton && controller != null)
        {
            controller.SelectBuyQuantity(0);
        }

        //Close skill tabs
        if(highlightAction.CloseAllTabs && controller != null)
        {
            controller.CloseAllTabs();
        }

        if(highlightAction.BackgroundIndex >= 0 && highlightAction.BackgroundIndex <= backgrounds.Length)
        {
            tutorialMask.style.display = DisplayStyle.Flex;
            tutorialMask.pickingMode = PickingMode.Ignore;
            tutorialMask.style.backgroundImage = new StyleBackground(backgrounds[highlightAction.BackgroundIndex]);
        }
        else
        {
            tutorialMask.style.display = DisplayStyle.None;
        }

        if (highlightAction.IsForceClick)
        {
            highlightTarget.pickingMode = PickingMode.Position;

            Action action = highlightAction.ClickMode switch
            {
                ClickMode.Next => OnNextClick,
                ClickMode.Done => OnDoneClick,
                _ => (Action)null
            };

            if (action != null)
            {
                highlightCB = _ => action.Invoke();
                highlightTarget.RegisterCallback(highlightCB);
            }
        }

        UpdateNavButtons();
    }

    private void ClearHighlight()
    {
        tutorialRoot.pickingMode = PickingMode.Position;

        if (highlightTarget == null)
        {
            return;
        }


        if (highlightCB != null)
        {
            highlightTarget.UnregisterCallback(highlightCB);
        }

        highlightTarget = null;
        CurrentHighlightID = null;
        highlightCB = null;

        tutorialMask.style.display = DisplayStyle.None;

        UpdateNavButtons();
    }

    private void StartTyping(string sentence)
    {
        if (typingJob != null)
        {
            StopCoroutine(typingJob);
        }

        UpdateNavButtons();          
        typingJob = StartCoroutine(TypeRoutine(sentence));        
    }

    private void SkipTyping()
    {
        if (!isTyping)
            return;

        if (typingJob != null)
        {
            StopCoroutine(typingJob);
            typingJob = null;
        }

        guideText.text = steps[step].GuideDescriptions[currentPage];

        audioController.StopTyping();

        isTyping = false;
        UpdateNavButtons();
    }

    private IEnumerator TypeRoutine(string sentence)
    {
        isTyping = true;
        guideText.text = string.Empty;

        audioController.StartTyping();

        float delay = 1f / charsPerSecond;
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < sentence.Length;)
        {
            if (sentence[i] == '<') 
            {
                int sentenceEnd = sentence.IndexOf('>', i);

                builder.Append(sentence, i, sentenceEnd - i + 1);
                i = sentenceEnd + 1;
            }
            else
            {
                builder.Append(sentence[i++]);
                guideText.text = builder.ToString();
                yield return new WaitForSecondsRealtime(delay);
            }
        }

        guideText.text = builder.ToString();
        audioController.StopTyping();
        isTyping = false;
        UpdateNavButtons();
    }
    private bool IsAllResetUnlocked(ResetUpgrade[] resetUpgrades)
    {
        int unlockedCounter = 0;

        foreach (ResetUpgrade upgrade in resetUpgrades)
        {
            if (upgrade.isPurchased)
            {
                unlockedCounter++;
            }
        }

        return unlockedCounter == 10;
    }

    private void BuildStepTable()
    {
        bool GetTenGain() => totalGain.Value >= 10;
        bool BoughtRightTech() => clickUpgrades.Upgrades[0].currentLevel > 0;
        bool HaveTenUpgrades() => clickUpgrades.Upgrades.Sum(upgrade => upgrade.currentLevel) >= 10;
        bool HaveEnougGain() => gain.Value >= 800;
        bool BoughtTrainingClothes() => idleUpgrades.Upgrades[0].currentLevel > 0;
        bool CloseToReset() => totalGain.Value >= 27000000;
        bool ReadyForReset() => totalGain.Value >= 30000000;
        bool PerformedReset() => GameController.Instance.GetResetStage() > 0;
        bool TutorialDone() => IsAllResetUnlocked(resetUpgrades.ResetUpgrades);


        steps = new()
        {
            [Step.Welcome] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "Welcome, I'm <color=#FFD133>Guy</color> and I will guide you on your journey.",
                    "Before hitting the Gym, let's get you through the basics.",
                    "Start pumping those muscles! <color=#FFD133>Tap</color> anywhere to earn <color=#FFD133>GAIN</color>."
                },
                RequirementForNextStep = GetTenGain,
                TutoElementTypes = TutorialElementTypes.Default
            },

            [Step.RightTechnique] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "Let's <color=#FFD133>speed</color> things up!",
                    "Open the <color=#FFD133>click skills tab</color>.",
                    "Here you can <color=#FFD133>buy skills</color> that increase your <color=#FFD133>gain/click</color>",
                    "<color=#FFD133>Buy</color> your first upgrade: <color=#FFD133>Right Technique</color>."
                },
                RequirementForNextStep = BoughtRightTech,
                Highlights = new Dictionary<int, HighlightAction>
                {
                    [1] = new HighlightAction
                    {
                        ElementName = "click-btn",
                        IsForceClick = true,
                        ClickMode = ClickMode.Next,
                        BackgroundIndex = 0,
                        ResetQuantityButton = true,
                        CloseAllTabs = true
                    },
                    [3] = new HighlightAction
                    {
                        ElementName = "right-technique",
                        IsForceClick = true,
                        ClickMode = ClickMode.Done,
                        BackgroundIndex = 1,
                        ResetQuantityButton = true,
                        CloseAllTabs = false
                    }
                },
                TutoElementTypes = TutorialElementTypes.Default
            },

            [Step.BuyQuantity] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "You can buy multiple upgrades at once with the <color=#FFD133>1x/x5/x10/x100.. button</color>."
                },
                RequirementForNextStep = HaveTenUpgrades,
                //Highlights = new Dictionary<int, HighlightAction>
                //{
                //    [0] = new HighlightAction
                //    {
                //        ElementName = "buy-quantity-toggle-button",
                //        IsForceClick = true,          
                //        ClickMode = ClickMode.Done,
                //        BackgroundIndex = 2,
                //        ResetQuantityButton = false,
                //        CloseAllTabs = false
                //    }
                //},
                TutoElementTypes = TutorialElementTypes.ShiftUp
            },

            [Step.CloseToIdle] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "Great progress!",
                    "The <color=#FFD133>idle skills</color> are now available, click on the <color=#FFD133>idle skills tab</color>.",
                    "Keep earning <color=#FFD133>GAIN</color> until you can unlock your <color=#FFD133>first idle skill</color>."
                },
                RequirementForNextStep = HaveEnougGain,
                Highlights = new Dictionary<int, HighlightAction>
                {
                    [1] = new HighlightAction
                    {
                        ElementName = "idle-btn",
                        IsForceClick = true,
                        ClickMode = ClickMode.Next,
                        BackgroundIndex = 3,
                        ResetQuantityButton = true,
                        CloseAllTabs = true
                    },
                },
                TutoElementTypes = TutorialElementTypes.Default
            },

            [Step.TrainingClothes] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "<color=#FFD133>Nice job!</color> You earned 800 gain.",
                    "Go to the <color=#FFD133>idle skills tab</color>.",
                    "Here you can buy skills that <color=#FFD133>increase</color> your <color=#FFD133>idle gain income</color>.",
                    "<color=#FFD133>Buy</color> your first idle skill: <color=#FFD133>Training Clothes</color>!",
                },
                RequirementForNextStep = BoughtTrainingClothes,
                Highlights = new Dictionary<int, HighlightAction>
                {
                    [1] = new HighlightAction
                    {
                        ElementName = "idle-btn",
                        IsForceClick = true,
                        ClickMode = ClickMode.Next,
                        BackgroundIndex = 3,
                        ResetQuantityButton = true,
                        CloseAllTabs = true
                    },

                    [3] = new HighlightAction
                    {
                        ElementName = "training-clothes",
                        IsForceClick = true,
                        ClickMode = ClickMode.Done,
                        BackgroundIndex = 1,
                        ResetQuantityButton = true,
                        CloseAllTabs = false
                    }
                },
                TutoElementTypes = TutorialElementTypes.Default
            },

            [Step.AfterTrainingClothes] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "Now you know the basics and you can start to <color=#FFD133>grind on your own</color>!!",
                    "The next goal is to <color=#FFD133>reach</color> your <color=#FFD133>first reset</color>! I will see you soon!"
                },
                RequirementForNextStep = CloseToReset,
                TutoElementTypes = TutorialElementTypes.Default
            },

            [Step.CloseToReset] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "You're almost ready for your first <color=#FFD133>reset</color>. Collect <color=#FFD133>3 more million GAIN!</color>"
                },
                RequirementForNextStep = ReadyForReset,
                Highlights = new Dictionary<int, HighlightAction>
                {
                    [0] = new HighlightAction
                    {
                        ElementName = "reset-progress-button-alma",
                        IsForceClick = false,
                        ClickMode = ClickMode.Done,
                        BackgroundIndex = 5,
                        ResetQuantityButton = false,
                        CloseAllTabs = false
                    }
                },
                TutoElementTypes = TutorialElementTypes.ShiftUp
            },

            [Step.Reset] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "Smashing it!",
                    "If you feel ready, hit the <color=#FFD133>Reset button</color> to restart stronger than ever."
                },
                RequirementForNextStep = PerformedReset,               
                TutoElementTypes = TutorialElementTypes.Default
            },

            [Step.BuyResetSkills] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "<color=#FFD133>A fresh start!</color> New challenges and new skills!",
                    "With the <color=#FFD133>reset skills</color> you can <color=#FFD133>increase the income</color> of your <color=#FFD133>click</color> and <color=#FFD133>idle</color> skills!",
                    "Go the the <color=#FFD133>reset skills tab</color>.",
                    "Buy <color=#FFD133>all of the new skills!</color>",
                },
                RequirementForNextStep = TutorialDone,
                Highlights = new Dictionary<int, HighlightAction>
                {
                    [2] = new HighlightAction
                    {
                        ElementName = "reset-btn",
                        IsForceClick = true,
                        ClickMode = ClickMode.Next,
                        BackgroundIndex = 6,
                        ResetQuantityButton = false,
                        CloseAllTabs = true
                    }
                },
                TutoElementTypes = TutorialElementTypes.NoGuy
            }
        };
    }
}
