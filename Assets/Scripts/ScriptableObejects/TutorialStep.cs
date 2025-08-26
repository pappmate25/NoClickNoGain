using UnityEngine;

public enum TutorialStepType { Info, ClickTarget }

[CreateAssetMenu(menuName = "SO/Tutorial/Step")]
public class TutorialStep : ScriptableObject
{
    public string StepName;
    public TutorialStepType Type;
    public string TargetElementName;
    [TextArea] public string StepDiscription;
    public int StepIndex;
    public bool ForceClick = true;
    public bool IsStepDone = false;
}
