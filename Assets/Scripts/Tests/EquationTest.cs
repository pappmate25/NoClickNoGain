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
        var equation = new Equation("5/0");
        Assert.Throws<DivideByZeroException>(() => equation.Evaluate());
    }

    [Test]
    public void InvalidEquation_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Equation("invalid"));
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
        var equationResult = equation.Evaluate(("x", 2), ("y", 4), ("z", 2));

        Debug.Log($"(-x+y)*z^2={equationResult}");

        Assert.AreEqual(8, equationResult);
    }
}
