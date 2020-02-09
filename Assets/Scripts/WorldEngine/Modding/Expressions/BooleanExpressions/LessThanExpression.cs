using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LessThanExpression : BinaryOpBooleanExpression
{
    public LessThanExpression(Expression expressionA, Expression expressionB) :
        base(expressionA, expressionB)
    {
    }

    public static Expression Build(Context context, string expressionAStr, string expressionBStr)
    {
        NumericExpression expressionA =
            NumericExpression.ValidateExpression(BuildExpression(context, expressionAStr));
        NumericExpression expressionB =
            NumericExpression.ValidateExpression(BuildExpression(context, expressionBStr));

        if ((expressionA is FixedNumberExpression) &&
            (expressionB is FixedNumberExpression))
        {
            FixedNumberExpression numExpA = expressionA as FixedNumberExpression;
            FixedNumberExpression numExpB = expressionB as FixedNumberExpression;

            return new FixedBooleanValueExpression(numExpA.NumberValue < numExpB.NumberValue);
        }

        return new LessThanExpression(expressionA, expressionB);
    }

    protected override bool Evaluate()
    {
        return (ExpressionA as NumericExpression).GetValue() < (ExpressionB as NumericExpression).GetValue();
    }

    public override string ToString()
    {
        return ExpressionA.ToString() + " < " + ExpressionB.ToString();
    }
}
