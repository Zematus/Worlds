using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class StringEqualsExpression : EqualsExpression
{
    protected StringEqualsExpression(IExpression expressionA, IExpression expressionB) :
        base(expressionA, expressionB)
    {
    }

    public static Expression Build(IStringExpression expressionA, IStringExpression expressionB)
    {
        if ((expressionA is FixedStringValueExpression) &&
            (expressionB is FixedStringValueExpression))
        {
            FixedStringValueExpression boolExpA = expressionA as FixedStringValueExpression;
            FixedStringValueExpression boolExpB = expressionB as FixedStringValueExpression;

            return new FixedBooleanValueExpression(boolExpA.Value == boolExpB.Value);
        }

        return new StringEqualsExpression(expressionA, expressionB);
    }

    protected override bool Evaluate()
    {
        return
            (ExpressionA as IStringExpression).Value
            == (ExpressionB as IStringExpression).Value;
    }
}
