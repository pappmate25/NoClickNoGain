using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Mono.Cecil;
using NUnit.Framework;
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
    }

    private enum Step
    {
        Welcome,            
        RightTechnique,     
        BuyQuantity,        
        CloseToIdle,        
        TrainingClothes,    
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

    //words by characters 'anim'
    [SerializeField] private float charsPerSecond = 20f;
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

    //for forceclick target highlight
    private VisualElement highlightTarget;
    private EventCallback<ClickEvent> highlightCB;
    private Texture2D[] backgrounds;
    private int maxBackgrounds = 7;


    //save steps progress
    private int completedMask;
    private const string prefKey = "Tutorial.Step";
    private const string prefKeyMask = "Tutorial.Mask";

    private void Awake()
    {
        step = (Step)PlayerPrefs.GetInt(prefKey, 0);
        completedMask = PlayerPrefs.GetInt(prefKeyMask, 0);
        BuildStepTable();
    }

    private void Start()
    {
        //for highlights
        backgrounds = new Texture2D[maxBackgrounds];
        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i] = Resources.Load<Texture2D>($"UI/Tutorial/backgrounds {i}");
        }
        if (GameController.Instance.IsFirstGameStart())
        {
            root = uiDocument.rootVisualElement;
            tutorialRoot = root.Q<VisualElement>("tutorial");
            tutorialMask = root.Q<VisualElement>("tutorial-mask");

            guideText = root.Q<Label>("description");

            nextButton = root.Q<Button>("next-button");
            doneButton = root.Q<Button>("done-button");

            nextButton.clicked += OnNextClick;
            doneButton.clicked += OnDoneClick;

            ShowStepUI();
        }
    }
    private void Update()
    {
        if (GameController.Instance.IsFirstGameStart())
        {
            if (isTutorialOpen)
            {
                return;
            }

            if (steps.TryGetValue(step, out StepInfo stepInfo) && stepInfo.RequirementForNextStep != null && stepInfo.RequirementForNextStep())
            {
                Advance();
            }
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
        PlayerPrefs.SetInt(prefKeyMask, completedMask);
    }


    private void OnNextClick()
    {
        currentPage++;
        RefreshPage();
    }
    private void OnDoneClick()
    {
        MarkOverlayDismissed(step);
        SaveMask();
        ToggleOverlay(false);
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
            GameController.Instance.SetFirstGameStart(false);
            PlayerPrefs.DeleteKey("Tutorial.Step");
            PlayerPrefs.DeleteKey("Tutorial.Mask");
        }

        PlayerPrefs.SetInt(prefKey, (int)step);
        PlayerPrefs.Save();

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
        tutorialRoot.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
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

            nextButton.style.display = DisplayStyle.None;
            doneButton.style.display = DisplayStyle.None;
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

    private IEnumerator TypeRoutine(string sentence)
    {
        isTyping = true;
        guideText.text = string.Empty;

        float delay = 1f / charsPerSecond;

        foreach (char c in sentence)
        {
            guideText.text += c;
            yield return new WaitForSecondsRealtime(delay);
        }

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
                    "Welcome, I'm XY and I will guide you on your journey.",
                    "Before hitting the Gym, let's get you through the basics.",
                    "Start pumping those muscles! Tap anywhere to earn GAIN."
                },
                RequirementForNextStep = GetTenGain,
                TutoElementTypes = TutorialElementTypes.Default
            },

            [Step.RightTechnique] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "Let's speed things up!",
                    "Open the 'click skills tab'",
                    "Buy your first upgrade: 'Right Technique'."
                },
                RequirementForNextStep = BoughtRightTech,
                Highlights = new Dictionary<int, HighlightAction>
                {
                    [1] = new HighlightAction
                    {
                        ElementName = "upgrade-section",
                        IsForceClick = true,
                        ClickMode = ClickMode.Next,
                        BackgroundIndex = 0
                    },
                    [2] = new HighlightAction
                    {
                        ElementName = "right-technique",
                        IsForceClick = true,
                        ClickMode = ClickMode.Done,
                        BackgroundIndex = 1
                    }
                },
                TutoElementTypes = TutorialElementTypes.Default
            },

            [Step.BuyQuantity] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "You can buy multiple upgrades at once with the 'x5/x10/x100 button'."
                },
                RequirementForNextStep = HaveTenUpgrades,
                Highlights = new Dictionary<int, HighlightAction>
                {
                    [0] = new HighlightAction
                    {
                        ElementName = "buy-quantity-toggle-button",
                        IsForceClick = true,          
                        ClickMode = ClickMode.Done,
                        BackgroundIndex = 2
                    }
                },
                TutoElementTypes = TutorialElementTypes.ShiftUp
            },

            [Step.CloseToIdle] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "Great progress!",
                    "The idle skilss are now avaiable, click on the 'idle skills tab'",
                    "Keep earning GAIN until you can unlock your first idle skill."
                },
                RequirementForNextStep = HaveEnougGain,
                Highlights = new Dictionary<int, HighlightAction>
                {
                    [1] = new HighlightAction
                    {
                        ElementName = "idle-btn",
                        IsForceClick = true,
                        ClickMode = ClickMode.Next,
                        BackgroundIndex = 3
                    },
                },
                TutoElementTypes = TutorialElementTypes.Default
            },

            [Step.TrainingClothes] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "Nice job! You earned 800 gain.",
                    "Go to the 'idle skills tab' and buy your first idle skill: 'Training Clothes'!"
                },
                RequirementForNextStep = CloseToReset,
                Highlights = new Dictionary<int, HighlightAction>
                {
                    [1] = new HighlightAction
                    {
                        ElementName = "training-clothes",
                        IsForceClick = true,
                        ClickMode = ClickMode.Done,
                        BackgroundIndex = 4
                    }
                },
                TutoElementTypes = TutorialElementTypes.Default
            },

            [Step.CloseToReset] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "You're almost ready for your first reset. Push to 30 M Total GAIN!"
                },
                RequirementForNextStep = ReadyForReset,
                TutoElementTypes = TutorialElementTypes.ShiftUp
            },

            [Step.Reset] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "Smashing it!",
                    "If you feel ready, hit the Reset button to restart stronger than ever."
                },
                RequirementForNextStep = PerformedReset,
                Highlights = new Dictionary<int, HighlightAction>
                {
                    [1] = new HighlightAction
                    {
                        ElementName = "reset-button",
                        IsForceClick = false,
                        ClickMode = ClickMode.Done,
                        BackgroundIndex = 5
                    }
                },
                TutoElementTypes = TutorialElementTypes.Default
            },

            [Step.BuyResetSkills] = new StepInfo
            {
                GuideDescriptions = new[]
                {
                    "A fresh start! New challenges and new skills!",
                    "With the reset skills you can increase the income of your 'click' and 'idle' skills!",
                    "Go the the 'reset skills tab'",
                    "Buy all of the new skills",
                },
                RequirementForNextStep = TutorialDone,
                Highlights = new Dictionary<int, HighlightAction>
                {
                    [2] = new HighlightAction
                    {
                        ElementName = "reset-btn",
                        IsForceClick = true,
                        ClickMode = ClickMode.Next,
                        BackgroundIndex = 6
                    }
                },
                TutoElementTypes = TutorialElementTypes.NoGuy
            }
        };
    }
}
