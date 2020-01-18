using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BinaryBooleanOpExpression : EvalToBooleanExpression
{
    public EvalToBooleanExpression ExpressionA;
    public EvalToBooleanExpression ExpressionB;

    public BinaryBooleanOpExpression(string expressionAStr, string expressionBStr)
    {
        ExpressionA = ValidateExpression(BuildExpression(expressionAStr));
        ExpressionB = ValidateExpression(BuildExpression(expressionBStr));
    }

    public BinaryBooleanOpExpression(Expression expressionA, Expression expressionB)
    {
        ExpressionA = ValidateExpression(expressionA);
        ExpressionB = ValidateExpression(expressionB);
    }
}
