using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHandler : MonoBehaviour
{
    [SerializeField]
    private GameEvent ClickEvent;

    void Update()
    {
		if (Input.GetMouseButtonDown(0))
		{
            ClickEvent.Raise(NoDetails.Instance);
        }
    }

    private void CastClickRay()
    {
        Camera camera = Camera.main;
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = camera.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, camera.nearClipPlane));
        if(Physics.Raycast(ray, out var hit))
        {
            ClickEvent.Raise(NoDetails.Instance);
        }
    }
}
