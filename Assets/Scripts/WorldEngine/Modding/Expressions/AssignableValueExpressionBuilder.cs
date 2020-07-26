using System;
using System.Collections.Generic;

public static class AssignableValueExpressionBuilder
{
    public static IAssignableValueExpression<T> BuildAssignableValueExpression<T>(
        Context context, string expressionStr)
    {
        return ValidateAssignableValueExpression<T>(
            ExpressionBuilder.BuildExpression(context, expressionStr));
    }

    public static IAssignableValueExpression<T> ValidateAssignableValueExpression<T>(
        IExpression expression)
    {
        if (!(expression is IAssignableValueExpression<T> valExpression))
        {
            throw new ArgumentException(
                expression + " is not a valid assignable " +
                GetModValueTypeString(typeof(T)) + " expression");
        }

        return valExpression;
    }

    public static IAssignableValueExpression<T>[] BuildAssignableValueExpressions<T>(
        Context context, ICollection<string> expressionStrs)
    {
        IAssignableValueExpression<T>[] expressions =
            new IAssignableValueExpression<T>[expressionStrs.Count];

        int i = 0;
        foreach (string expStr in expressionStrs)
        {
            expressions[i++] = BuildAssignableValueExpression<T>(context, expStr);
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

        throw new Exception("Internal: Unexpected type " + type);
    }
}
