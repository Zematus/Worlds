using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MinFunctionExpression : FunctionExpression, IValueExpression<float>
{
    public const string FunctionId = "min";

    private readonly IValueExpression<float>[] _parameterExps;

    public MinFunctionExpression(Context c, IExpression[] arguments) :
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
            float min = float.MinValue;

            foreach (IValueExpression<float> exp in _parameterExps)
            {
                float value = exp.Value;

                if (min > value)
                    min = value;
            }

            return min;
        }
    }

    public object ValueObject => Value;

    public string GetFormattedString() => Value.ToString().ToBoldFormat();
}
