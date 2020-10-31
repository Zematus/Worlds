using System;
using System.Collections.Generic;

public static class ValueExpressionBuilder
{
    public static IValueExpression<T> BuildValueExpression<T>(Context context, string expressionStr)
    {
        return ValidateValueExpression<T>(ExpressionBuilder.BuildExpression(context, expressionStr));
    }

    public static IBaseValueExpression BuildValueExpression(Context context, string expressionStr)
    {
        return ValidateValueExpression(ExpressionBuilder.BuildExpression(context, expressionStr));
    }

    public static IBaseValueExpression ValidateValueExpression(IExpression expression)
    {
        if (expression is BaseValueEntityExpression vEntityExp)
        {
            return vEntityExp.BaseValueEntity.BaseValueExpression;
        }

        if (!(expression is IBaseValueExpression valExpression))
        {
            throw new ArgumentException(
                expression + " is not a valid value expression");
        }

        return valExpression;
    }

    public static IValueExpression<T> ValidateValueExpression<T>(IExpression expression)
    {
        if (expression is ValueEntityAttributeExpression<IValueEntity<T>> vEntityAttrExp)
        {
            return vEntityAttrExp.Value.ValueExpression;
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
        Context context, ICollection<string> expressionStrs)
    {
        IValueExpression<T>[] expressions = new IValueExpression<T>[expressionStrs.Count];

        int i = 0;
        foreach (string expStr in expressionStrs)
        {
            expressions[i++] = BuildValueExpression<T>(context, expStr);
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
