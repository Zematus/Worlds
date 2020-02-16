using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EntityEntityAttribute : EntityAttribute
{
    public EntityEntityAttribute(string id, Entity entity, IExpression[] arguments)
        : base(id, entity, arguments)
    { }

    public abstract Entity AttributeEntity { get; }

    protected override EntityAttributeExpression BuildExpression()
    {
        return new EntityEntityAttributeExpression(this);
    }
}
