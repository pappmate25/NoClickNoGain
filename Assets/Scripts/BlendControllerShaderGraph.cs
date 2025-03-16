using UnityEngine;

public class BlendControllerShaderGraph : MonoBehaviour
{
    private Material material;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        material = GetComponent<MeshRenderer>().sharedMaterial;
    }

    // Update is called once per frame
    void Update()
    {        
        material.SetFloat("_GradientValue", GetComponent<TimeClickController>().variable);
    }
}
