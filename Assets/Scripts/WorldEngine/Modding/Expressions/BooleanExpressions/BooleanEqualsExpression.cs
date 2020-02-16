using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BooleanEqualsExpression : EqualsExpression
{
    private IBooleanExpression _boolExpressionA;
    private IBooleanExpression _boolExpressionB;

    protected BooleanEqualsExpression(IBooleanExpression expressionA, IBooleanExpression expressionB) :
        base(expressionA, expressionB)
    {
        _boolExpressionA = ExpressionBuilder.ValidateBooleanExpression(expressionA);
        _boolExpressionB = ExpressionBuilder.ValidateBooleanExpression(expressionB);
    }

    public static IExpression Build(IBooleanExpression expressionA, IBooleanExpression expressionB)
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

    public override bool Value => _boolExpressionA.Value == _boolExpressionB.Value;
}
