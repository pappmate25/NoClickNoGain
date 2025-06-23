
using System;
using System.Globalization;
using System.Text.RegularExpressions;

[Serializable]
public struct EquationToken
{
    private static readonly Regex VariableNameRegex = new(@"^[a-zA-Z]+$");

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
    /// <list type="bullet">
    /// <item>
    /// -1 if this operator is lower in precedence than the other one.
    /// </item>
    /// <item>
    /// 0 if this operator equals the other operator in precedence.
    /// </item>
    /// <item>
    /// 1 if this operator is higher in precedence than the other one.
    /// </item> 
    /// </list>
    /// </returns>
    public readonly int ComparePrecedence(EquationToken other)
    {
        if (TypeGroup != TokenTypeGroup.Operator)
            throw new ArgumentException("This token is not an operator but an attempt was made to compare it in precedence.");

        if (other.TypeGroup != TokenTypeGroup.Operator)
            throw new ArgumentException("The token this was compared to in precedence is not an operator.");

        int thisPrecedenceRank = (int)TokenType / 10;
        int otherPrecedenceRank = (int)other.TokenType / 10;

        return Math.Sign(otherPrecedenceRank - thisPrecedenceRank);
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
    Precedence is understood in groups of ten, so for example 10..19 count as the same precedence.

    Using modulo 10 you can determine the associativity of an operator. 
    If modulo 10 results in a number smaller than 5, then the operator is left associative, if not then it is right associative.
    */
    UnaryNegation = 0,
    Exponentiation = 15,
    Multiplication = 20,
    Division = 21,
    Addition = 30,
    Subtraction = 31,
}

[Serializable]
public enum TokenTypeGroup
{
    Value,
    Operator,
    Parenthesis,
}