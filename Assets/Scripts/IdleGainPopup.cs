using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class IdleGainPopup : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UpgradeList idleUpgradeList;
    [SerializeField] private GameEvent upgradeBoughtEvent;


    private VisualElement root;
    private ProgressBar[] idleBars;
    private double[] idleCurrentValues;
    private const double floatingNumberErrorMargin = 1e-3;

    void Start()
    {
        root = uiDocument.rootVisualElement;
        idleBars = new ProgressBar[idleUpgradeList.Upgrades.Length];

        for (int i = 0; i < idleUpgradeList.Upgrades.Length; i++)
        {
            idleBars[i] = root.Q<ProgressBar>(UIController.IdleNameFormat(idleUpgradeList.Upgrades[i].Name));
        }

        idleCurrentValues = GetIdleValue();
    }

    
    void Update()
    {
        if (UIController.IsClaimed)
        {
            for (int i = 0; i < idleUpgradeList.Upgrades.Length; i++)
            {
                Upgrade idleUpgrade = idleUpgradeList.Upgrades[i];

                IdleUpgradeDetails idleUpgradeDetails = idleUpgrade.IdleUpgradeDetails;

                while (idleUpgradeDetails.CurrentProgress >= 1.0 - floatingNumberErrorMargin)
                {
                    //Between -0.000123 and 0 we make up for what we missed between 0.999876 and 1
                    idleUpgradeDetails.CurrentProgress -= 1.0;


                    ShowGainValue(idleBars[i], idleCurrentValues[i]);
                }
            }
        }
    }

    private void ShowGainValue(ProgressBar progressBar, double gain)
    {
        Rect layout = progressBar.worldBound;

        float progressBarCenter = layout.x + (layout.width / 2f - 125f); //-125f --> Label width 250/2
        Vector2 popupPos = new Vector2(progressBarCenter, layout.y);


        Label idlePopupLabel = new Label(name = $"+{NumberFormatter.FormatNumber(gain)}");
        idlePopupLabel.AddToClassList("idlePopUpLabelStyle");
        idlePopupLabel.pickingMode = PickingMode.Ignore;
        root.Add(idlePopupLabel);


        StartCoroutine(ShowGainFloatingAnimation(idlePopupLabel, popupPos));
    }

    private IEnumerator ShowGainFloatingAnimation(Label label, Vector2 startPos)
    {
        float duration = 1.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;

            float offsetY = -160f * progress; // Upwards floating
            float opacity = 1.1f - progress;

            label.style.translate = new StyleTranslate(new Translate(startPos.x, startPos.y - 40f + offsetY, 0));
            label.style.opacity = opacity;

            elapsed += Time.deltaTime;
            yield return null;
        }

        root.Remove(label);
    }

    private double[] GetIdleValue()
    {
        double[] idleValue = new double[idleUpgradeList.Upgrades.Length];


        for (int i = 0; i < idleUpgradeList.Upgrades.Length; i++)
        {
            idleValue[i] = idleUpgradeList.Upgrades[i].currentEffect;
        }

        return idleValue;
    }
    public void OnUpgradeBought(IGameEventDetails details)
    {
        UpgradeBought upgradeBought = details as UpgradeBought;
        Upgrade upgrade = upgradeBought.Upgrade;
        
        if(!upgrade.IsClickUpgrade)
        {
            UpdateIdleCurrentValue();
        }
    }

    public void UpdateIdleCurrentValue()
    {
        idleCurrentValues = GetIdleValue();
    }

}
