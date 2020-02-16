using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NumericEntityAttributeExpression : EntityAttributeExpression, INumericExpression
{
    private readonly NumericEntityAttribute _numAttribute;

    public NumericEntityAttributeExpression(
        EntityAttribute attribute, string args)
        : base(attribute, args)
    {
        _numAttribute = attribute as NumericEntityAttribute;
    }

    public float Value => _numAttribute.Value;
}
