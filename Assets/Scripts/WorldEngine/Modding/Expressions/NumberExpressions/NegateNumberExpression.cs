using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NegateNumberExpression : UnaryNumberOpExpression
{
    public NegateNumberExpression(string expressionStr) : base(expressionStr)
    {
    }

    public NegateNumberExpression(Expression expression) : base(expression)
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

        return new NegateNumberExpression(expression);
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
