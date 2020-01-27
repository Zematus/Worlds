using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LerpFunctionExpression : NumericExpression
{
    private NumericExpression _startArg;
    private NumericExpression _endArg;
    private NumericExpression _percentArg;

    public LerpFunctionExpression(Expression[] arguments)
    {
        if (arguments.Length < 3)
        {
            throw new System.ArgumentException("Number of arguments less than 3");
        }

        _startArg = ValidateExpression(arguments[0]);
        _endArg = ValidateExpression(arguments[1]);
        _percentArg = ValidateExpression(arguments[2]);
    }

    protected override float Evaluate()
    {
        return Mathf.Lerp(
            _startArg.GetValue(),
            _endArg.GetValue(),
            _percentArg.GetValue());
    }

    public override void Reset()
    {
        _startArg.Reset();
        _endArg.Reset();
        _percentArg.Reset();

        base.Reset();
    }

    public override string ToString()
    {
        return "lerp(" +
            _startArg.ToString() + ", " +
            _endArg.ToString() + ", " +
            _percentArg.ToString() + ")";
    }
}
