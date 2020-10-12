using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public static class NotEqualsExpressionBuilder
{
    public static IExpression BuildNotEqualsExpression(
        Context context, string expressionAStr, string expressionBStr)
    {
        IBaseValueExpression expressionA =
            ValueExpressionBuilder.BuildValueExpression(context, expressionAStr);
        IBaseValueExpression expressionB =
            ValueExpressionBuilder.BuildValueExpression(context, expressionBStr);

        if ((expressionA is IValueExpression<float>) &&
            (expressionB is IValueExpression<float>))
        {
            return new NotEqualsExpression<float>(expressionA, expressionB);
        }

        if ((expressionA is IValueExpression<bool>) &&
            (expressionB is IValueExpression<bool>))
        {
            return new NotEqualsExpression<bool>(expressionA, expressionB);
        }

        if ((expressionA is IValueExpression<string>) &&
            (expressionB is IValueExpression<string>))
        {
            return new NotEqualsExpression<string>(expressionA, expressionB);
        }

        if ((expressionA is IValueExpression<IEntity>) &&
            (expressionB is IValueExpression<IEntity>))
        {
            return new NotEqualsExpression<IEntity>(expressionA, expressionB);
        }

        throw new System.Exception(
            "Unhandled 'not equals' expression type combination: (" +
            expressionA.GetType() + ", " +
            expressionB.GetType() + "), original: " +
            expressionAStr + " != " + expressionBStr);
    }
}
