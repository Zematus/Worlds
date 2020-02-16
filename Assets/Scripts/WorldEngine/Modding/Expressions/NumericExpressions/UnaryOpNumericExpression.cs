using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryOpNumericExpression : UnaryOpExpression, INumericExpression
{
    protected INumericExpression _numExpression;

    public UnaryOpNumericExpression(string opStr, IExpression expression) :
        base(opStr, expression)
    {
        _numExpression = ExpressionBuilder.ValidateNumericExpression(expression);
    }

    public abstract float Value { get; }
}
