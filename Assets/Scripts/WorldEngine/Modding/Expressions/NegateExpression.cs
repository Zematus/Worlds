using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NegateExpression : UnaryOpExpression
{
    public NegateExpression(string expressionStr) : base(expressionStr)
    {
    }

    public NegateExpression(Expression expression) : base(expression)
    {
    }

    public static Expression Build(string expressionStr)
    {
        Expression expression = BuildExpression(expressionStr);

        if (expression is NumberExpression)
        {
            NumberExpression numExp = expression as NumberExpression;

            numExp.NumberValue = -numExp.NumberValue;

            return numExp;
        }

        return new NegateExpression(expression);
    }

    public override float Evaluate()
    {
        return -Expression.Evaluate();
    }

    public override string ToString()
    {
        return "-(" + Expression.ToString() + ")";
    }
}
