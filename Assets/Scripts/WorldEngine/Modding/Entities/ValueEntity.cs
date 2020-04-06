using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ValueEntity<T> : Entity
{
    public const string ValueAttributeId = "value";

    private ValueGetterEntityAttribute<T> _valueAttribute;

    public T Value { get; private set; }

    protected override object _reference => Value;

    private IValueExpression<T> _valueExpression = null;

    public ValueEntity(string id) : base(id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ValueAttributeId:
                _valueAttribute =
                    _valueAttribute ?? new ValueGetterEntityAttribute<T>(
                        ValueAttributeId, this, () => Value);
                return _valueAttribute;
        }

        throw new System.ArgumentException("Faction: Unable to find attribute: " + attributeId);
    }

    public override void Set(object o)
    {
        if (!(o is T))
        {
            throw new System.Exception("Input reference is not of type " + typeof(T));
        }

        Value = (T)o;
    }

    public override string GetFormattedString()
    {
        return Value.ToString();
    }

    public IValueExpression<T> ValueExpression
    {
        get
        {
            _valueExpression = _valueExpression ?? new FixedValueExpression<T>(Value);

            return _valueExpression;
        }
    }

    public override IValueExpression<Entity> Expression
    {
        get
        {
            _expression = _expression ?? new ValueEntityExpression<T>(this);

            return _expression;
        }
    }
}
