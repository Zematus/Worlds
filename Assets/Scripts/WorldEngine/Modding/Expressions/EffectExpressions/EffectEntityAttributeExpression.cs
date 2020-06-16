using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class EffectEntityAttributeExpression : EntityAttributeExpression, IEffectExpression
{
    private readonly EffectEntityAttribute _effectAttribute;

    public EffectEntityAttributeExpression(
        EntityAttribute attribute) : base(attribute)
    {
        _effectAttribute = attribute as EffectEntityAttribute;
    }

    public void Apply() => _effectAttribute.Apply();
}
