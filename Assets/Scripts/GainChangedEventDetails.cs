public struct GainChangedEventDetails : IGameEventDetails
{
    public double NewGain;
    public GainChangeType ChangeType;
}

public enum GainChangeType
{
    Click, 
    Idle,
    UpgradeBought,
    Reset,
    SaveLoadFromClipboard,
    WelcomeBackClaimed
}
