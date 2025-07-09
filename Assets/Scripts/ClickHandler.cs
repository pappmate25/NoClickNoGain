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
    private UIDocument rootElement;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();

        inputActions.Player.Click.performed += HandleClick;
    }

    void HandleClick(InputAction.CallbackContext context)
    {
        Vector2 position = inputActions.Player.PointerPosition.ReadValue<Vector2>();
        var picked = rootElement.rootVisualElement.panel.Pick(position);

        Debug.Log($"Input position: {position}, Over UI: {picked != null}");

        if (!(picked != null) && UIController.isClaimed)
        {
            ClickEvent.Raise(NoDetails.Instance);
            ClickCounter++;
            Debug.Log($"Total clicks: {ClickCounter}");
        }
    }
}
