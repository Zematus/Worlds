using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DivideExpression : BinaryOpExpressionWithOutput<float>
{
    protected IValueExpression<float> _numExpressionA;
    protected IValueExpression<float> _numExpressionB;

    public DivideExpression(IValueExpression<float> expressionA, IValueExpression<float> expressionB)
        : base("/", expressionA, expressionB)
    {
        _numExpressionA = expressionA;
        _numExpressionB = expressionB;
    }

    public static IExpression Build(
        Context context,
        string expressionAStr,
        string expressionBStr,
        bool allowInputRequesters = false)
    {
        IExpression expressionA =
            ValueExpressionBuilder.BuildValueExpression<float>(
                context, expressionAStr, allowInputRequesters);
        IExpression expressionB =
            ValueExpressionBuilder.BuildValueExpression<float>(
                context, expressionBStr, allowInputRequesters);

        return new MultiplyExpression(expressionA, expressionB);
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
