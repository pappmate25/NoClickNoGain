using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TestUpgrade", menuName = "SO/Configuration/TestUpgrade")]
public class TestUpgrade : Upgrade
{
    [SerializeField]
    private Equation _baseValueEquation;
    [SerializeField]
    private Equation _costEquation;
    [SerializeField]
    private Equation _effectEquation;

    [SerializeField, HideInInspector]
    private string _lastParsedBaseValueEquation;
    [SerializeField, HideInInspector]
    private string _lastParsedCostEquation;
    [SerializeField, HideInInspector]
    private string _lastParsedEffectEquation;

    public new void OnEnable()
    {
        base.OnEnable();

        // Initialize last parsed values if they're null
        _lastParsedBaseValueEquation ??= "";

        _lastParsedCostEquation ??= "";

        _lastParsedEffectEquation ??= "";
    }

    public void ParseEquations()
    {
        try
        {
            _baseValueEquation = new Equation(BaseValueEquation);
            _lastParsedBaseValueEquation = BaseValueEquation;

            _costEquation = new Equation(CostEquation);
            _lastParsedCostEquation = CostEquation;

            _effectEquation = new Equation(EffectEquation);
            _lastParsedEffectEquation = EffectEquation;

            Debug.Log("Equations parsed successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing equations: {ex.Message}");
        }
    }

    public override double GetCumulativeCost(int targetLevel)
    {
        double cost = 0;
        for (int i = currentLevel; i < targetLevel; i++)
        {
            double lvlCost = _costEquation.Evaluate(("x", i));
            cost += lvlCost;
        }
        return Math.Ceiling(cost);
    }

    public override void UpdateEffect(int level)
    {
        int multiplierValue = GetMultiplierForLevel(level);

        if (EffectEquation != null)
        {
            currentEffect = _effectEquation.Evaluate(("x", multiplierValue));
        }
        else
        {
            currentEffect = 0;
        }
    }
    public override void SetMultipliedBaseValue(int resetMultiplier) //after a reset upgrade buy
    {
        if (BaseValueEquation != null)
        {
            currentBaseValue = (int)_baseValueEquation.Evaluate(("k", resetMultiplier));
        }
    }

    public bool BaseValueEquationDirty()
    {
        return !string.Equals(BaseValueEquation ?? "", _lastParsedBaseValueEquation ?? "");
    }

    public bool CostEquationDirty()
    {
        return !string.Equals(CostEquation ?? "", _lastParsedCostEquation ?? "");
    }

    public bool EffectEquationDirty()
    {
        return !string.Equals(EffectEquation ?? "", _lastParsedEffectEquation ?? "");
    }
}
