using UnityEngine;

public class BlendControllerUV : MonoBehaviour
{
    private Renderer renderer;
    private float offset = 0.66f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        renderer = transform.GetChild(0).GetComponent<Renderer>();
        renderer.material.SetTextureOffset("_BaseMap", new Vector2(0,offset));

    }

    // Update is called once per frame
    void Update()
    {
        renderer.material.SetTextureOffset("_BaseMap", new Vector2(0, offset - GetComponent<TimeClickController>().variable * offset));
    }
}
