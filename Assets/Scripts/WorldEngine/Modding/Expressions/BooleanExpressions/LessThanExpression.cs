using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LessThanExpression : BinaryOpExpression<bool>
{
    private readonly IValueExpression<float> _numExpressionA;
    private readonly IValueExpression<float> _numExpressionB;

    public LessThanExpression(
        IValueExpression<float> expressionA,
        IValueExpression<float> expressionB) :
        base("<", expressionA, expressionB)
    {
        _numExpressionA = expressionA;
        _numExpressionB = expressionB;
    }

    public static IExpression Build(Context context, string expressionAStr, string expressionBStr)
    {
        IValueExpression<float> expressionA =
            ExpressionBuilder.ValidateValueExpression<float>(
                ExpressionBuilder.BuildExpression(context, expressionAStr));
        IValueExpression<float> expressionB =
            ExpressionBuilder.ValidateValueExpression<float>(
                ExpressionBuilder.BuildExpression(context, expressionBStr));

        if ((expressionA is FixedNumberExpression) &&
            (expressionB is FixedNumberExpression))
        {
            FixedNumberExpression numExpA = expressionA as FixedNumberExpression;
            FixedNumberExpression numExpB = expressionB as FixedNumberExpression;

            return new FixedBooleanValueExpression(numExpA.NumberValue < numExpB.NumberValue);
        }

        return new LessThanExpression(expressionA, expressionB);
    }

    public override bool Value => _numExpressionA.Value < _numExpressionB.Value;
}
