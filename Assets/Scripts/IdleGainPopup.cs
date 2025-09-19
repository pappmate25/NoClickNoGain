using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class IdleGainPopup : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UpgradeList idleUpgradeList;
    [SerializeField] private GameEvent upgradeBoughtEvent;


    private VisualElement root;
    private VisualElement closestSiblingToPlaceBehind;
    private ProgressBar[] idleBars;
    private double[] idleCurrentValues;
    private const double floatingNumberErrorMargin = 1e-3;

    void Start()
    {
        closestSiblingToPlaceBehind = uiDocument.rootVisualElement.Q("upgrade-section");
        root = closestSiblingToPlaceBehind.parent;
        
        idleBars = new ProgressBar[idleUpgradeList.Upgrades.Length];

        for (int i = 0; i < idleUpgradeList.Upgrades.Length; i++)
        {
            idleBars[i] = root.Q<ProgressBar>(UIController.IdleNameFormat(idleUpgradeList.Upgrades[i].Name));
            DisablePicking(idleBars[i]);
        }

        idleCurrentValues = GetIdleValue();
    }
    
    private static void DisablePicking(VisualElement element)
    {
        element.pickingMode = PickingMode.Ignore;
        foreach (var child in element.Children())
        {
            DisablePicking(child);
        }
    }

    public void ShowGainValue(int progressBarIndex, double gain)
    {
        ProgressBar progressBar = idleBars[progressBarIndex];
        
        Rect layout = progressBar.worldBound;

        float progressBarCenter = layout.x + (layout.width / 2f - 125f); //-125f --> Label width 250/2
        Vector2 popupPos = new Vector2(progressBarCenter, layout.y);


        Label idlePopupLabel = new Label(name = $"+{NumberFormatter.FormatNumber(gain)}");
        idlePopupLabel.AddToClassList("idlePopUpLabelStyle");
        idlePopupLabel.pickingMode = PickingMode.Ignore;
        root.Add(idlePopupLabel);
        idlePopupLabel.PlaceBehind(closestSiblingToPlaceBehind);

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
