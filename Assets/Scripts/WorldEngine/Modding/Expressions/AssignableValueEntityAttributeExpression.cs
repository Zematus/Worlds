using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AssignableValueEntityAttributeExpression<T>
    : EntityAttributeExpression, IAssignableValueExpression<T>
{
    private readonly AssignableValueEntityAttribute<T> _valAttribute;

    public AssignableValueEntityAttributeExpression(EntityAttribute attribute)
        : base(attribute)
    {
        _valAttribute = attribute as AssignableValueEntityAttribute<T>;

        if (_valAttribute == null)
        {
            throw new System.ArgumentException("'" + attribute.Id +
                "' is not an assignable value entity attribute.");
        }
    }

    public T Value
    {
        get => _valAttribute.Value;
        set => _valAttribute.Value = value;
    }

    public string GetFormattedString() => Value.ToString();
}
