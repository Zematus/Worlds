using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ValueAssignmentExpression<T> : BinaryOpExpression, IEffectExpression
{
    private readonly IAssignableValueExpression<T> _targetValueExp;
    private readonly IValueExpression<T> _sourceValueExp;

    public ValueAssignmentExpression(
        IExpression expressionA, IExpression expressionB)
        : base("=", expressionA, expressionB)
    {
        _targetValueExp =
            AssignableValueExpressionBuilder.ValidateAssignableValueExpression<T>(expressionA);
        _sourceValueExp =
            ValueExpressionBuilder.ValidateValueExpression<T>(expressionB);
    }

    public void Apply()
    {
        _targetValueExp.Value = _sourceValueExp.Value;
    }
}
