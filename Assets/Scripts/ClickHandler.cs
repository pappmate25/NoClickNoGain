using UnityEngine;

public class ClickHandler : MonoBehaviour
{
    [SerializeField]
    private GameEvent ClickEvent;

    // Update is called once per frame
    void Update()
    {
		if (Input.GetMouseButtonDown(0))
		{
			CastClickRay();
		}
    }

    private void CastClickRay()
    {
        var camera = Camera.main;
        var mousePosition = Input.mousePosition;
        var ray = camera.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, camera.nearClipPlane));
        if(Physics.Raycast(ray, out var hit) && hit.collider.gameObject == gameObject)
        {
            ClickEvent.Raise(NoDetails.Instance);
        }
    }
}
