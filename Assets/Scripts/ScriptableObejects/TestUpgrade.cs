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

    [ContextMenu("Parse Equations")]
    public void ParseEquations()
    {
        _baseValueEquation = new Equation(BaseValueEquation);
        _costEquation = new Equation(CostEquation);
        _effectEquation = new Equation(EffectEquation);
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
}
