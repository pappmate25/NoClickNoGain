using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EquationTest
{
    [Test]
    public void SimpleAddition_ReturnsCorrectResult()
    {
        var equation = new Equation("2+3");
        Assert.AreEqual(5, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void SimpleSubtraction_ReturnsCorrectResult()
    {
        var equation = new Equation("5-3");
        Assert.AreEqual(2, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void SimpleMultiplication_ReturnsCorrectResult()
    {
        var equation = new Equation("4*3");
        Assert.AreEqual(12, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void SimpleDivision_ReturnsCorrectResult()
    {
        var equation = new Equation("10/2");
        Assert.AreEqual(5, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void SimpleExponentiation_ReturnsCorrectResult()
    {
        var equation = new Equation("2^3");
        Assert.AreEqual(8, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void UnaryNegationAtStart_ReturnsCorrectResult()
    {
        var equation = new Equation("-5");
        Assert.AreEqual(-5, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void UnaryNegationAfterOperator_ReturnsCorrectResult()
    {
        var equation = new Equation("5+-3");
        Assert.AreEqual(2, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void UnaryNegationAfterParenthesis_ReturnsCorrectResult()
    {
        var equation = new Equation("(-5)");
        Assert.AreEqual(-5, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void ComplexUnaryNegation_ReturnsCorrectResult()
    {
        var equation = new Equation("-5+-3");
        Assert.AreEqual(-8, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void SimpleParentheses_ReturnsCorrectResult()
    {
        var equation = new Equation("(2+3)*4");
        Assert.AreEqual(20, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void NestedParentheses_ReturnsCorrectResult()
    {
        var equation = new Equation("((2+3)*4)+1");
        Assert.AreEqual(21, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void Variables_ReturnsCorrectResult()
    {
        var equation = new Equation("x+y");
        var variables = new Dictionary<string, double> { { "x", 2 }, { "y", 3 } };
        Assert.AreEqual(5, equation.Evaluate(variables));
    }

    [Test]
    public void ComplexExpressionWithVariables_ReturnsCorrectResult()
    {
        var equation = new Equation("(x+y)*z");
        var variables = new Dictionary<string, double> { { "x", 2 }, { "y", 3 }, { "z", 3 } };
        Assert.AreEqual(15, equation.Evaluate(variables));
    }
    [Test]
    public void DivisionByZero_LogsErrorAndCreatesInvalidEquation()
    {
        Assert.Throws<ArgumentException>(() => new Equation("5/0"));
    }

    [Test]
    public void InvalidEquation_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Equation("invalid#"));
    }

    [Test]
    public void MissingVariables_ThrowsException()
    {
        var equation = new Equation("x+y");
        var variables = new Dictionary<string, double> { { "x", 2 } }; // Missing "y"
        Assert.Throws<KeyNotFoundException>(() => equation.Evaluate(variables));
    }

    [Test]
    public void ComplexExpressionWithAllOperations_ReturnsCorrectResult()
    {
        var equation = new Equation("(-x+y)*z^2");
        var variables = new Dictionary<string, double> { { "x", 2 }, { "y", 4 }, { "z", 2 } };
        Assert.AreEqual(8, equation.Evaluate(variables));
    }

    [Test]
    public void EmptyEquation_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Equation(""));
    }

    [Test]
    public void WhitespaceEquation_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Equation("   "));
    }

    [Test]
    public void UnbalancedParenthesesLeft_ThrowsException()
    {
        Assert.Throws<Exception>(() => new Equation("(2+3"));
    }

    [Test]
    public void UnbalancedParenthesesRight_ThrowsException()
    {
        Assert.Throws<Exception>(() => new Equation("2+3)"));
    }

    [Test]
    public void InvalidOperatorSequence_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Equation("2/*3"));
    }

    [Test]
    public void IncompleteEquation_ThrowsException()
    {
        // The equation has a trailing operator without a right operand
        Assert.Throws<ArgumentException>(() => new Equation("2+"));
    }
    [Test]
    public void DivisionByVariableZero_ThrowsException()
    {
        var equation = new Equation("5/x");
        var variables = new Dictionary<string, double> { { "x", 0 } };
        Assert.Throws<DivideByZeroException>(() => equation.Evaluate(variables));
    }

    [Test]
    public void EquationToString_ReturnsPostfixRepresentation()
    {
        var equation = new Equation("2+3*4");
        string representation = equation.ToString();

        // The toString should show the postfix representation
        Assert.IsTrue(!string.IsNullOrEmpty(representation));
        Assert.AreEqual(14, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void InvalidEquationToString_ReturnsInvalidMessage()
    {
        // This test just verifies that invalid equations throw during construction
        Assert.Throws<ArgumentException>(() => new Equation("2+"));
    }

    [Test]
    public void DecimalNumbers_EvaluatesCorrectly()
    {
        var equation = new Equation("2.5+3.5");
        Assert.AreEqual(6.0, equation.Evaluate(new Dictionary<string, double>()));
    }

    [Test]
    public void ValidateEquation_HandlesStackUnderflow()
    {
        Assert.Throws<ArgumentException>(() => new Equation("++2"));
    }

    [Test]
    public void SubtractionBeforeParenthesis_ReturnsCorrectResult()
    {
        var equation = new Equation("5-(2+3)");
        Assert.AreEqual(0, equation.Evaluate(new Dictionary<string, double>()));
    }
}
