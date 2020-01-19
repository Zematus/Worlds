using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NegateBooleanValueExpression : UnaryOpBooleanExpression
{
    public NegateBooleanValueExpression(Expression expression) : base(expression)
    {
    }

    public static Expression Build(Context context, string expressionStr)
    {
        Expression expression = BuildExpression(context, expressionStr);

        if (expression is FixedBooleanValueExpression)
        {
            FixedBooleanValueExpression boolExp = expression as FixedBooleanValueExpression;

            boolExp.BooleanValue = !boolExp.BooleanValue;

            return boolExp;
        }

        return new NegateBooleanValueExpression(expression);
    }

    public override bool Evaluate()
    {
        return !Expression.Evaluate();
    }

    public override string ToString()
    {
        return "!(" + Expression.ToString() + ")";
    }
}
