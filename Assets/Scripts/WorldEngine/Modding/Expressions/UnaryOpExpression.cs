using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryOpExpression : Expression
{
    public Expression Expression;

    public UnaryOpExpression(string expressionStr)
    {
        Expression = BuildExpression(expressionStr);
    }

    public UnaryOpExpression(Expression expression)
    {
        Expression = expression;
    }
}
