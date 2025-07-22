using System;
using UnityEngine;

public class AudioController : MonoBehaviour
{
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

    int buyQuantitySwapSoundsIndex = 0;


    public static AudioController Instance { get; private set; }

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

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

        audioSource.PlayOneShot(clip);
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