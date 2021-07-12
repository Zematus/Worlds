using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NormalizeFunctionExpression : FunctionExpressionWithOutput<float>
{
    public const string FunctionId = "normalize";

    private readonly IValueExpression<float> _valueArg;
    private readonly IValueExpression<float> _minArg;
    private readonly IValueExpression<float> _maxArg;

    public NormalizeFunctionExpression(Context c, IExpression[] arguments) :
        base(c, FunctionId, 3, arguments)
    {
        _valueArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);
        _minArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
        _maxArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[2]);
    }

    public override float Value
    {
        get {
            float min = _minArg.Value;
            float max = _maxArg.Value;

            if (max <= min)
            {
                string inputValueStr = _valueArg.ToPartiallyEvaluatedString();
                string minValueStr = _minArg.ToPartiallyEvaluatedString();
                string maxValueStr = _maxArg.ToPartiallyEvaluatedString();

                throw new System.ArgumentException(
                    _context.Id + " - " +
                    FunctionId + ": max value can't be equal or less than min value" +
                    "\n - expression: " + ToString() +
                    "\n - input value: " + inputValueStr +
                    "\n - min value: " + minValueStr +
                    "\n - max value: " + maxValueStr);
            }

            return (_valueArg.Value - min) / (max - min);
        }
    }
}
