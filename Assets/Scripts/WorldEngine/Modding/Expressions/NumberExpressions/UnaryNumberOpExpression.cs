using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryNumberOpExpression : EvalToNumberExpression
{
    public EvalToNumberExpression Expression;

    public UnaryNumberOpExpression(string expressionStr)
    {
        Expression = ValidateExpression(BuildExpression(expressionStr));
    }

    public UnaryNumberOpExpression(Expression expression)
    {
        Expression = ValidateExpression(expression);
    }
}
