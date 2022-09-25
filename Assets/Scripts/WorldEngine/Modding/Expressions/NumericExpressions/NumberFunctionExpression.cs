using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NumberFunctionExpression : FunctionExpressionWithOutput<float>
{
    public const string FunctionId = "number";

    private readonly IValueExpression<bool> _arg;

    public NumberFunctionExpression(Context c, IExpression[] arguments) :
        base(c, FunctionId, 1, arguments)
    {
        _arg = ValueExpressionBuilder.ValidateValueExpression<bool>(arguments[0]);
    }

    public override float Value => _arg.Value ? 1 : 0;
}
