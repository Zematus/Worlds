using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NegateNumberValueExpression : UnaryOpNumericExpression
{
    public NegateNumberValueExpression(Expression expression) : base(expression)
    {
    }

    public static Expression Build(Context context, string expressionStr)
    {
        Expression expression = BuildExpression(context, expressionStr);

        if (expression is FixedNumberValueExpression)
        {
            FixedNumberValueExpression numExp = expression as FixedNumberValueExpression;

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
