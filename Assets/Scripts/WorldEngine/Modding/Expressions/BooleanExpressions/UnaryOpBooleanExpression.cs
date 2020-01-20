using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryOpBooleanExpression : BooleanExpression
{
    public BooleanExpression Expression;

    public UnaryOpBooleanExpression(Context context, string expressionStr)
    {
        Expression = ValidateExpression(BuildExpression(context, expressionStr));
    }

    public UnaryOpBooleanExpression(Expression expression)
    {
        Expression = ValidateExpression(expression);
    }

    public override void ResetCache()
    {
        Expression.ResetCache();

        base.ResetCache();
    }
}
