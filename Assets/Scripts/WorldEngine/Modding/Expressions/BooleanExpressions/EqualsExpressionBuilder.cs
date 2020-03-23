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

        if ((expressionA is FixedNumberExpression) &&
            (expressionB is FixedNumberExpression))
        {
            FixedNumberExpression expA = expressionA as FixedNumberExpression;
            FixedNumberExpression expB = expressionB as FixedNumberExpression;

            return new FixedBooleanValueExpression(expA.NumberValue == expB.NumberValue);
        }

        if ((expressionA is FixedBooleanValueExpression) &&
            (expressionB is FixedBooleanValueExpression))
        {
            FixedBooleanValueExpression expA = expressionA as FixedBooleanValueExpression;
            FixedBooleanValueExpression expB = expressionB as FixedBooleanValueExpression;

            return new FixedBooleanValueExpression(expA.BooleanValue == expB.BooleanValue);
        }

        if ((expressionA is FixedStringValueExpression) &&
            (expressionB is FixedStringValueExpression))
        {
            FixedStringValueExpression expA = expressionA as FixedStringValueExpression;
            FixedStringValueExpression expB = expressionB as FixedStringValueExpression;

            return new FixedBooleanValueExpression(expA.Value == expB.Value);
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
