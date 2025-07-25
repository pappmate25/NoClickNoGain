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

    void Awake()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        sfxSource = audioSources[0];
        musicSource = audioSources[1];

        SetMusicVolume(musicVolume);
        SetSfxVolume(sfxVolume);

        PlayMusic(gameController.IsBeastModeBought());
    }

    public bool IsMuted() => sfxSource.mute;

    public bool ToggleMute(int musicLevel, int sfxLevel)
    {
        bool isMuted = IsMuted();

        if (!isMuted)
        {
            SetMusicVolume(0f);
            SetSfxVolume(0f);
            musicSource.mute = true;
            sfxSource.mute = true;
        }
        else
        {
            musicSource.mute = false;
            sfxSource.mute = false;
            SetMusicVolume(musicLevel / 6f);
            SetSfxVolume(sfxLevel / 6f);
        }

        return IsMuted();
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
    private void PlayMusic(bool beastMode)
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
        Debug.Log("Playing sound: " + Enum.GetName(typeof(SfxType), sfxType));

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
            _ => throw new ArgumentOutOfRangeException(nameof(sfxType), $"No sound defined for {sfxType}"),
        };

        if (sfxType == SfxType.BuyQuantitySwap)
            buyQuantitySwapSoundsIndex %= buyQuantitySwapSounds.Length;

        sfxSource.PlayOneShot(clip);
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

        if (musicSource.mute && musicVolume > 0f)
            musicSource.mute = false;
    }

    public void SetSfxVolume(float newVolume)
    {
        sfxVolume = Mathf.Clamp01(newVolume);
        sfxSource.volume = sfxVolume;

        if (sfxSource.mute && sfxVolume > 0f)
            sfxSource.mute = false;
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
    WelcomeBackClaimed
}
