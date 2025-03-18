using UnityEngine;

public class TimeClickController : MonoBehaviour
{
    public float variable = 0;
    private float timerStarted;
    public int counter = 0;

    public ClickerProperties properties;
    public float timeToFill;
    public float timeAddedPerClick;
    private float fillPerSec;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeToFill = properties.timeToFill;
        timeAddedPerClick = properties.timeAddedPerClick;
        fillPerSec = 1 / timeToFill;
    }

    // Update is called once per frame
    void Update()
    {
        if(properties.valueChanged)
        {
            timeToFill = properties.timeToFill;
            timeAddedPerClick = properties.timeAddedPerClick;
            fillPerSec = 1 / timeToFill;
        }

        variable += fillPerSec * Time.deltaTime;
        if (variable > 1)
        {
            counter++;
            if (variable > 1) variable--;
            else variable = 0;
        }

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
            variable += fillPerSec * timeAddedPerClick;
        }
    }
}
