using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryBooleanOpExpression : EvalToBooleanExpression
{
    public EvalToBooleanExpression Expression;

    public UnaryBooleanOpExpression(string expressionStr)
    {
        Expression = ValidateExpression(BuildExpression(expressionStr));
    }

    public UnaryBooleanOpExpression(Expression expression)
    {
        Expression = ValidateExpression(expression);
    }
}
