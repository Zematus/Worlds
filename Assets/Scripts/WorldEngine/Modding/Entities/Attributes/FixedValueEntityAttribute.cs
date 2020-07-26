using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedValueEntityAttribute<T> : ValueEntityAttribute<T>
{
    private readonly T _attrValue;

    public FixedValueEntityAttribute(T attrValue, string id, Entity entity)
        : base(id, entity, null)
    {
        _attrValue = attrValue;
    }

    public override T Value => _attrValue;
}
