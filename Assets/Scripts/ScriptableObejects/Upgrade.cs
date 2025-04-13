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
    // x is the current level, the equation shows the effect on the current level
    //public string Multiplier;


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
        float multiplierValue = 1;

		//if (Multiplier != null)
		//{
		//	ExpressionEvaluator.Evaluate(Multiplier.Replace("x", level.ToString()), out multiplierValue);
		//	Debug.Log(multiplierValue + " szorzó");
		//}

		//Multiplier Breakpoints
		if (level >= 10)
		{
			multiplierValue = Mathf.Floor(level / 10) * 2;
		}

		if (EffectEquation != null)
		{
			ExpressionEvaluator.Evaluate(EffectEquation.Replace("x", level.ToString()).Replace("y", multiplierValue.ToString()), out currentEffect);
		}
		else
		{
			currentEffect = 0;
		}
	}
}
