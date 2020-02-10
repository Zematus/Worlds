using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class StringEqualsExpression : EqualsExpression
{
    protected StringEqualsExpression(Expression expressionA, Expression expressionB) :
        base(expressionA, expressionB)
    {
    }

    public static Expression Build(StringExpression expressionA, StringExpression expressionB)
    {
        if ((expressionA is FixedStringValueExpression) &&
            (expressionB is FixedStringValueExpression))
        {
            FixedStringValueExpression boolExpA = expressionA as FixedStringValueExpression;
            FixedStringValueExpression boolExpB = expressionB as FixedStringValueExpression;

            return new FixedBooleanValueExpression(boolExpA.StringValue == boolExpB.StringValue);
        }

        return new StringEqualsExpression(expressionA, expressionB);
    }

    protected override bool Evaluate()
    {
        return
            (ExpressionA as StringExpression).GetValue()
            == (ExpressionB as StringExpression).GetValue();
    }
}
