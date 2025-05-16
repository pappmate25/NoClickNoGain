using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public struct Equation
{
    public static Regex TestEquationRegex = new Regex(@"^[\d+\s ]+$");
    public static Regex EquationRegex = new Regex(@"[+\-\/*^\(\)]|\d+|\w+");

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

        var tokens = EquationRegex.Matches(equation);
        equationTokens = tokens.ToList().Select((token) => new EquationToken(token.Value)).ToArray();
    }

    public double Evaluate(params (string, double)[] variables)
    {
        if (!isValid)
        {
            throw new Exception("Equation is not properly initialized");
        }

        checkVariables(variables);

        List<double> variableValues = new();

        return lhs + rhs;
    }

    private void checkVariables((string, double)[] variables)
    {
        var variableTokens = equationTokens
            .Where(token => token.TokenType == EquationTokenType.Variable)
            .Select(token => token.VariableName)
            .Distinct()
            .ToArray();

        var providedVarNames = variables
            .Select(v => v.Item1)
            .Distinct()
            .ToArray();

        var missingVars = variableTokens
            .Where(varName => !providedVarNames.Contains(varName))
            .ToArray();

        if (missingVars.Length > 0)
        {
            throw new ArgumentException($"Missing required variables: {string.Join(", ", missingVars)}");
        }

        var extraVars = providedVarNames
            .Where(varName => !variableTokens.Contains(varName))
            .ToArray();

        if (extraVars.Length > 0)
        {
            Debug.LogWarning($"Extra variables provided but not used: {string.Join(", ", extraVars)}");
        }
    }
}

[Serializable]
public struct EquationToken
{
    private static Regex VariableNameRegex = new(@"^[a-zA-Z]+$");

    public EquationTokenType TokenType;
    public double Value;
    public string VariableName;

    public EquationToken(string tokenString)
    {
        Value = 0;
        VariableName = string.Empty;

        switch (tokenString)
        {
            case "+":
                TokenType = EquationTokenType.Addition;
                break;
            case "-":
                TokenType = EquationTokenType.Subtraction;
                break;
            case "*":
                TokenType = EquationTokenType.Multiplication;
                break;
            case "/":
                TokenType = EquationTokenType.Division;
                break;
            case "^":
                TokenType = EquationTokenType.Exponentiation;
                break;
            default:
                if (double.TryParse(tokenString, out double value))
                {
                    TokenType = EquationTokenType.Constant;
                    Value = value;
                }
                else if (VariableNameRegex.IsMatch(tokenString))
                {
                    TokenType = EquationTokenType.Variable;
                    VariableName = tokenString;
                }
                else
                    throw new ArgumentException($"Invalid token: {tokenString}");

                break;
        }
    }
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
