using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SumExpression : BinaryOpNumericExpression
{
    public SumExpression(Expression expressionA, Expression expressionB) : base(expressionA, expressionB)
    {
    }

    public static Expression Build(Context context, string expressionAStr, string expressionBStr)
    {
        Expression expressionA = BuildExpression(context, expressionAStr);
        Expression expressionB = BuildExpression(context, expressionBStr);

        if ((expressionA is FixedNumberExpression) &&
            (expressionB is FixedNumberExpression))
        {
            FixedNumberExpression numExpA = expressionA as FixedNumberExpression;
            FixedNumberExpression numExpB = expressionB as FixedNumberExpression;

            numExpA.NumberValue += numExpB.NumberValue;

            return numExpA;
        }

        return new SumExpression(expressionA, expressionB);
    }

    protected override float Evaluate()
    {
        return ExpressionA.GetValue() + ExpressionB.GetValue();
    }

    public override string ToString()
    {
        return ExpressionA.ToString() + " + " + ExpressionB.ToString();
    }
}
