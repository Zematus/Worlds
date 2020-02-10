using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BooleanEqualsExpression : EqualsExpression
{
    protected BooleanEqualsExpression(BooleanExpression expressionA, BooleanExpression expressionB) :
        base(expressionA, expressionB)
    {
    }

    public static Expression Build(BooleanExpression expressionA, BooleanExpression expressionB)
    {
        if ((expressionA is FixedBooleanValueExpression) &&
            (expressionB is FixedBooleanValueExpression))
        {
            FixedBooleanValueExpression boolExpA = expressionA as FixedBooleanValueExpression;
            FixedBooleanValueExpression boolExpB = expressionB as FixedBooleanValueExpression;

            return new FixedBooleanValueExpression(boolExpA.BooleanValue == boolExpB.BooleanValue);
        }

        return new BooleanEqualsExpression(expressionA, expressionB);
    }

    protected override bool Evaluate()
    {
        return
            (ExpressionA as BooleanExpression).GetValue()
            == (ExpressionB as BooleanExpression).GetValue();
    }
}
