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
            clickEvent.Raise(NoDetails.Instance);
            clickCounter++;
            Debug.Log($"Total clicks: {clickCounter}");
        }
    }
}
