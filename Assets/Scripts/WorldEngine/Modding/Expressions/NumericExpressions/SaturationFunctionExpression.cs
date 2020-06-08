using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SaturationFunctionExpression : FunctionExpression, IValueExpression<float>
{
    public const string FunctionId = "saturation";

    private readonly IValueExpression<float> _maxSatArg;
    private readonly IValueExpression<float> _valueArg;

    public SaturationFunctionExpression(Context c, IExpression[] arguments) :
        base(c, FunctionId, 2, arguments)
    {
        _maxSatArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);
        _valueArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
    }

    public float Value
    {
        get
        {
            float value = _valueArg.Value;

            if (value < 0)
            {
                throw new System.ArgumentException(
                    _context.Id + " - " +
                    FunctionId + ": input value can't be lower than zero" +
                    "\n - expression: " + ToString() +
                    "\n - max saturation: " + _maxSatArg.ToPartiallyEvaluatedString() +
                    "\n - input value: " + _valueArg.ToPartiallyEvaluatedString());
            }

            return value / (value + _maxSatArg.Value);
        }
    }

    public object ValueObject => Value;

    public string GetFormattedString() => Value.ToString().ToBoldFormat();
}
