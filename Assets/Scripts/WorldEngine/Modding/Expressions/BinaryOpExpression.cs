using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BinaryOpExpression : Expression
{
    public Expression ExpressionA;
    public Expression ExpressionB;

    public BinaryOpExpression(string expressionAStr, string expressionBStr)
    {
        ExpressionA = BuildExpression(expressionAStr);
        ExpressionB = BuildExpression(expressionBStr);
    }

    public BinaryOpExpression(Expression expressionA, Expression expressionB)
    {
        ExpressionA = expressionA;
        ExpressionB = expressionB;
    }
}
