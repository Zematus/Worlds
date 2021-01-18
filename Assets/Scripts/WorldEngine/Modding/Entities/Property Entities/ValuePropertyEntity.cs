using System;
using UnityEngine;

public class ValuePropertyEntity<T> : PropertyEntity<T>
{
    private IValueExpression<T> _valExpression = null;

    public override bool RequiresInput => _valExpression.RequiresInput;

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

    public override string GetDebugString() =>
        GetValue().ToString();

    public override string GetFormattedString() =>
        GetValue().ToString().ToBoldFormat();

    public override string ToPartiallyEvaluatedString(bool evaluate) =>
        _valExpression.ToPartiallyEvaluatedString(evaluate);

    public override bool TryGetRequest(out InputRequest request) =>
        _valExpression.TryGetRequest(out request);
}
