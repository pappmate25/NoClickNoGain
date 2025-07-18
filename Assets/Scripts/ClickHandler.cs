using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ClickHandler : MonoBehaviour
{
    [SerializeField]
    private GameEvent clickEvent;

    private int clickCounter;

    private InputSystem_Actions inputActions;

    [SerializeField]
    private UIDocument uiDocument;
    private VisualElement rootElement;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();

        inputActions.Player.Click.performed += HandleClick;

        rootElement = uiDocument.rootVisualElement;
    }

    void HandleClick(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = inputActions.Player.PointerPosition.ReadValue<Vector2>();

        var position = RuntimePanelUtils.ScreenToPanel(
            rootElement.panel,
            screenPosition
        );

        position.y = rootElement.resolvedStyle.height - position.y; // Adjust Y coordinate to match UI origin

        var picked = rootElement.panel.Pick(position);

        //Debug.Log($"Input position: {position}, Over UI: {picked != null}");

        if (picked != null)
        {
            Debug.Log($"Picked element: {picked.name}");
            Debug.Log(picked.parent.name);
        }

        if (picked == null && UIController.IsClaimed)
        {
            double clickValue = GameController.Instance.GetClickValue();
            clickEvent.Raise(NoDetails.Instance);
            clickCounter++;
            Debug.Log($"Total clicks: {clickCounter}");

            ShowGainValue(screenPosition, clickValue);
        }
    }

    private void ShowGainValue(Vector2 position, double gain)
    {
        position.y = rootElement.resolvedStyle.height - position.y;


        //Random X|Y offset
        //float yOffset = Random.Range(-200f, 200f);
        //float xOffset = Random.Range(-300f, 250f);
        //position.y += yOffset;
        //position.x += xOffset;


        Label clickPopUpLabel = new Label($"+{NumberFormatter.FormatNumber(gain)}");
        clickPopUpLabel.AddToClassList("clickPopUpLabelStyle");
        clickPopUpLabel.pickingMode = PickingMode.Ignore;

        rootElement.Add(clickPopUpLabel);

        StartCoroutine(ShowGainFloatingAnimation(clickPopUpLabel, position));
    }


    //without the random offset:
        //duration = 1.0f;
        //float offsetY = -275f * progress; // Upwards floating

    //with the random offset:
        //duration = 1.5f;
        //float offsetY = -50f * progress; // Upwards floating
    private IEnumerator ShowGainFloatingAnimation(Label label, Vector2 startPos)
    {
        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;

            float offsetY = -325f * progress; // Upwards floating
            float opacity = 1.1f - progress;

            label.style.translate = new StyleTranslate(new Translate(startPos.x + 15f, startPos.y -40f + offsetY, 0)); //a bit up and right from the click position --> without random offset --> startPos.x + 15f, startPos.y -40f + offsetY
            label.style.opacity = opacity;

            elapsed += Time.deltaTime;
            yield return null;
        }

        rootElement.Remove(label);
    }
}
