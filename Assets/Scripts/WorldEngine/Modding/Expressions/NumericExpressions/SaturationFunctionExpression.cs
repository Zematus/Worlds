using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SaturationFunctionExpression : FunctionExpressionWithOutput<float>
{
    public const string FunctionId = "saturation";

    private readonly IValueExpression<float> _maxSatArg;
    private readonly IValueExpression<float> _valueArg;

    public SaturationFunctionExpression(Context c, IExpression[] arguments) :
        base(c, FunctionId, 2, arguments)
    {
        _valueArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);
        _maxSatArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
    }

    public override float Value
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
                    "\n - input value: " + _valueArg.ToPartiallyEvaluatedString() +
                    "\n - max saturation: " + _maxSatArg.ToPartiallyEvaluatedString());
            }

            return value / (value + _maxSatArg.Value);
        }
    }
}
