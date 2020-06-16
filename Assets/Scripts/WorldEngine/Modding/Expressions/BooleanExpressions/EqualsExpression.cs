using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class EqualsExpression<T> : BinaryOpExpressionWithOutput<bool> where T : IComparable<T>
{
    private readonly IValueExpression<T> _valueExpressionA;
    private readonly IValueExpression<T> _valueExpressionB;

    public EqualsExpression(IExpression expressionA, IExpression expressionB) :
        base("==", expressionA, expressionB)
    {
        _valueExpressionA =
            ValueExpressionBuilder.ValidateValueExpression<T>(expressionA);
        _valueExpressionB =
            ValueExpressionBuilder.ValidateValueExpression<T>(expressionB);
    }

    public override bool Value => _valueExpressionA.Value.Equals(_valueExpressionB.Value);
}
