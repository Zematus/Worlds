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

    public static IExpression Build(Context context, string expressionAStr, string expressionBStr)
    {
        IValueExpression<float> expressionA =
            AssignableValueExpressionBuilder.BuildAssignableValueExpression<float>(context, expressionAStr);
        IValueExpression<float> expressionB =
            ValueExpressionBuilder.BuildValueExpression<float>(context, expressionBStr);

        return new ValueAssignmentExpression<T>(expressionA, expressionB);
    }

    public void Apply()
    {
        _targetValueExp.Value = _sourceValueExp.Value;
    }

    public override string ToPartiallyEvaluatedString(bool evaluate)
    {
        return
            "(" + _expressionA.ToPartiallyEvaluatedString(false) +
            " = " + _expressionB.ToPartiallyEvaluatedString(evaluate) + ")";
    }
}
