using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PercentFunctionExpression : IValueExpression<string>
{
    public const string FunctionId = "percent";

    private readonly IValueExpression<float> _arg;

    public PercentFunctionExpression(IExpression[] arguments)
    {
        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException("Number of arguments less than 1");
        }

        _arg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);
    }

    public string Value => _arg.Value.ToString("P");

    public string GetFormattedString() => Value;

    public override string ToString()
    {
        return "percent(" + _arg.ToString() + ")";
    }
}
