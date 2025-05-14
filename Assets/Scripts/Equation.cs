using System;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public struct Equation
{
    public static Regex EquationRegex = new Regex(@"^[\d+\s]+$");

    [SerializeField]
    [HideInInspector]
    private bool isValid;

    [HideInInspector]
    [SerializeField]
    private double lhs;
    [HideInInspector]
    [SerializeField]
    private double rhs;

    public Equation(string equation)
    {
        if (!EquationRegex.IsMatch(equation))
        {
            isValid = false;
            throw new ArgumentException("Invalid equation.");
        }

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

        lhs = tempLhs;
        rhs = tempRhs;

        isValid = true;
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
