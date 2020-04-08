using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ValueEntity<T> : Entity
{
    public const string ValueAttributeId = "value";

    protected ValueGetterEntityAttribute<T> _valueAttribute;

    public T Value { get; protected set; }

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
                        ValueAttributeId, this, GetValue);
                return _valueAttribute;
        }

        throw new System.ArgumentException("Faction: Unable to find attribute: " + attributeId);
    }

    public virtual T GetValue() => Value;

    public virtual void Set(T v) => Value = v;

    public override void Set(object o)
    {
        if (o is T v)
        {
            Value = v;
        }
        else
        {
            throw new System.Exception("Input reference is not of type " + typeof(T));
        }
    }

    public override string GetFormattedString()
    {
        return Value.ToString();
    }

    public IValueExpression<T> ValueExpression
    {
        get
        {
            _valueExpression = _valueExpression ?? new ValueGetterExpression<T>(GetValue);

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
