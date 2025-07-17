using UnityEngine;

public static class RandomEquationGenerator
{
    private static readonly string[] operators = new[] { "+", "-", "*", "/" };

    public const int MinNumber = -1000;
    public const int MaxNumber = 1000;

    public const float ProbabilityOfNumber = 0.2f;
    public const float ProbabilityOfParentheses = 0.2f;

    public static string GenerateRandomEquation(int seed, int maxDepth, int maxOperators, System.Random random = null)
    {
        random ??= new System.Random(seed + maxDepth + maxOperators);

        if (maxDepth <= 0 || maxOperators <= 0)
        {
            // Base case: just return a number
            return Random.Range(MinNumber, MaxNumber).ToString();
        }

        if ((double)random.NextDouble() < ProbabilityOfNumber)
        {
            return Random.Range(MinNumber, MaxNumber).ToString();
        }

        // Build a binary expression
        string left = GenerateRandomEquation(seed, maxDepth - 1, maxOperators - 1, random);
        string op = operators[random.Next(operators.Length)];
        string right = GenerateRandomEquation(seed, maxDepth - 1, maxOperators - 1, random);

        if (random.NextDouble() < ProbabilityOfParentheses)
        {
            return "(" + left + " " + op + " " + right + ")";
        }
        else
        {
            return left + op + right;
        }
    }
}
