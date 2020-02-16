using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EqualsExpression : BinaryOpBooleanExpression
{
    protected EqualsExpression(IExpression expressionA, IExpression expressionB) :
        base("==", expressionA, expressionB)
    {
    }

    public static IExpression Build(Context context, string expressionAStr, string expressionBStr)
    {
        IExpression expressionA = ExpressionBuilder.BuildExpression(context, expressionAStr);
        IExpression expressionB = ExpressionBuilder.BuildExpression(context, expressionBStr);

        if ((expressionA is INumericExpression) &&
            (expressionB is INumericExpression))
        {
            return NumericEqualsExpression.Build(
                expressionA as INumericExpression, expressionB as INumericExpression);
        }

        if ((expressionA is IBooleanExpression) &&
            (expressionB is IBooleanExpression))
        {
            return BooleanEqualsExpression.Build(
                expressionA as IBooleanExpression, expressionB as IBooleanExpression);
        }

        if ((expressionA is IStringExpression) &&
            (expressionB is IStringExpression))
        {
            return StringEqualsExpression.Build(
                expressionA as IStringExpression, expressionB as IStringExpression);
        }

        throw new System.ArgumentException(
            "Unable to compare '" + expressionA + "' with '"
            + expressionB + "': expression type mismatch...");
    }
}
