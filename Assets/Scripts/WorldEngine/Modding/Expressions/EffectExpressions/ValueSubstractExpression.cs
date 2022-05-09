using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ValueSubstractExpression : BinaryOpExpression, IEffectExpression
{
    private readonly IAssignableValueExpression<float> _targetValueExp;
    private readonly IValueExpression<float> _sourceValueExp;

    public ValueSubstractExpression(
        IExpression expressionA, IExpression expressionB)
        : base("=", expressionA, expressionB)
    {
        _targetValueExp =
            AssignableValueExpressionBuilder.ValidateAssignableValueExpression<float>(expressionA);
        _sourceValueExp =
            ValueExpressionBuilder.ValidateValueExpression<float>(expressionB);
    }

    public IEffectTrigger Trigger { get; set; }

    public void Apply()
    {
        _targetValueExp.Value -= _sourceValueExp.Value;
    }

    public override string ToPartiallyEvaluatedString(int depth)
    {
        return
            "(" + _expressionA.ToPartiallyEvaluatedString(0) +
            " -= " + _expressionB.ToPartiallyEvaluatedString(depth) + ")";
    }
}
