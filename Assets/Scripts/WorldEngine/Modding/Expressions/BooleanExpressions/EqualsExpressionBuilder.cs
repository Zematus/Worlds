﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public static class EqualsExpressionBuilder
{
    public static IExpression BuildEqualsExpression(
        Context context,
        string expressionAStr,
        string expressionBStr,
        bool allowInputRequesters = false)
    {
        IBaseValueExpression expressionA =
            ValueExpressionBuilder.BuildValueExpression(
                context, expressionAStr, allowInputRequesters);
        IBaseValueExpression expressionB =
            ValueExpressionBuilder.BuildValueExpression(
                context, expressionBStr, allowInputRequesters);

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

        if ((expressionA is IValueExpression<IEntity>) &&
            (expressionB is IValueExpression<IEntity>))
        {
            return new EqualsExpression<IEntity>(expressionA, expressionB);
        }

        throw new System.Exception(
            "Unhandled 'equals' expression type combination: (" +
            expressionA.GetType() + ", " +
            expressionB.GetType() + "), original: " +
            expressionAStr + " == " + expressionBStr);
    }
}
