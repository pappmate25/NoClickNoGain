using UnityEngine;

public class TimeClickController : MonoBehaviour
{
    public float variable = 0;
    private float timerStarted;
    public int counter = 0;

    public float timeToFill = 10;
    public float timeAddedPerClick = 0.5f;
    private float fillPerSec;

    [SerializeField]
    private Collider collider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timerStarted = Time.time;
        fillPerSec = 1 / timeToFill;
    }

    // Update is called once per frame
    void Update()
    {
        if(variable < 1)
        {
            variable += fillPerSec * Time.deltaTime;
        }
        else
        {
            timerStarted = Time.time;
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
