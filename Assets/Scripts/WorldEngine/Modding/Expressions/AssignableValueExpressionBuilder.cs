using System;
using System.Collections.Generic;

public static class AssignableValueExpressionBuilder
{
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
