using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NegateNumberExpression : UnaryOpNumericExpression
{
    public NegateNumberExpression(Expression expression) : base(expression)
    {
    }

    public static Expression Build(Context context, string expressionStr)
    {
        Expression expression = BuildExpression(context, expressionStr);

        if (expression is FixedNumberExpression)
        {
            FixedNumberExpression numExp = expression as FixedNumberExpression;

            numExp.NumberValue = -numExp.NumberValue;

            return numExp;
        }

        return new NegateNumberExpression(expression);
    }

    protected override float Evaluate()
    {
        return -Expression.GetValue();
    }

    public override string ToString()
    {
        return "-" + Expression.ToString();
    }
}
