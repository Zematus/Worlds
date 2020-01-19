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

        if ((expressionA is FixedNumberValueExpression) &&
            (expressionB is FixedNumberValueExpression))
        {
            FixedNumberValueExpression numExpA = expressionA as FixedNumberValueExpression;
            FixedNumberValueExpression numExpB = expressionB as FixedNumberValueExpression;

            numExpA.NumberValue += numExpB.NumberValue;

            return numExpA;
        }

        return new SumExpression(expressionA, expressionB);
    }

    public override float Evaluate()
    {
        return ExpressionA.Evaluate() + ExpressionB.Evaluate();
    }

    public override string ToString()
    {
        return "(" + ExpressionA.ToString() + ") + (" + ExpressionB.ToString() + ")";
    }
}
