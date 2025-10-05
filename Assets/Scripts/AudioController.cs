using System;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [Header("SFX Clips")]
    [SerializeField]
    private AudioClip[] buyQuantitySwapSounds;
    [SerializeField]
    private AudioClip closeSkills;
    [SerializeField]
    private AudioClip openSkills;
    [SerializeField]
    private AudioClip resetPassiveSkillBuy;
    [SerializeField]
    private AudioClip resetShower;
    [SerializeField]
    private AudioClip swapSkillTabWhileOpen;
    [SerializeField]
    private AudioClip upgradeSkills;
    [SerializeField]
    private AudioClip welcomeBackClaimed;
    [SerializeField]
    private AudioClip menuButtons;
    [SerializeField]
    private AudioClip tutorialDoneNext;
    [SerializeField]
    private AudioClip tutorialTyping;

    [Header("Music Clips")]
    [SerializeField]
    private AudioClip beastModeMusic;
    [SerializeField]
    private AudioClip regular;


    [Header("References")]
    [SerializeField]
    private Upgrade beastModeUpgrade;

    [SerializeField]
    private GameController gameController;
    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioSource typingSource;
    private bool isTyping;
    private float tutorialDoneNextVolume = 10f;

    public bool IsSFXSourceMuted { get; set; }
    public bool IsMusicSourceMuted { get; set; }

    void Awake()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        sfxSource = audioSources[0];
        musicSource = audioSources[1];
        typingSource = audioSources[2];
    }

    public void OnEnable()
    {
        IsMusicSourceMuted = ConfigurationHandler.Configuration.MusicMuted;
        IsSFXSourceMuted = ConfigurationHandler.Configuration.SfxMuted;

        SetMuteOnLoad();

        SetMusicVolume(musicVolume);
        SetSfxVolume(sfxVolume);

        PlayMusic(gameController.IsBeastModeBought());
    }

    public bool IsMusicMuted() => IsMusicSourceMuted;
    public bool IsSfxMuted() => IsSFXSourceMuted;

    public void ToggleMusicMute(int musicLevel)
    {
        bool isMuted = musicSource.mute;
        musicSource.mute = !isMuted;
        IsMusicSourceMuted = !isMuted;

        ConfigurationHandler.Configuration.MusicMuted = IsMusicSourceMuted;
        ConfigurationHandler.Save();

        SetMusicVolume(isMuted ? musicLevel / 6f : 0f);
    }

    public void ToggleSfxMute(int sfxLevel)
    {
        bool isMuted = sfxSource.mute;
        sfxSource.mute = !isMuted;
        typingSource.mute = !isMuted;
        IsSFXSourceMuted = !isMuted;

        ConfigurationHandler.Configuration.SfxMuted = IsSFXSourceMuted;
        ConfigurationHandler.Save();

        SetSfxVolume(isMuted ? sfxLevel / 6f : 0f);
    }
    public void MuteMusicTemporarily(bool mute)
    {
        musicSource.mute = mute;
    }

    public void SetMuteOnLoad()
    {
        sfxSource.mute = IsSFXSourceMuted;
        musicSource.mute = IsMusicSourceMuted;
        typingSource.mute = IsSFXSourceMuted;
    }

    public void OnSaveLoadedFromClipboard()
    {
        PlayMusic(gameController.IsBeastModeBought());
    }
    
    public void OnReset()
    {
        PlayMusic(false);
    }

    public void OnUpgradeBought(IGameEventDetails details)
    {
        if (details is UpgradeBought upgradeDetails)
        {
            if (upgradeDetails.Upgrade == beastModeUpgrade)
            {
                PlayMusic(true);
            }
        }
    }

    bool isBeastModeActive = false;
    public void PlayMusic(bool beastMode)
    {
        if (isBeastModeActive == beastMode && musicSource.isPlaying)
            return;

        if (beastMode)
        {
            musicSource.clip = beastModeMusic;
        }
        else
        {
            musicSource.clip = regular;
        }
        musicSource.Play();

        isBeastModeActive = beastMode;
    }

    int buyQuantitySwapSoundsIndex = 0;
    public void PlaySound(SfxType sfxType)
    {
        //Debug.Log("Playing sound: " + Enum.GetName(typeof(SfxType), sfxType));

        AudioClip clip = sfxType switch
        {
            SfxType.BuyQuantitySwap => buyQuantitySwapSounds[buyQuantitySwapSoundsIndex++],
            SfxType.CloseSkills => closeSkills,
            SfxType.OpenSkills => openSkills,
            SfxType.ResetPassiveSkillBuy => resetPassiveSkillBuy,
            SfxType.ResetShower => resetShower,
            SfxType.SwapSkillTabWhileOpen => swapSkillTabWhileOpen,
            SfxType.UpgradeSkills => upgradeSkills,
            SfxType.WelcomeBackClaimed => welcomeBackClaimed,
            SfxType.MenuButtons => menuButtons,
            SfxType.TutorialDoneNext => tutorialDoneNext,
            _ => throw new ArgumentOutOfRangeException(nameof(sfxType), $"No sound defined for {sfxType}"),
        };

        if (sfxType == SfxType.BuyQuantitySwap)
            buyQuantitySwapSoundsIndex %= buyQuantitySwapSounds.Length;

        float volume = sfxType == SfxType.TutorialDoneNext ? tutorialDoneNextVolume : 1;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void StartTyping()
    {
        if(tutorialTyping == null || isTyping)
        {
            return;
        }

        typingSource.clip = tutorialTyping;
        typingSource.loop = true;
        typingSource.Play();
        isTyping = true;
    }

    public void StopTyping()
    {
        if (!isTyping)
        {
            return;
        }

        typingSource.Stop();
        isTyping = false;
    }

    // Volume control sliders
    private float musicVolume = 0.5f;
    private float sfxVolume = 1f;

    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    public void SetMusicVolume(float newVolume)
    {
        musicVolume = Mathf.Clamp01(newVolume);
        musicSource.volume = musicVolume;

        //if (musicSource.mute && musicVolume > 0f)
        //    musicSource.mute = false;
    }

    public void SetSfxVolume(float newVolume)
    {
        sfxVolume = Mathf.Clamp01(newVolume);
        sfxSource.volume = sfxVolume;

        //if (sfxSource.mute && sfxVolume > 0f)
        //    sfxSource.mute = false;
    }
    
}

public enum SfxType
{
    BuyQuantitySwap,
    CloseSkills,
    OpenSkills,
    ResetPassiveSkillBuy,
    ResetShower,
    SwapSkillTabWhileOpen,
    UpgradeSkills,
    WelcomeBackClaimed,
    MenuButtons,
    TutorialDoneNext,
}
