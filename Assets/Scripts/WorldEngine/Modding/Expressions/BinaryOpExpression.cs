using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Base class for all binary operation expressions (i.e. '2 + 2', 'A && B')
/// </summary>
public abstract class BinaryOpExpression : IExpression
{
    protected IExpression _expressionA;
    protected IExpression _expressionB;

    private readonly string _opStr;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="opStr">Operator string (i.e. '+', '-', '!', '*')</param>
    /// <param name="expressionA">First operand expression</param>
    /// <param name="expressionB">Second operand expression</param>
    public BinaryOpExpression(
        string opStr, IExpression expressionA, IExpression expressionB)
    {
        _opStr = opStr;
        _expressionA = expressionA;
        _expressionB = expressionB;
    }

    public override string ToString()
    {
        return "(" + _expressionA + " " + _opStr + " " + _expressionB + ")";
    }

    public virtual string ToPartiallyEvaluatedString(bool evaluate = true)
    {
        string expAPartial = _expressionA.ToPartiallyEvaluatedString(evaluate);
        string expBPartial = _expressionB.ToPartiallyEvaluatedString(evaluate);

        return "(" + expAPartial + " " + _opStr + " " + expBPartial + ")";
    }
}
