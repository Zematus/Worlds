using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LerpFunctionExpression : INumericExpression
{
    private INumericExpression _startArg;
    private INumericExpression _endArg;
    private INumericExpression _percentArg;

    public LerpFunctionExpression(IExpression[] arguments)
    {
        if ((arguments == null) || (arguments.Length < 3))
        {
            throw new System.ArgumentException("Number of arguments less than 3");
        }

        _startArg = ExpressionBuilder.ValidateNumericExpression(arguments[0]);
        _endArg = ExpressionBuilder.ValidateNumericExpression(arguments[1]);
        _percentArg = ExpressionBuilder.ValidateNumericExpression(arguments[2]);
    }

    public float Value => Mathf.Lerp(
            _startArg.Value,
            _endArg.Value,
            _percentArg.Value);

    public override string ToString()
    {
        return "lerp(" +
            _startArg.ToString() + ", " +
            _endArg.ToString() + ", " +
            _percentArg.ToString() + ")";
    }
}
