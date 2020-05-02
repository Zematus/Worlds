using System;
using UnityEngine;

public class ValuePropertyEntity<T> : PropertyEntity<T>
{
    private IValueExpression<T> _valExpression;

    public ValuePropertyEntity(Context context, string id, IExpression exp)
        : base(context, id)
    {
        _valExpression = ValueExpressionBuilder.ValidateValueExpression<T>(exp);
    }

    public override T GetValue()
    {
        EvaluateIfNeeded();

        return Value;
    }

    protected override void Calculate()
    {
        Value = _valExpression.Value;
    }

    public override string GetFormattedString()
    {
        return GetValue().ToString();
    }
}
