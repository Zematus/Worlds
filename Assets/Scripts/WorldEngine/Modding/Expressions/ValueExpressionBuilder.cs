using System;
using System.Collections.Generic;
using UnityEngine;

public static class ValueExpressionBuilder
{
    public static IValueExpression<T> BuildValueExpression<T>(
        Context context, string expressionStr, bool allowInputRequesters = false)
    {
        return ValidateValueExpression<T>(
            ExpressionBuilder.BuildExpression(context, expressionStr, allowInputRequesters));
    }

    public static IBaseValueExpression BuildValueExpression(
        Context context, string expressionStr, bool allowInputRequesters = false)
    {
        return ValidateValueExpression(
            ExpressionBuilder.BuildExpression(context, expressionStr, allowInputRequesters));
    }

    public static IBaseValueExpression ValidateValueExpression(IExpression expression)
    {
        if (expression is BaseValueEntityExpression vEntityExp)
        {
            return vEntityExp.BaseValueEntity.BaseValueExpression;
        }

        if (expression is IBaseValueExpression valExpression)
        {
            return valExpression;
        }

        throw new ArgumentException(
            expression + " is not a valid value expression");
    }

    public static IValueExpression<T> ValidateValueExpression<T>(IExpression expression)
    {
        if (expression is ValueEntityAttributeExpression<IEntity> entityAttrExp)
        {
            if (entityAttrExp.Value is IValueEntity<T> vEntity)
            {
                return vEntity.ValueExpression;
            }
        }

        if (expression is ValueEntityExpression<T> vEntityExp)
        {
            return vEntityExp.ValueEntity.ValueExpression;
        }

        if (expression is IValueExpression<T> valExpression)
        {
            return valExpression;
        }

        throw new ArgumentException(
            expression + " is not a valid " +
            GetModValueTypeString(typeof(T)) + " expression");
    }

    public static IValueExpression<T>[] BuildValueExpressions<T>(
        Context context, ICollection<string> expressionStrs, bool allowInputRequesters = false)
    {
        IValueExpression<T>[] expressions = new IValueExpression<T>[expressionStrs.Count];

        int i = 0;
        foreach (string expStr in expressionStrs)
        {
            IValueExpression<T> expression = BuildValueExpression<T>(context, expStr);

            expressions[i++] = expression;
        }

        return expressions;
    }

    private static string GetModValueTypeString(Type type)
    {
        if (type == typeof(string))
        {
            return "string";
        }

        if (type == typeof(bool))
        {
            return "boolean";
        }

        if (type == typeof(float))
        {
            return "number";
        }

        if (type == typeof(IEntity))
        {
            return "entity";
        }

        throw new Exception("Internal: Unexpected type " + type);
    }
}
