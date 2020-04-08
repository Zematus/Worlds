using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NegateBooleanValueExpression : UnaryOpExpression<bool>
{
    IValueExpression<bool> _boolExpression;

    public NegateBooleanValueExpression(IExpression expression) : base("!", expression)
    {
        _boolExpression = ValueExpressionBuilder.ValidateValueExpression<bool>(expression);
    }

    public static IExpression Build(Context context, string expressionStr)
    {
        IExpression expression = ExpressionBuilder.BuildExpression(context, expressionStr);

        return new NegateBooleanValueExpression(expression);
    }

    public override bool Value => !_boolExpression.Value;
}
