using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NumericEqualsExpression : EqualsExpression
{
    protected NumericEqualsExpression(Expression expressionA, Expression expressionB) :
        base(expressionA, expressionB)
    {
    }

    public static Expression Build(NumericExpression expressionA, NumericExpression expressionB)
    {
        if ((expressionA is FixedNumberExpression) &&
            (expressionB is FixedNumberExpression))
        {
            FixedNumberExpression numExpA = expressionA as FixedNumberExpression;
            FixedNumberExpression numExpB = expressionB as FixedNumberExpression;

            return new FixedBooleanValueExpression(numExpA.NumberValue > numExpB.NumberValue);
        }

        return new NumericEqualsExpression(expressionA, expressionB);
    }

    protected override bool Evaluate()
    {
        return
            (ExpressionA as NumericExpression).GetValue()
            == (ExpressionB as NumericExpression).GetValue();
    }
}
