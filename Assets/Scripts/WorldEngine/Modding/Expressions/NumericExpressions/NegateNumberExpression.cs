using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NegateNumberExpression : UnaryOpExpression<float>
{
    protected IValueExpression<float> _numExpression;

    public NegateNumberExpression(IExpression expression) : base("-", expression)
    {
        _numExpression = ValueExpressionBuilder.ValidateValueExpression<float>(expression);
    }

    public static IExpression Build(
        Context context, string expressionStr, bool allowInputRequesters = false)
    {
        IExpression expression =
            ExpressionBuilder.BuildExpression(context, expressionStr, allowInputRequesters);

        return new NegateNumberExpression(expression);
    }

    public override float Value => -_numExpression.Value;
}
