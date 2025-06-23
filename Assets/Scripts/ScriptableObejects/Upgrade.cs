using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

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
	public List<MultiplierRule> multiplierRules;

	public IdleUpgradeDetails IdleUpgradeDetails;

	// This is a runtime variable
	internal double currentEffect;
	internal int currentLevel;
	internal int currentBaseValue;

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

	public void OnEnable()
	{
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

	public void SetLevel(int level)
	{
		currentLevel = level;
		UpdateEffect(currentLevel);
	}

	public virtual double GetCumulativeCost(int targetLevel)
	{
		double cost = 0;
		for (int i = currentLevel; i < targetLevel; i++)
		{
			double lvlCost = _costEquation.Evaluate(("x", i));
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
			BuyQuantity.BREAKPOINT => multiplierRules.Find((rule => rule.minLevel > currentLevel)).minLevel,
			_ => throw new ArgumentOutOfRangeException(nameof(quantity), quantity, null)
		};
	}

	public int GetMaxAchievableLevel(double availableFunds)
	{
		int maxLevel = currentLevel + 1;

		while (GetCumulativeCost(++maxLevel) <= availableFunds) ;

		return maxLevel - 1;
	}

	public virtual void UpdateEffect(int level)
	{
		double multiplierValue = GetMultiplierForLevel(level);

		if (EffectEquation != null)
		{
			currentEffect = _effectEquation.Evaluate(("x", level), ("y", level), ("z", multiplierValue), ("k", currentBaseValue));
		}
		else
		{
			currentEffect = 0;
		}
	}

	public virtual void SetMultipliedBaseValue(int resetMultiplier) //after a reset upgrade buy
	{
		if (BaseValueEquation != null)
		{
			currentBaseValue = (int)_baseValueEquation.Evaluate(("k", resetMultiplier));
		}
	}

	public double GetMultiplierForLevel(int level)
	{
		double result = 1;
		foreach (var rule in multiplierRules)
		{
			if (level >= rule.minLevel)
			{
				result *= rule.multiplier;
			}
		}

		return result;
	}
}
