using System;

public class ConditionSetPropertyEntity : PropertyEntity
{
    public const string ValueId = "value";

    private bool _value;

    public IValueExpression<bool>[] Conditions;

    private EntityAttribute _valueAttribute;

    public ConditionSetPropertyEntity(
        Context context, Context.LoadedContext.LoadedProperty p)
        : base(context, p)
    {
        if (p.conditions == null)
        {
            throw new ArgumentException("'conditions' list can't be empty");
        }

        Conditions = ValueExpressionBuilder.BuildValueExpressions<bool>(context, p.conditions);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ValueId:
                _valueAttribute =
                    _valueAttribute ?? new ValueGetterEntityAttribute<bool>(
                        ValueId, this, GetValue);
                return _valueAttribute;
        }

        throw new System.ArgumentException(Id + " property: Unable to find attribute: " + attributeId);
    }

    public bool GetValue()
    {
        EvaluateIfNeeded();

        return _value;
    }

    protected override void Calculate()
    {
        _value = true;

        foreach (IValueExpression<bool> exp in Conditions)
        {
            _value &= exp.Value;

            if (!_value)
                break;
        }
    }

    public override string GetFormattedString()
    {
        EvaluateIfNeeded();

        return _value.ToString();
    }
}
