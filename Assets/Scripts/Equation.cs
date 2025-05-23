using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public struct Equation
{
    public static Regex EquationRegex = new Regex(@"[+\-\/*^\(\)]|\d+\.\d+|\w+");

    [SerializeField]
    [HideInInspector]
    private bool isValid;

    [SerializeField]
    private EquationToken[] equationTokens;

    public Equation(string equation)
    {
        isValid = false;
        equationTokens = new EquationToken[] { };

        try
        {
            if (!EquationRegex.IsMatch(equation))
            {
                isValid = false;
                throw new ArgumentException("Invalid equation.");
            }

            equation = equation.Replace(" ", string.Empty);

            var tokens = EquationRegex.Matches(equation);
            equationTokens = tokens.ToList().Select((token) => new EquationToken(token.Value)).ToArray();

            // Handle unary negation
            // If a minus sign is at the beginning or after another operator then assume it is a unary negation symbol.
            // TODO: implement replacing minuses with unary negation enums
            // TODO: implement unary negation in the shunting yard algorithm 


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
                        while (operatorStack.Count() != 0 && ShouldPopOperatorFromStack(token, operatorStack.Peek()))
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

                                if (operatorsRemain && result.TokenType == EquationTokenType.LeftParenthesis)
                                {
                                    // Pop left parenthesis
                                    operatorStack.Pop();
                                    break;
                                }

                                if (!operatorsRemain)
                                {
                                    throw new Exception("No matching left parenthesis found for right parenthesis.");
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
            isValid = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n{e.StackTrace}");
        }
    }

    static bool ShouldPopOperatorFromStack(EquationToken o1, EquationToken o2) =>
            o2.TokenType != EquationTokenType.LeftParenthesis
            && (o2.ComparePrecedence(o1) == 1 || (o2.ComparePrecedence(o1) == 0 && o1.IsLeftAssociative()));

    public readonly double Evaluate(params (string, double)[] variables)
    {
        if (!isValid)
        {
            throw new Exception("Equation is not properly initialized");
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
}

[Serializable]
public struct EquationToken
{
    private static Regex VariableNameRegex = new(@"^[a-zA-Z]+$");

    public EquationTokenType TokenType;
    public double Value;
    public string VariableName;

    public readonly TokenTypeGroup TypeGroup => TokenType switch
    {
        EquationTokenType.Constant or EquationTokenType.Variable => TokenTypeGroup.Value,
        EquationTokenType.LeftParenthesis or EquationTokenType.RightParenthesis => TokenTypeGroup.Parenthesis,
        _ => TokenTypeGroup.Operator
    };

    /// <summary>
    /// Compares two operator tokens in terms of operator precedence.
    /// </summary> 
    /// <returns>
    /// <para>-1 this operator is lower in precedence than the other one.</para>
    /// <para>0 this operator equals the other operator in precedence.</para>
    /// <para>1 this operator is higher in precedence than the other one.</para>
    /// </returns>
    public readonly int ComparePrecedence(EquationToken other)
    {
        if (TypeGroup != TokenTypeGroup.Operator)
            throw new ArgumentException("This token is not an operator but an attempt was made to compare it in precedence.");

        if (other.TypeGroup != TokenTypeGroup.Operator)
            throw new ArgumentException("The token this was compared to in precedence is not an operator.");

        int thisPrecedenceRank = (int)TokenType / 10;
        int otherPrecedenceRank = (int)other.TokenType / 10;

        return Math.Sign(thisPrecedenceRank - otherPrecedenceRank);
    }

    public readonly bool IsLeftAssociative()
    {
        if (TypeGroup != TokenTypeGroup.Operator)
            throw new ArgumentException("This token is not an operator but an attempt was made to check its associativity.");

        return (int)TokenType % 10 < 5;
    }

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
            case "(":
                TokenType = EquationTokenType.LeftParenthesis;
                break;
            case ")":
                TokenType = EquationTokenType.RightParenthesis;
                break;
            default:
                if (double.TryParse(tokenString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double value))
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
    Constant = -1,
    Variable = -2,
    LeftParenthesis = -3,
    RightParenthesis = -4,
    /*
    Lower number means higher precedence.
    Precedence is understood in groups of ten, therefore 10 and 11 count as the same precedence.

    Using modulo 10 you can determine the associativity of an operator. 
    If modulo 10 results in a number smaller than 5, then the operator is left associative, if not it is right associative.
    */
    UnaryNegation = 0, 
    Exponentiation = 15,
    Addition = 20,
    Subtraction = 21,
    Multiplication = 30,
    Division = 31,
}

[Serializable]
public enum TokenTypeGroup
{
    Value,
    Operator,
    Parenthesis,
}