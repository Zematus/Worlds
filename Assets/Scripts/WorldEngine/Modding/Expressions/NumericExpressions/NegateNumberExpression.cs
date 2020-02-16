using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NegateNumberExpression : UnaryOpNumericExpression
{
    public NegateNumberExpression(IExpression expression) : base("-", expression)
    {
    }

    public static IExpression Build(Context context, string expressionStr)
    {
        IExpression expression = ExpressionBuilder.BuildExpression(context, expressionStr);

        if (expression is FixedNumberExpression)
        {
            FixedNumberExpression numExp = expression as FixedNumberExpression;

            numExp.NumberValue = -numExp.NumberValue;

            return numExp;
        }

        return new NegateNumberExpression(expression);
    }

    public override float Value => -_numExpression.Value;
}
