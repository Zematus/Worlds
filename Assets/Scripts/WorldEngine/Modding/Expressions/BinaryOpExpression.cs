using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BinaryOpExpression : IExpression
{
    protected IExpression _expressionA;
    protected IExpression _expressionB;

    private string _opStr;

    public BinaryOpExpression(string opStr, IExpression expressionA, IExpression expressionB)
    {
        _opStr = opStr;
        _expressionA = expressionA;
        _expressionB = expressionB;
    }

    public override string ToString()
    {
        return "(" + _expressionA + " " + _opStr + " " + _expressionB + ")";
    }
}
