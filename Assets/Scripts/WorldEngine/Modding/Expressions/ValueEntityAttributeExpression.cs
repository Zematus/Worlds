using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ValueEntityAttributeExpression<T> : EntityAttributeExpression, IValueExpression<T>
{
    private readonly ValueEntityAttribute<T> _valAttribute;

    public ValueEntityAttributeExpression(EntityAttribute attribute)
        : base(attribute)
    {
        _valAttribute = attribute as ValueEntityAttribute<T>;
    }

    public T Value => _valAttribute.Value;

    public string GetFormattedString() => Value.ToString();
}
