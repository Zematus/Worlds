using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public static class EqualsExpressionBuilder
{
    public static IExpression BuildEqualsExpression(
        Context context, string expressionAStr, string expressionBStr)
    {
        IExpression expressionA = ExpressionBuilder.BuildExpression(context, expressionAStr);
        IExpression expressionB = ExpressionBuilder.BuildExpression(context, expressionBStr);

        if ((expressionA is FixedValueExpression<float> nExpA) &&
            (expressionB is FixedValueExpression<float> nExpB))
        {
            return new FixedBooleanValueExpression(nExpA.FixedValue == nExpB.FixedValue);
        }

        if ((expressionA is FixedValueExpression<bool> bExpA) &&
            (expressionB is FixedValueExpression<bool> bExpB))
        {
            return new FixedBooleanValueExpression(bExpA.FixedValue == bExpB.FixedValue);
        }

        if ((expressionA is FixedValueExpression<string> sExpA) &&
            (expressionB is FixedValueExpression<string> sExpB))
        {
            return new FixedBooleanValueExpression(sExpA.FixedValue == sExpB.FixedValue);
        }

        if ((expressionA is IValueExpression<float>) &&
            (expressionB is IValueExpression<float>))
        {
            return new EqualsExpression<float>(expressionA, expressionB);
        }

        if ((expressionA is IValueExpression<bool>) &&
            (expressionB is IValueExpression<bool>))
        {
            return new EqualsExpression<bool>(expressionA, expressionB);
        }

        if ((expressionA is IValueExpression<string>) &&
            (expressionB is IValueExpression<string>))
        {
            return new EqualsExpression<string>(expressionA, expressionB);
        }

        if ((expressionA is IValueExpression<Entity>) &&
            (expressionB is IValueExpression<Entity>))
        {
            return new EqualsExpression<Entity>(expressionA, expressionB);
        }

        throw new System.Exception(
            "Unhandled binary op expression type combination: (" +
            expressionA.GetType() + ", " + expressionB.GetType() + ")");
    }
}
