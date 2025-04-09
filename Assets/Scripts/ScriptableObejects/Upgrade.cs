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
		return cost;
	}

	public void UpdateEffect(int level)
	{
		if (EffectEquation != null)
		{
			ExpressionEvaluator.Evaluate(EffectEquation.Replace("x", level.ToString()), out currentEffect);
		}
		else
		{
			currentEffect = 0;
		}
	}
}
