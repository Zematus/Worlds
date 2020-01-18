using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BinaryOpBooleanExpression : BooleanExpression
{
    public BooleanExpression ExpressionA;
    public BooleanExpression ExpressionB;

    public BinaryOpBooleanExpression(string expressionAStr, string expressionBStr)
    {
        ExpressionA = ValidateExpression(BuildExpression(expressionAStr));
        ExpressionB = ValidateExpression(BuildExpression(expressionBStr));
    }

    public BinaryOpBooleanExpression(Expression expressionA, Expression expressionB)
    {
        ExpressionA = ValidateExpression(expressionA);
        ExpressionB = ValidateExpression(expressionB);
    }
}
