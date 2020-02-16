using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryOpBooleanExpression : UnaryOpExpression, IBooleanExpression
{
    protected IBooleanExpression _boolExpression;

    public UnaryOpBooleanExpression(string opStr, IExpression expression) :
        base(opStr, expression)
    {
        _boolExpression = ExpressionBuilder.ValidateBooleanExpression(expression);
    }

    public abstract bool Value { get; }
}
