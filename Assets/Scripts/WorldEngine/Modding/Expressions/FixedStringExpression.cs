using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedStringExpression : Expression
{
    public string Value;

    public FixedStringExpression(string value)
    {
        Value = value;
    }

    public override void Reset()
    {
    }

    public static FixedStringExpression ValidateExpression(Expression expression)
    {
        if (!(expression is FixedStringExpression stringExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid string expression");
        }

        return stringExpression;
    }
}
