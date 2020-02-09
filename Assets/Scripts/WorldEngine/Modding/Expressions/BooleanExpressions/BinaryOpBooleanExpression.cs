using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BinaryOpBooleanExpression : BooleanExpression
{
    public Expression ExpressionA;
    public Expression ExpressionB;

    public BinaryOpBooleanExpression(Context context, string expressionAStr, string expressionBStr)
    {
        ExpressionA = BuildExpression(context, expressionAStr);
        ExpressionB = BuildExpression(context, expressionBStr);
    }

    public BinaryOpBooleanExpression(Expression expressionA, Expression expressionB)
    {
        ExpressionA = expressionA;
        ExpressionB = expressionB;
    }

    public override void Reset()
    {
        ExpressionA.Reset();
        ExpressionB.Reset();

        base.Reset();
    }
}
