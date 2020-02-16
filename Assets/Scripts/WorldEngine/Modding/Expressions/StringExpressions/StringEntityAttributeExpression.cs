using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class StringEntityAttributeExpression : EntityAttributeExpression, IStringExpression
{
    private readonly StringEntityAttribute _strAttribute;

    public StringEntityAttributeExpression(
        EntityAttribute attribute, string args)
        : base(attribute, args)
    {
        _strAttribute = attribute as StringEntityAttribute;
    }

    public string Value => _strAttribute.Value;
}
