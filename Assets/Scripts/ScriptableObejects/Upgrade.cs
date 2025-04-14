using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "SO/Configuration/Upgrade")]
public class Upgrade : ScriptableObject
{
	public string Name;
	public string Description;

	// Supported mathematical functions
	// https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ExpressionEvaluator.Evaluate.html

	// x is the current level, the equation shows the cost of the next level
	public string CostEquation;
	// x is the current level, the equation shows the effect on the current level
	public string EffectEquation;



    [SerializeField]
    public List<MultiplierRule> multiplierRules;



    public IdleUpgradeDetails IdleUpgradeDetails;

	// This is a runtime variable
	internal double currentEffect;
	internal int currentLevel;
	

	public void Awake()
	{
		SetLevel(0);
	}

	public void SetLevel(int level)
	{
		currentLevel = level;
		UpdateEffect(currentLevel);
	}

	public double GetCumilativeCost(int targetLevel)
	{
		double cost = 0;
		for (int i = currentLevel; i < targetLevel; i++)
		{
			ExpressionEvaluator.Evaluate(CostEquation.Replace("x", i.ToString()), out double lvlCost);
			cost += lvlCost;
		}
		return Math.Ceiling(cost);
	}

	public void UpdateEffect(int level)
	{
		int multiplierValue = GetMultiplierForLevel(level);
		Debug.Log($"A jelenlegi szorzó {multiplierValue}");

        if (EffectEquation != null)
		{
			ExpressionEvaluator.Evaluate(EffectEquation.Replace("x", level.ToString()).Replace("y", multiplierValue.ToString()), out currentEffect);
		}
		else
		{
			currentEffect = 0;
		}
	}

    
    public int GetMultiplierForLevel(int level)
    {
        int result = 1;
        foreach (var rule in multiplierRules)
        {
            if (level >= rule.minLevel)
                result *= rule.multiplier;
        }

		return result;
    }
}
