using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryOpExpression : IExpression
{
    protected IExpression _expression;

    private string _opStr;

    public UnaryOpExpression(string opStr, IExpression expression)
    {
        _opStr = opStr;
        _expression = expression;
    }

    public override string ToString()
    {
        return "(" + _opStr + _expression + ")";
    }
}
