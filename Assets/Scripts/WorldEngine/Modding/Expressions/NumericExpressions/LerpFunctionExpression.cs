using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LerpFunctionExpression : FunctionExpression, IValueExpression<float>
{
    public const string FunctionId = "lerp";

    private readonly IValueExpression<float> _startArg;
    private readonly IValueExpression<float> _endArg;
    private readonly IValueExpression<float> _percentArg;

    public LerpFunctionExpression(IExpression[] arguments) : base (FunctionId, 3, arguments)
    {
        _startArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);
        _endArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
        _percentArg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[2]);
    }

    public float Value => Mathf.Lerp(
            _startArg.Value,
            _endArg.Value,
            _percentArg.Value);

    public object ValueObject => Value;

    public string GetFormattedString() => Value.ToString().ToBoldFormat();
}
