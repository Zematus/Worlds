using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public static class ValueAssignmentExpressionBuilder
{
    public static IExpression BuildValueAssignmentExpression(
        Context context,
        string expressionAStr,
        string expressionBStr,
        bool allowInputRequesters = false)
    {
        IExpression expressionA =
            ExpressionBuilder.BuildExpression(
                context, expressionAStr, allowInputRequesters);
        IBaseValueExpression expressionB =
            ValueExpressionBuilder.BuildValueExpression(
                context, expressionBStr, allowInputRequesters);

        if ((expressionA is IAssignableValueExpression<float>) &&
            (expressionB is IValueExpression<float>))
        {
            return new ValueAssignmentExpression<float>(expressionA, expressionB);
        }

        if ((expressionA is IAssignableValueExpression<bool>) &&
            (expressionB is IValueExpression<bool>))
        {
            return new ValueAssignmentExpression<bool>(expressionA, expressionB);
        }

        if ((expressionA is IAssignableValueExpression<string>) &&
            (expressionB is IValueExpression<string>))
        {
            return new ValueAssignmentExpression<string>(expressionA, expressionB);
        }

        if ((expressionA is IAssignableValueExpression<Entity>) &&
            (expressionB is IValueExpression<Entity>))
        {
            return new ValueAssignmentExpression<Entity>(expressionA, expressionB);
        }

        throw new System.Exception(
            "Unhandled value assignment expression type combination: (" +
            expressionA.GetType() + ", " +
            expressionB.GetType() + "), original: " +
            expressionAStr + " = " + expressionBStr);
    }

    public static IExpression BuildValueAddExpression(
        Context context,
        string expressionAStr,
        string expressionBStr,
        bool allowInputRequesters = false)
    {
        IExpression expressionA =
            ExpressionBuilder.BuildExpression(
                context, expressionAStr, allowInputRequesters);
        IBaseValueExpression expressionB =
            ValueExpressionBuilder.BuildValueExpression(
                context, expressionBStr, allowInputRequesters);

        if ((expressionA is IAssignableValueExpression<float>) &&
            (expressionB is IValueExpression<float>))
        {
            return new ValueAddExpression(expressionA, expressionB);
        }

        throw new System.Exception(
            "Unhandled value assignment expression type combination: (" +
            expressionA.GetType() + ", " +
            expressionB.GetType() + "), original: " +
            expressionAStr + " += " + expressionBStr);
    }

    public static IExpression BuildValueSubstractExpression(
        Context context,
        string expressionAStr,
        string expressionBStr,
        bool allowInputRequesters = false)
    {
        IExpression expressionA =
            ExpressionBuilder.BuildExpression(
                context, expressionAStr, allowInputRequesters);
        IBaseValueExpression expressionB =
            ValueExpressionBuilder.BuildValueExpression(
                context, expressionBStr, allowInputRequesters);

        if ((expressionA is IAssignableValueExpression<float>) &&
            (expressionB is IValueExpression<float>))
        {
            return new ValueSubstractExpression(expressionA, expressionB);
        }

        throw new System.Exception(
            "Unhandled value assignment expression type combination: (" +
            expressionA.GetType() + ", " +
            expressionB.GetType() + "), original: " +
            expressionAStr + " -= " + expressionBStr);
    }
}
