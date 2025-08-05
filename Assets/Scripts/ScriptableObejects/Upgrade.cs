using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Upgrade", menuName = "SO/Configuration/Upgrade")]
public class Upgrade : ScriptableObject
{
    public string Name;
    public string Description;

    // Supported mathematical functions
    // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ExpressionEvaluator.Evaluate.html

    public string BaseValueEquation;

    // x is the current level, the equation shows the cost of the next level
    public string CostEquation;
    // x is the current level, the equation shows the effect on the current level
    public string EffectEquation;

    [SerializeField]
    [FormerlySerializedAs("multiplierRules")]
    public List<MultiplierRule> MultiplierRules;

    public IdleUpgradeDetails IdleUpgradeDetails;

    // This is a runtime variable
    internal double currentEffect;
    internal int currentLevel;
    internal int currentBaseValue;

    [SerializeField]
    [FormerlySerializedAs("_baseValueEquation")]
    private Equation baseValueEquation;
    [SerializeField]
    [FormerlySerializedAs("_costEquation")]
    private Equation costEquation;
    [SerializeField]
    [FormerlySerializedAs("_effectEquation")]
    private Equation effectEquation;

    [SerializeField, HideInInspector]
    [FormerlySerializedAs("_lastParsedBaseValueEquation")]
    private string lastParsedBaseValueEquation;
    [SerializeField, HideInInspector]
    [FormerlySerializedAs("_lastParsedCostEquation")]
    private string lastParsedCostEquation;
    [SerializeField, HideInInspector]
    [FormerlySerializedAs("_lastParsedEffectEquation")]
    private string lastParsedEffectEquation;


    double resetMultiplier1;
    double resetMultiplier2;
    double resetMultiplier3;

    public void OnEnable()
    {
        lastParsedBaseValueEquation ??= "";
        lastParsedCostEquation ??= "";
        lastParsedEffectEquation ??= "";
    }

    public void ParseEquations()
    {
        try
        {
            baseValueEquation = new Equation(BaseValueEquation);
            lastParsedBaseValueEquation = BaseValueEquation;

            costEquation = new Equation(CostEquation);
            lastParsedCostEquation = CostEquation;

            effectEquation = new Equation(EffectEquation);
            lastParsedEffectEquation = EffectEquation;

            Debug.Log("Equations parsed successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing equations: {ex.Message}");
        }
    }

    public bool BaseValueEquationDirty()
    {
        return !string.Equals(BaseValueEquation ?? "", lastParsedBaseValueEquation ?? "");
    }

    public bool CostEquationDirty()
    {
        return !string.Equals(CostEquation ?? "", lastParsedCostEquation ?? "");
    }

    public bool EffectEquationDirty()
    {
        return !string.Equals(EffectEquation ?? "", lastParsedEffectEquation ?? "");
    }

    public void SetLevel(int level)
    {
        currentLevel = level;
        UpdateEffect(currentLevel);
    }

    public virtual double GetCumulativeCost(int targetLevel)
    {
        Dictionary<string, double> variables = new Dictionary<string, double>
        {
            { "x", currentLevel }
        };

        double cost = 0;
        for (int i = currentLevel; i < targetLevel; i++)
        {
            variables["x"] = i;
            double lvlCost = costEquation.Evaluate(variables);
            cost += lvlCost;
        }
        return Math.Ceiling(cost);
    }

    public int GetTargetLevelToTarget(BuyQuantity quantity, double availableFunds)
    {
        return quantity switch
        {
            BuyQuantity.ONE => currentLevel + 1,
            BuyQuantity.FIVE => currentLevel + 5,
            BuyQuantity.TEN => currentLevel + 10,
            BuyQuantity.HUNDRED => currentLevel + 100,
            BuyQuantity.MAX => GetMaxAchievableLevel(availableFunds),
            BuyQuantity.BREAKPOINT => MultiplierRules.Find((rule => rule.MinLevel > currentLevel)).MinLevel,
            _ => throw new ArgumentOutOfRangeException(nameof(quantity), quantity, null)
        };
    }

    public int GetMaxAchievableLevel(double availableFunds)
    {
        var variables = new Dictionary<string, double> { { "x", currentLevel } };
        double totalCost = 0;

        while (true)
        {
            totalCost += costEquation.Evaluate(variables);
            if (totalCost > availableFunds)
                break;
            variables["x"]++;
        }

        return (int)variables["x"] == currentLevel ? currentLevel + 1 : (int)variables["x"];
    }

    public virtual void UpdateEffect(int level)
    {
        double multiplierValue = GetMultiplierForLevel(level);

        double multiplier1 = resetMultiplier1;
        double multiplier2 = resetMultiplier2;
        double multiplier3 = resetMultiplier3;

        if (EffectEquation != null)
        {
            currentEffect = effectEquation.Evaluate(new Dictionary<string, double>
            {
                { "x", currentBaseValue },
                { "y", level },
                { "z", multiplierValue },

                { "j", multiplier1},
                { "k", multiplier2},
                { "l", multiplier3},

            });
        }
        else
        {
            currentEffect = 0;
        }
    }

    public double GetTargetLevelIncome(int level)
    {
        double income;

        double multiplierValue = GetMultiplierForLevel(level);

        double multiplier1 = resetMultiplier1;
        double multiplier2 = resetMultiplier2;
        double multiplier3 = resetMultiplier3;

        if (EffectEquation != null)
        {
            income = effectEquation.Evaluate(new Dictionary<string, double>
            {
                { "x", currentBaseValue },
                { "y", level },
                { "z", multiplierValue },

                { "j", multiplier1},
                { "k", multiplier2},
                { "l", multiplier3},

            });
            return income;
        }
        else
        {
            income = 0;
        }
        return income;
    }

    public virtual void SetResetMultiplier(double multiplier, int resetRank)
    {
        if (BaseValueEquation != null)
        {
            currentBaseValue = (int)baseValueEquation.Evaluate(new Dictionary<string, double>());
        }

        switch (resetRank)
        {
            case 1:
                resetMultiplier1 = multiplier;
                break;

            case 2:
                resetMultiplier2 = multiplier;
                break;

            case 3:
                resetMultiplier3 = multiplier;
                break;
        }

        UpdateEffect(currentLevel);
    }

    public double GetMultiplierForLevel(int level)
    {
        double result = 1;
        foreach (var rule in MultiplierRules)
        {
            if (level >= rule.MinLevel)
            {
                result *= rule.Multiplier;
            }
        }

        return result;
    }
}
