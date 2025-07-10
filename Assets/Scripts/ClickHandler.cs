using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ClickHandler : MonoBehaviour
{
    [SerializeField]
    private GameEvent ClickEvent;

    private int ClickCounter;

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

        var picked = rootElement.panel.Pick(position);

        Debug.Log($"Input position: {position}, Over UI: {picked != null}");

        if (picked != null)
        {
            Debug.Log($"Picked element: {picked.name}");
            Debug.Log(picked.parent.name);
        }

        if (picked == null && UIController.isClaimed)
        {
            ClickEvent.Raise(NoDetails.Instance);
            ClickCounter++;
            Debug.Log($"Total clicks: {ClickCounter}");
        }
    }
}
