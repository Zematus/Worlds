using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MaxFunctionExpression : FunctionExpression, IValueExpression<float>
{
    public const string FunctionId = "max";

    private readonly IValueExpression<float>[] _parameterExps;

    public MaxFunctionExpression(Context c, IExpression[] arguments) :
        base(c, FunctionId, 2, arguments)
    {
        _parameterExps = new IValueExpression<float>[arguments.Length];

        for (int i = 0; i < arguments.Length; i++)
        {
            _parameterExps[i] =
                ValueExpressionBuilder.ValidateValueExpression<float>(arguments[i]);
        }
    }

    public float Value
    {
        get {
            float max = float.MinValue;

            foreach (IValueExpression<float> exp in _parameterExps)
            {
                float value = exp.Value;

                if (max < value)
                    max = value;
            }

            return max;
        }
    }

    public object ValueObject => Value;

    public string GetFormattedString() => Value.ToString().ToBoldFormat();
}
