using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public struct Equation
{
    public static Regex EquationRegex = new Regex(@"[+\-\/*^\(\)]|\d+|\w+");

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

            // Shunting yard algorithm
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
                        /*
                        - an operator o1:
                            while (
                                there is an operator o2 at the top of the operator stack which is not a left parenthesis, 
                                and (o2 has greater precedence than o1 or (o1 and o2 have the same precedence and o1 is left-associative))
                            ): 
                                pop o2 from the operator stack into the output queue
                            push o1 onto the operator stack
                        */

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
            Debug.LogError($"{e.StackTrace}");
        }
    }

    static bool ShouldPopOperatorFromStack(EquationToken o1, EquationToken o2) =>
            o2.TokenType != EquationTokenType.LeftParenthesis
            && (o2.ComparePrecedence(o1) == 1 || (o2.ComparePrecedence(o1) == 0 && o1.IsLeftAssociative()));

    public double Evaluate(params (string, double)[] variables)
    {
        if (!isValid)
        {
            throw new Exception("Equation is not properly initialized");
        }

        checkVariables(variables);

        List<double> variableValues = new();

        return 1;
    }

    private readonly void checkVariables((string, double)[] variables)
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
    Exponentiation = 5,
    Addition = 10,
    Subtraction = 11,
    Multiplication = 20,
    Division = 21,
}

[Serializable]
public enum TokenTypeGroup
{
    Value,
    Operator,
    Parenthesis,
}