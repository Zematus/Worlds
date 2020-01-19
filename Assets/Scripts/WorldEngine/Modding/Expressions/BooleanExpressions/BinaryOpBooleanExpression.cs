using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BinaryOpBooleanExpression : BooleanExpression
{
    public BooleanExpression ExpressionA;
    public BooleanExpression ExpressionB;

    public BinaryOpBooleanExpression(Context context, string expressionAStr, string expressionBStr)
    {
        ExpressionA = ValidateExpression(BuildExpression(context, expressionAStr));
        ExpressionB = ValidateExpression(BuildExpression(context, expressionBStr));
    }

    public BinaryOpBooleanExpression(Expression expressionA, Expression expressionB)
    {
        ExpressionA = ValidateExpression(expressionA);
        ExpressionB = ValidateExpression(expressionB);
    }
}
