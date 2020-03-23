using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class EqualsExpression<T> : BinaryOpExpression<bool> where T : IComparable<T>
{
    private readonly IValueExpression<T> _valueExpressionA;
    private readonly IValueExpression<T> _valueExpressionB;

    public EqualsExpression(IExpression expressionA, IExpression expressionB) :
        base("==", expressionA, expressionB)
    {
        _valueExpressionA =
            ExpressionBuilder.ValidateValueExpression<T>(expressionA);
        _valueExpressionB =
            ExpressionBuilder.ValidateValueExpression<T>(expressionB);
    }

    public override bool Value => _valueExpressionA.Value.Equals(_valueExpressionB.Value);
}
