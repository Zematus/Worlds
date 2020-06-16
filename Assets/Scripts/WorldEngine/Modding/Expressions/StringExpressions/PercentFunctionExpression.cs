using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PercentFunctionExpression : FunctionExpression, IValueExpression<string>
{
    public const string FunctionId = "percent";

    private readonly IValueExpression<float> _arg;

    public PercentFunctionExpression(Context c, IExpression[] arguments) :
        base(c, FunctionId, 1, arguments)
    {
        _arg = ValueExpressionBuilder.ValidateValueExpression<float>(arguments[0]);
    }

    public string Value => _arg.Value.ToString("P");

    public object ValueObject => Value;

    public string GetFormattedString() => Value.ToBoldFormat();
}
