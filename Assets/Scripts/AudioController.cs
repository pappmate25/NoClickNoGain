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
    private Upgrade beastMode;

    [SerializeField]
    private GameController gameController;
    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Awake()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        sfxSource = audioSources[0];
        musicSource = audioSources[1];

        PlayMusic(gameController.IsBeastModeBought());
    }

    public bool ToggleMute()
    {
        sfxSource.mute = !sfxSource.mute;
        musicSource.mute = !musicSource.mute;

        Debug.Log($"SFX Mute: {sfxSource.mute}, Music Mute: {musicSource.mute}");

        return sfxSource.mute;
    }

    public void OnUpgradeBought(IGameEventDetails details)
    {
        if (details is UpgradeBought upgradeDetails)
        {
            if (upgradeDetails.Upgrade == beastMode)
            {
                PlayMusic(true);
            }
        }
    }

    private void PlayMusic(bool beastMode)
    {
        if (beastMode)
        {
            musicSource.clip = beastModeMusic;
        }
        else
        {
            musicSource.clip = regular;
        }
        musicSource.Play();
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
