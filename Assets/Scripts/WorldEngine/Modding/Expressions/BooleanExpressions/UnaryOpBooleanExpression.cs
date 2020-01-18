using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryOpBooleanExpression : BooleanExpression
{
    public BooleanExpression Expression;

    public UnaryOpBooleanExpression(string expressionStr)
    {
        Expression = ValidateExpression(BuildExpression(expressionStr));
    }

    public UnaryOpBooleanExpression(Expression expression)
    {
        Expression = ValidateExpression(expression);
    }
}
