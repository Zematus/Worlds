using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EffectEntityAttribute : EntityAttribute
{
    public EffectEntityAttribute(string id, Entity entity, IExpression[] arguments)
        : base(id, entity, arguments)
    { }

    public abstract void Apply();

    protected override EntityAttributeExpression BuildExpression()
    {
        return new EffectEntityAttributeExpression(this);
    }
}
