using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NormalizeFunctionExpression : FunctionExpression, IValueExpression<float>
{
    public const string FunctionId = "normalize";

    private readonly IValueExpression<float> _valueArg;
    private readonly IValueExpression<float> _minArg;
    private readonly IValueExpression<float> _maxArg;

    public NormalizeFunctionExpression(IExpression[] arguments) : base(FunctionId, 3, arguments)
    {
        _valueArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);
        _minArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
        _maxArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[2]);
    }

    public float Value
    {
        get {
            float min = _minArg.Value;
            float max = _maxArg.Value;

            if (max <= min)
            {
                throw new System.ArgumentException(
                    FunctionId + ": max value can't be equal or less than min value" +
                    " - expression: " + ToString() +
                    " - input value: " + _valueArg.ToPartiallyEvaluatedString() +
                    " - min value: " + _minArg.ToPartiallyEvaluatedString() +
                    " - max value: " + _maxArg.ToPartiallyEvaluatedString());
            }

            return (_valueArg.Value - min) / (max - min);
        }
    }

    public object ValueObject => Value;

    public string GetFormattedString() => Value.ToString();
}
