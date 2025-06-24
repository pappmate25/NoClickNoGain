using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public struct Equation
{
    /// <summary>
    /// Used for matching individual tokens in the equation so that we may loop over them.
    /// </summary>
    public static Regex EquationTokenRegex = new(@"[+\-\/*^\(\)]|\d+\.?\d+|\w+", RegexOptions.Compiled);

    /// <summary>
    /// Used for validating the entire equation string to ensure it only contains valid tokens.
    /// </summary>
    public static Regex EquationValidationRegex = new(@"^([+\-\/*^\(\)]|\d+\.?\d+|\w+)+$", RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// Indicates whether the equation is valid and can be evaluated.
    /// If false, trying to evaluate this equation will throw an exception.
    /// </summary>
    [SerializeField, HideInInspector]
    private bool isValid;

    [SerializeField]
    private EquationToken[] equationTokens;

    [SerializeField]
    private string[] variableNames;

    public Equation(string equation)
    {
        if (!EquationValidationRegex.IsMatch(equation))
            throw new ArgumentException("Invalid equation.");

        equation = equation.Replace(" ", string.Empty);

        equationTokens = EquationTokenRegex.Matches(equation).Select((token) => new EquationToken(token.Value)).ToArray();

        if (equationTokens.Length == 0)
            throw new ArgumentException("Equation cannot be empty.");


        // Handle unary negation
        // The general logic is every "-" is a subtraction until it is determined that it is a unary negation.

        // If the first token is "-", since it doesn't have a number before it, it is a unary negation.
        if (equationTokens[0].TokenType == EquationTokenType.Subtraction)
        {
            equationTokens[0].TokenType = EquationTokenType.UnaryNegation;
        }

        // This loop is about finding "-" tokens where the previous token is not a number, implying that it is a unary negation.
        for (int i = 1; i < equationTokens.Length; i++)
        {
            if (equationTokens[i].TokenType != EquationTokenType.Subtraction) continue;

            if (equationTokens[i - 1].TypeGroup == TokenTypeGroup.Operator
                || equationTokens[i - 1].TokenType == EquationTokenType.LeftParenthesis)
                equationTokens[i].TokenType = EquationTokenType.UnaryNegation;
        }

        equationTokens = ConvertToRPN(equationTokens);

        variableNames = equationTokens
            .Where(token => token.TokenType == EquationTokenType.Variable)
            .Select(token => token.VariableName)
            .ToArray();

        // Needs to be set to true ahead of time so that Evaluate can be called without throwing an exception.
        isValid = true;

        try
        {
            // We do a test run of the equation with placeholder values to ensure that it is valid.
            Evaluate(equationTokens
                .Where(token => token.TokenType == EquationTokenType.Variable)
                .ToDictionary(token => token.VariableName, token => 1.0));
        }
        catch (Exception)
        {
            isValid = false;
            throw new ArgumentException($"Invalid equation: {equation}.");
        }
    }

    public readonly double Evaluate(Dictionary<string, double> variables)
    {
        if (!isValid)
        {
            throw new InvalidOperationException("Equation is not properly initialized");
        }

        Stack<double> stack = new();

        foreach (var token in equationTokens)
        {
            switch (token.TokenType)
            {
                case EquationTokenType.Constant:
                    stack.Push(token.Value);
                    break;

                case EquationTokenType.Variable:
                    stack.Push(variables[token.VariableName]);
                    break;

                case EquationTokenType.UnaryNegation:
                    stack.Push(-stack.Pop());
                    break;

                case EquationTokenType.Addition:
                    stack.Push(stack.Pop() + stack.Pop());
                    break;

                case EquationTokenType.Subtraction:
                    var subtrahend = stack.Pop();
                    var minuend = stack.Pop();
                    stack.Push(minuend - subtrahend);
                    break;

                case EquationTokenType.Multiplication:
                    stack.Push(stack.Pop() * stack.Pop());
                    break;

                case EquationTokenType.Division:
                    var divisor = stack.Pop();
                    var dividend = stack.Pop();

                    if (divisor == 0)
                    {
                        throw new DivideByZeroException("Division by zero.");
                    }

                    stack.Push(dividend / divisor);
                    break;

                case EquationTokenType.Exponentiation:
                    var exponent = stack.Pop();
                    var baseValue = stack.Pop();
                    stack.Push(Math.Pow(baseValue, exponent));
                    break;

                default:
                    throw new ArgumentException($"Invalid token: {token.TokenType}");
            }
        }

        // At the end of evaluation, there should be exactly one value left in the stack
        // which is the result of the equation.
        if (stack.Count != 1)
        {
            throw new Exception("Invalid equation.");
        }

        return stack.Pop();
    }

    /*private readonly Dictionary<string, double> ProcessVariables((string, double)[] variables)
    {
        var providedVariables = variables
            .Select(v => v.Item1)
            .ToArray();

        var equationVariables = equationTokens
            .Where(token => token.TokenType == EquationTokenType.Variable)
            .Select(token => token.VariableName)
            .ToArray();

        if (providedVariables.Length < equationVariables.Length)
        {
            throw new ArgumentException("Duplicate variable names are not allowed.");
        }

        // Check if all required variables are provided
        var missingVariables = equationVariables
            .Where(variable => !providedVariables.Contains(variable));

        var extraVariables = providedVariables
            .Where(variable => !equationVariables.Contains(variable));

        if (missingVariables.Any())
        {
            throw new ArgumentException($"Missing required variables: {string.Join(", ", missingVariables)}");
        }

        if (extraVariables.Any())
        {
            Debug.LogWarning($"Extra variables provided but not used: {string.Join(", ", extraVariables)}");
        }

        return variables.ToDictionary(v => v.Item1, v => v.Item2);
    }*/

    public readonly override string ToString()
    {
        if (!isValid)
        {
            return "Invalid Equation";
        }

        return string.Join(" ", equationTokens.Select(token =>
        {
            return token.TokenType switch
            {
                EquationTokenType.Constant => token.Value.ToString(CultureInfo.InvariantCulture),
                EquationTokenType.Variable => token.VariableName,
                EquationTokenType.LeftParenthesis => "(",
                EquationTokenType.RightParenthesis => ")",
                EquationTokenType.Addition => "+",
                EquationTokenType.Subtraction => "-",
                EquationTokenType.Multiplication => "*",
                EquationTokenType.Division => "/",
                EquationTokenType.Exponentiation => "^",
                EquationTokenType.UnaryNegation => "n",
                _ => throw new ArgumentException($"Invalid token type: {token.TokenType}"),
            };
        }));
    }

    private static EquationToken[] ConvertToRPN(EquationToken[] tokens)
    {
        // Shunting yard algorithm for turning infix notation to postfix notation
        // https://en.wikipedia.org/wiki/Shunting_yard_algorithm
        var outputTokens = new List<EquationToken>();
        var operatorStack = new Stack<EquationToken>();

        foreach (var token in tokens)
        {
            switch (token.TypeGroup)
            {
                case TokenTypeGroup.Value:
                    outputTokens.Add(token);
                    break;
                case TokenTypeGroup.Operator:
                    while (operatorStack.TryPeek(out var o2)
                            && o2.TokenType != EquationTokenType.LeftParenthesis
                            && (o2.ComparePrecedence(token) == 1 || (o2.ComparePrecedence(token) == 0 && token.IsLeftAssociative())))
                    {
                        outputTokens.Add(operatorStack.Pop());
                    }
                    operatorStack.Push(token);

                    break;
                case TokenTypeGroup.Parenthesis:
                    if (token.TokenType == EquationTokenType.LeftParenthesis)
                    {
                        operatorStack.Push(token);
                    }
                    else
                    {
                        while (true)
                        {
                            bool operatorsRemain = operatorStack.TryPeek(out var result);

                            if (!operatorsRemain)
                            {
                                throw new Exception("No matching left parenthesis found for right parenthesis.");
                            }
                            else if (result.TokenType == EquationTokenType.LeftParenthesis)
                            {
                                // Pop left parenthesis
                                operatorStack.Pop();
                                break;
                            }

                            outputTokens.Add(operatorStack.Pop());
                        }
                    }
                    break;
            }
        }

        while (operatorStack.TryPop(out var token))
        {
            if (token.TokenType == EquationTokenType.LeftParenthesis)
            {
                throw new Exception("No matching right parenthesis found for left parenthesis.");
            }

            outputTokens.Add(token);
        }

        return outputTokens.ToArray();
    }
}