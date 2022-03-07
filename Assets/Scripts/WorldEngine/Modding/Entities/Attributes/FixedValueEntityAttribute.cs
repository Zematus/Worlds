using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedValueEntityAttribute<T> : ValueEntityAttribute<T>
{
    protected readonly T _attrValue;

    public FixedValueEntityAttribute(T attrValue, string id, IEntity entity)
        : base(id, entity, null)
    {
        _attrValue = attrValue;
    }

    public override T Value => _attrValue;
}
