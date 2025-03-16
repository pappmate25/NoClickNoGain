using UnityEngine;

public class BlendControllerScale : MonoBehaviour
{
    [SerializeField]
    private GameObject filling;

    void Start()
    {
        filling = this.transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        filling.transform.localScale = new Vector3(1, GetComponent<TimeClickController>().variable, 1);
    }

}
