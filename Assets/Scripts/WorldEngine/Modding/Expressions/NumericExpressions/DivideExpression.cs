using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DivideExpression : BinaryOpExpressionWithOutput<float>
{
    protected IValueExpression<float> _numExpressionA;
    protected IValueExpression<float> _numExpressionB;

    public DivideExpression(IExpression expressionA, IExpression expressionB)
        : base("/", expressionA, expressionB)
    {
        _numExpressionA = ValueExpressionBuilder.ValidateValueExpression<float>(expressionA);
        _numExpressionB = ValueExpressionBuilder.ValidateValueExpression<float>(expressionB);
    }

    public static IExpression Build(
        Context context,
        string expressionAStr,
        string expressionBStr,
        bool allowInputRequesters = false)
    {
        IExpression expressionA =
            ExpressionBuilder.BuildExpression(
                context, expressionAStr, allowInputRequesters);
        IExpression expressionB =
            ExpressionBuilder.BuildExpression(
                context, expressionBStr, allowInputRequesters);

        return new DivideExpression(expressionA, expressionB);
    }

    public override float Value
    {
        get
        {
            float valueB = _numExpressionB.Value;

            if (valueB == 0)
            {
                throw new System.DivideByZeroException(
                    "Expression results in division by zero" +
                    "\n - expression: " + ToString() +
                    "\n - dividend: " + _numExpressionA.ToPartiallyEvaluatedString() +
                    "\n - divisor: " + _numExpressionB.ToPartiallyEvaluatedString());
            }

            return _numExpressionA.Value / valueB;
        }
    }
}
