using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SumExpression : BinaryOpNumericExpression
{
    public SumExpression(string expressionAStr, string expressionBStr) : base(expressionAStr, expressionBStr)
    {
    }

    public SumExpression(Expression expressionA, Expression expressionB) : base(expressionA, expressionB)
    {
    }

    public static Expression Build(string expressionAStr, string expressionBStr)
    {
        Expression expressionA = BuildExpression(expressionAStr);
        Expression expressionB = BuildExpression(expressionBStr);

        if ((expressionA is NumberValueExpression) &&
            (expressionB is NumberValueExpression))
        {
            NumberValueExpression numExpA = expressionA as NumberValueExpression;
            NumberValueExpression numExpB = expressionB as NumberValueExpression;

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
