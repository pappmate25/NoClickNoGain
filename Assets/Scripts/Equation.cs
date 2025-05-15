using System;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public struct Equation
{
    public static Regex EquationRegex = new Regex(@"^[\d+\s ]+$");

    [SerializeField]
    [HideInInspector]
    private bool isValid;

    [SerializeField, HideInInspector]
    private double lhs;

    [SerializeField, HideInInspector]
    private double rhs;

    [SerializeField]
    private EquationToken[] equationTokens;

    public Equation(string equation)
    {
        if (!EquationRegex.IsMatch(equation))
        {
            isValid = false;
            throw new ArgumentException("Invalid equation.");
        }

        equation = equation.Replace(" ", string.Empty);

        string[] operands = equation.Split("+");

        if (!double.TryParse(operands[0].Trim(), out double tempLhs))
        {
            isValid = false;
            throw new ArgumentException("Invalid left-hand side operand");
        }

        if (!double.TryParse(operands[1].Trim(), out double tempRhs))
        {
            isValid = false;
            throw new ArgumentException("Invalid right-hand side operand");
        }

        isValid = true;

        lhs = tempLhs;
        rhs = tempRhs;

        equationTokens = Array.Empty<EquationToken>();
    }

    public double Evaluate(params (string, double)[] variables)
    {
        if (!isValid)
        {
            throw new Exception("Equation is not properly initialized");
        }

        return lhs + rhs;
    }
}

[Serializable]
public struct EquationToken
{
    public EquationTokenType Type;
    public double Value;
    public string VariableName;
}

[Serializable]
public enum EquationTokenType
{
    Constant,
    Variable,
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Exponentiation,
}
