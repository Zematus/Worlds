using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LerpFunctionExpression : IValueExpression<float>
{
    private IValueExpression<float> _startArg;
    private IValueExpression<float> _endArg;
    private IValueExpression<float> _percentArg;

    public LerpFunctionExpression(IExpression[] arguments)
    {
        if ((arguments == null) || (arguments.Length < 3))
        {
            throw new System.ArgumentException("Number of arguments less than 3");
        }

        _startArg = ExpressionBuilder.ValidateValueExpression<float>(arguments[0]);
        _endArg = ExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
        _percentArg = ExpressionBuilder.ValidateValueExpression<float>(arguments[2]);
    }

    public float Value => Mathf.Lerp(
            _startArg.Value,
            _endArg.Value,
            _percentArg.Value);

    public string GetString() => Value.ToString();

    public override string ToString()
    {
        return "lerp(" +
            _startArg.ToString() + ", " +
            _endArg.ToString() + ", " +
            _percentArg.ToString() + ")";
    }
}
