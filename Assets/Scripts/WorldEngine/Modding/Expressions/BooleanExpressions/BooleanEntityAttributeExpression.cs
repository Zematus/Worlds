using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BooleanEntityAttributeExpression : EntityAttributeExpression, IBooleanExpression
{
    private readonly BooleanEntityAttribute _boolAttribute;

    public BooleanEntityAttributeExpression(
        EntityAttribute attribute) : base(attribute)
    {
        _boolAttribute = attribute as BooleanEntityAttribute;
    }

    public bool Value => _boolAttribute.Value;

    public string GetString() => Value.ToString();
}
