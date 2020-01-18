using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BinaryNumberOpExpression : EvalToNumberExpression
{
    public EvalToNumberExpression ExpressionA;
    public EvalToNumberExpression ExpressionB;

    public BinaryNumberOpExpression(string expressionAStr, string expressionBStr)
    {
        ExpressionA = ValidateExpression(BuildExpression(expressionAStr));
        ExpressionB = ValidateExpression(BuildExpression(expressionBStr));
    }

    public BinaryNumberOpExpression(Expression expressionA, Expression expressionB)
    {
        ExpressionA = ValidateExpression(expressionA);
        ExpressionB = ValidateExpression(expressionB);
    }
}
