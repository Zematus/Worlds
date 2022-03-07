using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ValueEntityAttributeExpression<T>
    : EntityAttributeExpression, IValueExpression<T>
{
    private readonly ValueEntityAttribute<T> _valAttribute;

    public ValueEntityAttributeExpression(EntityAttribute attribute)
        : base(attribute)
    {
        _valAttribute = attribute as ValueEntityAttribute<T>;

        if (_valAttribute == null)
        {
            throw new System.ArgumentException("'" + attribute.Id +
                "' is not an value entity attribute.");
        }
    }

    public T Value => _valAttribute.Value;

    public object ValueObject => Value;

    public string GetFormattedString()
    {
        if (Value is IEntity e)
        {
            return e.GetFormattedString();
        }

        return Value.ToFormattedString();
    }
}
