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
    public static Regex EquationRegex = new(@"[+\-\/*^\(\)]|\d+\.?\d+|\w+", RegexOptions.Compiled);
    /// <summary>
    /// Used for validating the entire equation string to ensure it only contains valid tokens.
    /// </summary>
    public static Regex ValidationRegex = new(@"^([+\-\/*^\(\)]|\d+\.?\d+|\w+)+$", RegexOptions.Compiled | RegexOptions.Singleline);

    [SerializeField]
    [HideInInspector]
    private bool isValid;

    [SerializeField]
    private EquationToken[] equationTokens;

    public Equation(string equation)
    {
        isValid = false;

        if (!ValidationRegex.IsMatch(equation))
            throw new ArgumentException("Invalid equation.");

        equation = equation.Replace(" ", string.Empty);

        equationTokens = EquationRegex.Matches(equation).Select((token) => new EquationToken(token.Value)).ToArray();

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


        // Shunting yard algorithm for turning infix notation to postfix notation
        // https://en.wikipedia.org/wiki/Shunting_yard_algorithm
        var outputQueue = new Queue<EquationToken>();
        var operatorStack = new Stack<EquationToken>();

        foreach (var token in equationTokens)
        {
            switch (token.TypeGroup)
            {
                case TokenTypeGroup.Value:
                    outputQueue.Enqueue(token);
                    break;
                case TokenTypeGroup.Operator:
                    while (operatorStack.TryPeek(out var o2)
                            && o2.TokenType != EquationTokenType.LeftParenthesis
                            && (o2.ComparePrecedence(token) == 1 || (o2.ComparePrecedence(token) == 0 && token.IsLeftAssociative())))
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
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

                            outputQueue.Enqueue(operatorStack.Pop());
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

            outputQueue.Enqueue(token);
        }

        equationTokens = outputQueue.ToArray();

        if (!ValidateEquation())
        {
            throw new ArgumentException($"Invalid equation: {equation}.");
        }

        isValid = true;
    }

    public readonly bool ValidateEquation()
    {
        try
        {
            Stack<double> stack = new();

            foreach (var token in equationTokens)
            {
                switch (token.TokenType)
                {
                    case EquationTokenType.Constant:
                        stack.Push(token.Value);
                        break;
                    case EquationTokenType.Variable:
                        stack.Push(1);
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

            if (stack.Count != 1)
            {
                throw new Exception("Invalid equation.");
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public readonly double Evaluate(params (string, double)[] variables)
    {
        if (!isValid)
        {
            throw new InvalidOperationException("Equation is not properly initialized");
        }

        var variableDict = checkVariables(variables);

        Stack<double> stack = new();

        foreach (var token in equationTokens)
        {
            switch (token.TokenType)
            {
                case EquationTokenType.Constant:
                    stack.Push(token.Value);
                    break;
                case EquationTokenType.Variable:
                    stack.Push(variableDict[token.VariableName]);
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

        if (stack.Count != 1)
        {
            throw new Exception("Invalid equation.");
        }

        return stack.Pop();
    }

    private readonly Dictionary<string, double> checkVariables((string, double)[] variables)
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

        return variables.ToDictionary(v => v.Item1, v => v.Item2);
    }

    public override string ToString()
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
}