using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ClampFunctionExpression : FunctionExpression, IValueExpression<float>
{
    public const string FunctionId = "clamp";

    private readonly IValueExpression<float> _minParameterExp;
    private readonly IValueExpression<float> _maxParameterExp;
    private readonly IValueExpression<float> _inputParameterExp;

    public ClampFunctionExpression(Context c, IExpression[] arguments) :
        base(c, FunctionId, 3, arguments)
    {
        _inputParameterExp =
            ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);
        _minParameterExp =
            ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
        _maxParameterExp =
            ValueExpressionBuilder.ValidateValueExpression<float>(arguments[2]);
    }

    public float Value
    {
        get {
            return Mathf.Clamp(
                _inputParameterExp.Value,
                _minParameterExp.Value,
                _maxParameterExp.Value);
        }
    }

    public object ValueObject => Value;

    public string GetFormattedString() => Value.ToString().ToBoldFormat();
}
