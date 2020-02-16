using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NumericEqualsExpression : EqualsExpression
{
    private INumericExpression _numExpressionA;
    private INumericExpression _numExpressionB;

    protected NumericEqualsExpression(INumericExpression expressionA, INumericExpression expressionB) :
        base(expressionA, expressionB)
    {
        _numExpressionA = ExpressionBuilder.ValidateNumericExpression(expressionA);
        _numExpressionB = ExpressionBuilder.ValidateNumericExpression(expressionB);
    }

    public static IExpression Build(INumericExpression expressionA, INumericExpression expressionB)
    {
        if ((expressionA is FixedNumberExpression) &&
            (expressionB is FixedNumberExpression))
        {
            FixedNumberExpression expA = expressionA as FixedNumberExpression;
            FixedNumberExpression expB = expressionB as FixedNumberExpression;

            return new FixedBooleanValueExpression(expA.NumberValue == expB.NumberValue);
        }

        return new NumericEqualsExpression(expressionA, expressionB);
    }

    public override bool Value => _numExpressionA.Value == _numExpressionB.Value;
}
