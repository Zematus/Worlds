using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EqualsExpression : BinaryOpBooleanExpression
{
    protected EqualsExpression(Expression expressionA, Expression expressionB) :
        base(expressionA, expressionB)
    {
    }

    public static Expression Build(Context context, string expressionAStr, string expressionBStr)
    {
        Expression expressionA = BuildExpression(context, expressionAStr);
        Expression expressionB = BuildExpression(context, expressionBStr);

        if ((expressionA is NumericExpression) &&
            (expressionB is NumericExpression))
        {
            return NumericEqualsExpression.Build(
                expressionA as NumericExpression, expressionB as NumericExpression);
        }

        if ((expressionA is BooleanExpression) &&
            (expressionB is BooleanExpression))
        {
            return BooleanEqualsExpression.Build(
                expressionA as BooleanExpression, expressionB as BooleanExpression);
        }

        if ((expressionA is StringExpression) &&
            (expressionB is StringExpression))
        {
            return StringEqualsExpression.Build(
                expressionA as StringExpression, expressionB as StringExpression);
        }

        throw new System.ArgumentException(
            "Unable to compare '" + expressionA.ToString() + "' with '"
            + expressionB.ToString() + "': expression type mismatch...");
    }

    public override string ToString()
    {
        return ExpressionA.ToString() + " == " + ExpressionB.ToString();
    }
}
