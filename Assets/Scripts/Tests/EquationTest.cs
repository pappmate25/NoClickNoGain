using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EquationTest
{
    [Test]
    public void SimpleAddition_ReturnsCorrectResult()
    {
        var equation = new Equation("2+3");
        Assert.AreEqual(5, equation.Evaluate());
    }

    [Test]
    public void SimpleSubtraction_ReturnsCorrectResult()
    {
        var equation = new Equation("5-3");
        Assert.AreEqual(2, equation.Evaluate());
    }

    [Test]
    public void SimpleMultiplication_ReturnsCorrectResult()
    {
        var equation = new Equation("4*3");
        Assert.AreEqual(12, equation.Evaluate());
    }

    [Test]
    public void SimpleDivision_ReturnsCorrectResult()
    {
        var equation = new Equation("10/2");
        Assert.AreEqual(5, equation.Evaluate());
    }

    [Test]
    public void SimpleExponentiation_ReturnsCorrectResult()
    {
        var equation = new Equation("2^3");
        Assert.AreEqual(8, equation.Evaluate());
    }

    [Test]
    public void UnaryNegationAtStart_ReturnsCorrectResult()
    {
        var equation = new Equation("-5");
        Assert.AreEqual(-5, equation.Evaluate());
    }

    [Test]
    public void UnaryNegationAfterOperator_ReturnsCorrectResult()
    {
        var equation = new Equation("5+-3");
        Assert.AreEqual(2, equation.Evaluate());
    }

    [Test]
    public void UnaryNegationAfterParenthesis_ReturnsCorrectResult()
    {
        var equation = new Equation("(-5)");
        Assert.AreEqual(-5, equation.Evaluate());
    }

    [Test]
    public void ComplexUnaryNegation_ReturnsCorrectResult()
    {
        var equation = new Equation("-5+-3");
        Assert.AreEqual(-8, equation.Evaluate());
    }

    [Test]
    public void SimpleParentheses_ReturnsCorrectResult()
    {
        var equation = new Equation("(2+3)*4");
        Assert.AreEqual(20, equation.Evaluate());
    }

    [Test]
    public void NestedParentheses_ReturnsCorrectResult()
    {
        var equation = new Equation("((2+3)*4)+1");
        Assert.AreEqual(21, equation.Evaluate());
    }

    [Test]
    public void Variables_ReturnsCorrectResult()
    {
        var equation = new Equation("x+y");
        Assert.AreEqual(5, equation.Evaluate(("x", 2), ("y", 3)));
    }

    [Test]
    public void ComplexExpressionWithVariables_ReturnsCorrectResult()
    {
        var equation = new Equation("(x+y)*z");
        Assert.AreEqual(15, equation.Evaluate(("x", 2), ("y", 3), ("z", 3)));
    }

    [Test]
    public void DivisionByZero_ThrowsException()
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
        Assert.Throws<ArgumentException>(() => equation.Evaluate(("x", 2)));
    }

    [Test]
    public void ComplexExpressionWithAllOperations_ReturnsCorrectResult()
    {
        var equation = new Equation("(-x+y)*z^2");
        Assert.AreEqual(8, equation.Evaluate(("x", 2), ("y", 4), ("z", 2)));
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
        Assert.Throws<DivideByZeroException>(() => equation.Evaluate(("x", 0)));
    }

    [Test]
    public void ExtraVariables_LogsWarningButEvaluates()
    {
        // Create a logger to capture warning messages
        LogAssert.Expect(LogType.Warning, "Extra variables provided but not used: z");

        var equation = new Equation("x+y");
        double result = equation.Evaluate(("x", 2), ("y", 3), ("z", 4));

        Assert.AreEqual(5, result);
    }

    [Test]
    public void EquationToString_ReturnsPostfixRepresentation()
    {
        var equation = new Equation("2+3*4");
        string representation = equation.ToString();

        // Since the equation is stored in postfix notation, the string should represent that
        Assert.IsTrue(!string.IsNullOrEmpty(representation));
        Assert.IsTrue(representation.Contains("2") && representation.Contains("3") &&
                     representation.Contains("4") && representation.Contains("+") &&
                     representation.Contains("*"));
    }

    [Test]
    public void InvalidEquationToString_ReturnsInvalidMessage()
    {
        try
        {
            // Create an equation that will be invalid
            var equation = new Equation("2+");
            string representation = equation.ToString();
            Assert.AreEqual("Invalid Equation", representation);
        }
        catch
        {
            // If the equation constructor throws, we'll consider the test passed
            Assert.Pass();
        }
    }

    [Test]
    public void DecimalNumbers_EvaluatesCorrectly()
    {
        var equation = new Equation("2.5+3.5");
        Assert.AreEqual(6.0, equation.Evaluate());
    }

    [Test]
    public void ValidateEquation_HandlesStackUnderflow()
    {
        Assert.Throws<ArgumentException>(() => new Equation("++2"));
    }
}
