using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MoreThanExpression : BinaryOpExpressionWithOutput<bool>
{
    private readonly IValueExpression<float> _numExpressionA;
    private readonly IValueExpression<float> _numExpressionB;

    public MoreThanExpression(
        IValueExpression<float> expressionA,
        IValueExpression<float> expressionB) :
        base(">", expressionA, expressionB)
    {
        _numExpressionA = expressionA;
        _numExpressionB = expressionB;
    }

    public static IExpression Build(Context context, string expressionAStr, string expressionBStr)
    {
        IValueExpression<float> expressionA =
            ValueExpressionBuilder.BuildValueExpression<float>(context, expressionAStr);
        IValueExpression<float> expressionB =
            ValueExpressionBuilder.BuildValueExpression<float>(context, expressionBStr);

        return new MoreThanExpression(expressionA, expressionB);
    }

    public override bool Value => _numExpressionA.Value > _numExpressionB.Value;
}
