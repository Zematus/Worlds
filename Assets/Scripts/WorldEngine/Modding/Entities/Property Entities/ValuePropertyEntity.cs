using System;

public class ValuePropertyEntity<T> : PropertyEntity
{
    private T _value;

    private IValueExpression<T> _expression;

    public const string ValueId = "value";

    protected EntityAttribute _valueAttribute;

    public ValuePropertyEntity(Context context, string id, IExpression exp)
        : base(context, id)
    {
        _expression = ValueExpressionBuilder.ValidateValueExpression<T>(exp);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ValueId:
                _valueAttribute =
                    _valueAttribute ?? new ValueGetterEntityAttribute<T>(
                        ValueId, this, GetValue);
                return _valueAttribute;
        }

        throw new System.ArgumentException(Id + " property: Unable to find attribute: " + attributeId);
    }

    public T GetValue()
    {
        EvaluateIfNeeded();

        return _value;
    }

    protected override void Calculate()
    {
        _value = _expression.Value;
    }

    public override string GetFormattedString()
    {
        EvaluateIfNeeded();

        return _value.ToString();
    }
}
