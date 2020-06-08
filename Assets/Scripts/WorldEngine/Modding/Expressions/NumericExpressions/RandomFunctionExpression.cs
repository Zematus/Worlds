using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RandomFunctionExpression : FunctionExpression, IValueExpression<float>
{
    public const string FunctionId = "random";

    private readonly IValueExpression<float> _minMaxArg;
    private readonly IValueExpression<float> _maxArg = null;

    private int _iterOffset;

    public RandomFunctionExpression(Context c, IExpression[] arguments) :
        base(c, FunctionId, 1, arguments)
    {
        _iterOffset = c.GetNextIterOffset();

        _minMaxArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);

        if (arguments.Length > 1)
        {
            _maxArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
        }
    }

    public float Value
    {
        get {
            float minMax = _minMaxArg.Value;

            float min = 0;
            float max = minMax;

            if (_maxArg != null)
            {
                min = minMax;
                max = _maxArg.Value;
            }

            if (max < min)
            {
                string minValueStr = min.ToString();
                string maxValueStr = _minMaxArg.ToPartiallyEvaluatedString();

                if (_maxArg != null)
                {
                    minValueStr = maxValueStr;
                    maxValueStr = _maxArg.ToPartiallyEvaluatedString();
                }

                throw new System.ArgumentException(
                    _context.Id + " - " +
                    FunctionId + ": max value can't be less than min value" +
                    "\n - expression: " + ToString() +
                    "\n - min value: " + minValueStr +
                    "\n - max value: " + maxValueStr);
            }

            return Mathf.Lerp(min, max, _context.GetNextRandomFloat(_iterOffset));
        }
    }

    public object ValueObject => Value;

    public string GetFormattedString() => Value.ToString().ToBoldFormat();
}
