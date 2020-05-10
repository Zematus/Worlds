using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SaturationFunctionExpression : IValueExpression<float>
{
    public const string FunctionId = "saturation";

    private readonly IValueExpression<float> _maxSatArg;
    private readonly IValueExpression<float> _valueArg;

    public SaturationFunctionExpression(IExpression[] arguments)
    {
        if ((arguments == null) || (arguments.Length < 2))
        {
            throw new System.ArgumentException("Number of arguments less than 2");
        }

        _maxSatArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);
        _valueArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
    }

    public float Value
    {
        get {
            float value = _valueArg.Value;

            if (value < 0)
            {
                throw new System.Exception(
                    "saturation: input value can't be lower than zero, input: " + value);
            }

            return value / (value + _maxSatArg.Value);
        }
    }

    public object ValueObject => Value;

    public string GetFormattedString() => Value.ToString();

    public override string ToString()
    {
        return "saturation(" +
            _maxSatArg.ToString() + ", " +
            _valueArg.ToString() + ")";
    }

    public string ToPartiallyEvaluatedString(bool evaluate)
    {
        return "saturation(" +
            _maxSatArg.ToPartiallyEvaluatedString(evaluate) + ", " +
            _valueArg.ToPartiallyEvaluatedString(evaluate) + ")";
    }
}
