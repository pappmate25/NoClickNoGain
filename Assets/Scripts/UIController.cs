using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private ClickerProperties properties;
    [SerializeField]
    private List<GameObject> cubes;

    private Label shaderCubeLabel;
    private Label scaleCubeLabel;
    private Label textureCubeLabel;

    private Button fillSpeedButton;
    private Button clickAmountButton;

    private VisualElement root;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        shaderCubeLabel = root.Q<Label>("shader-cube-label");
        scaleCubeLabel = root.Q<Label>("scale-cube-label");
        textureCubeLabel = root.Q <Label>("texture-cube-label");
        fillSpeedButton = root.Q<Button>("fill-speed-button");
        clickAmountButton = root.Q<Button>("click-amount-button");

        fillSpeedButton.RegisterCallback<ClickEvent>(FillSpeedButtonClicked);
        clickAmountButton.RegisterCallback<ClickEvent>(ClickAmountButtonClicked);

        shaderCubeLabel.dataSource = cubes[0].GetComponent<TimeClickController>();
        shaderCubeLabel.SetBinding(nameof(Label.text), new DataBinding { dataSourcePath = PropertyPath.FromName(nameof(TimeClickController.counter)) });

        scaleCubeLabel.dataSource = cubes[1].GetComponent<TimeClickController>();
        scaleCubeLabel.SetBinding(nameof(Label.text), new DataBinding { dataSourcePath = PropertyPath.FromName(nameof(TimeClickController.counter)) });

        textureCubeLabel.dataSource = cubes[2].GetComponent<TimeClickController>();
        textureCubeLabel.SetBinding(nameof(Label.text), new DataBinding { dataSourcePath = PropertyPath.FromName(nameof(TimeClickController.counter)) });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void ClickAmountButtonClicked(ClickEvent evt)
    {
        properties.timeAddedPerClick *= 2;
        properties.valueChanged = true;
    }

    private void FillSpeedButtonClicked(ClickEvent evt)
    {
        properties.timeToFill /= 2;
        properties.valueChanged = true;
    }
}
