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
    private GameObject animatedGranny;

    private Label animatedLabel;

    private Button fillSpeedButton;
    private Button clickAmountButton;

    private VisualElement root;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        animatedLabel = root.Q<Label>("animated-label");
        fillSpeedButton = root.Q<Button>("fill-speed-button");
        clickAmountButton = root.Q<Button>("click-amount-button");

        fillSpeedButton.RegisterCallback<ClickEvent>(FillSpeedButtonClicked);
        clickAmountButton.RegisterCallback<ClickEvent>(ClickAmountButtonClicked);

        animatedLabel.dataSource = animatedGranny.GetComponent<TimeClickController>();
        animatedLabel.SetBinding(nameof(Label.text), new DataBinding { dataSourcePath = PropertyPath.FromName(nameof(TimeClickController.counter)) });
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
