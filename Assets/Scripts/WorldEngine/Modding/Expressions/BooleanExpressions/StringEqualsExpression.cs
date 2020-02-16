using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class StringEqualsExpression : EqualsExpression
{
    private IStringExpression _strExpressionA;
    private IStringExpression _strExpressionB;

    protected StringEqualsExpression(IStringExpression expressionA, IStringExpression expressionB) :
        base(expressionA, expressionB)
    {
        _strExpressionA = ExpressionBuilder.ValidateStringExpression(expressionA);
        _strExpressionB = ExpressionBuilder.ValidateStringExpression(expressionB);
    }

    public static IExpression Build(IStringExpression expressionA, IStringExpression expressionB)
    {
        if ((expressionA is FixedStringValueExpression) &&
            (expressionB is FixedStringValueExpression))
        {
            FixedStringValueExpression expA = expressionA as FixedStringValueExpression;
            FixedStringValueExpression expB = expressionB as FixedStringValueExpression;

            return new FixedBooleanValueExpression(expA.Value == expB.Value);
        }

        return new StringEqualsExpression(expressionA, expressionB);
    }

    public override bool Value => _strExpressionA.Value == _strExpressionB.Value;
}
