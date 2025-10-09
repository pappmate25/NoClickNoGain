using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public readonly long[] RequiredTotalGain = { 30000000, 20000000000, 235000000000000 };
    
    [SerializeField]
    private GameState gameState;

    [SerializeField]
    private FloatVariable animationSpeed;
    [SerializeField]
    private LargeNumber resetCoin;
    [SerializeField]
    private ResetUpgradeList resetUpgradesList;

    [SerializeField]
    private Upgrade beastModeUpgrade;

    [SerializeField]
    private LargeNumber idleGain;


    [SerializeField]
    private LargeNumber resetStage;

    [SerializeField]
    private GameEvent clickEvent;
    [SerializeField]
    private GameEvent upgradeBoughtEvent;

    [SerializeField]
    private BoolVariable isTutorialFinished;
    
    [SerializeField]
    private IdleGainPopup idleGainPopup;

    public static GameController Instance { get; private set; }

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameState.Initialize();

        //DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animationSpeed.Value = 10.0f;
    }

    // Update is called once per frame
    void Update()
    {
        gameState.Update();
    }

    public void OnGainChanged(IGameEventDetails details)
    {
        if (details is GainChangedEventDetails gainChangedDetails) 
        {
            if (gainChangedDetails.ChangeType == GainChangeType.Idle)
            {
                idleGainPopup.ShowGainValue(gainChangedDetails.IdleIndex, gainChangedDetails.ChangeAmount);
            }
        }
        else
        {
            throw new ArgumentException("OnGainChanged() received unsupported IGameEventDetails type.");
        }
    }

    public bool IsBeastModeBought()
        => beastModeUpgrade.currentLevel > 0;

    // Starts with small letter because otherwise it will be automatically be called by the InputSystem
    public void onClick()
    {
        gameState.Click();
    }

    public void OnResetUpdradeBought(IGameEventDetails details)
    {
        if (details is not ResetUpgradeBought resetUpgradeBought || resetUpgradeBought.ResetUpgrade == null)
        {
            Debug.LogWarning("ResetUpgradeBought event received with invalid or null data.");
            return;
        }
        ResetUpgrade resetUpgrade = resetUpgradeBought.ResetUpgrade;

        //if (ResetCoin.Value >= resetUpgrade.Cost)
        //{
        //    ResetCoin.Value -= resetUpgrade.Cost;
        //    resetUpgrade.SetPurchased(true);
        //}
        resetUpgrade.SetPurchased(true);
    }

    public void OnPassiveSkillBought(IGameEventDetails details)
    {
        PassiveSkillBought passiveSkillBought = details as PassiveSkillBought;
        PassiveSkill passiveSkill = passiveSkillBought.PassiveSkill;

        if(resetCoin.Value >= passiveSkill.Price)
        {
            resetCoin.Value -= passiveSkill.Price;
        }

        passiveSkill.SetPurchased(true);
    }

    public void GetResetCoin() //passzív skillekre lehet majd költeni
    {
        double calc = Math.Ceiling(gameState.TotalGain / 2500);

        GameController.Instance.resetCoin.Value += calc;
    }

    public bool CanReset()
    {
        int currentResetStage = GetResetStage();

        if (currentResetStage >= RequiredTotalGain.Length)
            return false;

        return gameState.TotalGain >= RequiredTotalGain[currentResetStage];
    }

    public void IncreaseResetStage()
    {
        resetStage.Value++;
    }

    public int GetResetStage()
    {
        return Convert.ToInt32(resetStage.Value);
    }
}
