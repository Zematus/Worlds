﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MoreThanOrEqualExpression : BinaryOpBooleanExpression
{
    private readonly INumericExpression _numExpressionA;
    private readonly INumericExpression _numExpressionB;

    public MoreThanOrEqualExpression(IExpression expressionA, IExpression expressionB) :
        base(">=", expressionA, expressionB)
    {
        _numExpressionA = ExpressionBuilder.ValidateNumericExpression(expressionA);
        _numExpressionB = ExpressionBuilder.ValidateNumericExpression(expressionB);
    }

    public static IExpression Build(Context context, string expressionAStr, string expressionBStr)
    {
        INumericExpression expressionA =
            ExpressionBuilder.ValidateNumericExpression(
                ExpressionBuilder.BuildExpression(context, expressionAStr));
        INumericExpression expressionB =
            ExpressionBuilder.ValidateNumericExpression(
                ExpressionBuilder.BuildExpression(context, expressionBStr));

        if ((expressionA is FixedNumberExpression) &&
            (expressionB is FixedNumberExpression))
        {
            FixedNumberExpression numExpA = expressionA as FixedNumberExpression;
            FixedNumberExpression numExpB = expressionB as FixedNumberExpression;

            return new FixedBooleanValueExpression(numExpA.NumberValue >= numExpB.NumberValue);
        }

        return new MoreThanOrEqualExpression(expressionA, expressionB);
    }

    public override bool Value => _numExpressionA.Value >= _numExpressionB.Value;
}
