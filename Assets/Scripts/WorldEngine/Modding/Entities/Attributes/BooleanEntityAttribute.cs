using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BooleanEntityAttribute : EntityAttribute
{
    public BooleanEntityAttribute(string id, Entity entity, IExpression[] arguments)
        : base(id, entity, arguments)
    { }

    public abstract bool Value { get; }

    protected override EntityAttributeExpression BuildExpression()
    {
        return new BooleanEntityAttributeExpression(this);
    }
}
