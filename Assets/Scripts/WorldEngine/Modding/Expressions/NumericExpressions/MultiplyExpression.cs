using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MultiplyExpression : BinaryOpExpressionWithOutput<float>
{
    protected IValueExpression<float> _numExpressionA;
    protected IValueExpression<float> _numExpressionB;

    public MultiplyExpression(IExpression expressionA, IExpression expressionB)
        : base("*", expressionA, expressionB)
    {
        _numExpressionA = ValueExpressionBuilder.ValidateValueExpression<float>(expressionA);
        _numExpressionB = ValueExpressionBuilder.ValidateValueExpression<float>(expressionB);
    }

    public static IExpression Build(Context context, string expressionAStr, string expressionBStr)
    {
        IExpression expressionA = ExpressionBuilder.BuildExpression(context, expressionAStr);
        IExpression expressionB = ExpressionBuilder.BuildExpression(context, expressionBStr);

        return new MultiplyExpression(expressionA, expressionB);
    }

    public override float Value => _numExpressionA.Value * _numExpressionB.Value;
}
