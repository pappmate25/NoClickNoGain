using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHandler : MonoBehaviour
{
    [SerializeField]
    private GameEvent ClickEvent;

    private int ClickCounter;

    void Update()
    {
		if (Input.GetMouseButtonDown(0) && !UIInteraction.IsPointerOverUI && UIController.isClaimed)
		{
            ClickEvent.Raise(NoDetails.Instance);
            ClickCounter++;
            Debug.Log($"Total clicks: {ClickCounter}");
        }
    }

    //private void CastClickRay()
    //{
    //    Camera camera = Camera.main;
    //    Vector3 mousePosition = Input.mousePosition;
    //    Ray ray = camera.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, camera.nearClipPlane));
    //    if(Physics.Raycast(ray, out var hit))
    //    {
    //        ClickEvent.Raise(NoDetails.Instance);
    //    }
    //}
}
