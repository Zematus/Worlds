using System;
using UnityEngine;

public class ValuePropertyEntity<T> : PropertyEntity<T>
{
    private IValueExpression<T> _valExpression = null;

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

    public override string GetDebugString()
    {
        return GetValue().ToString();
    }

    public override string GetFormattedString()
    {
        return GetValue().ToString().ToBoldFormat();
    }

    public override string ToPartiallyEvaluatedString(bool evaluate)
    {
        return _valExpression.ToPartiallyEvaluatedString(evaluate);
    }
}
