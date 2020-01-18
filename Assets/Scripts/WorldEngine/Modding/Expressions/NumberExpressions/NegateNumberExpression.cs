using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NegateNumberValueExpression : UnaryOpNumericExpression
{
    public NegateNumberValueExpression(string expressionStr) : base(expressionStr)
    {
    }

    public NegateNumberValueExpression(Expression expression) : base(expression)
    {
    }

    public static Expression Build(string expressionStr)
    {
        Expression expression = BuildExpression(expressionStr);

        if (expression is NumberValueExpression)
        {
            NumberValueExpression numExp = expression as NumberValueExpression;

            numExp.NumberValue = -numExp.NumberValue;

            return numExp;
        }

        return new NegateNumberValueExpression(expression);
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
