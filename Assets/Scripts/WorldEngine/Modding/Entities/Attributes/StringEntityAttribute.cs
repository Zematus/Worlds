using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class StringEntityAttribute : EntityAttribute
{
    public StringEntityAttribute(string id, Entity entity, IExpression[] arguments)
        : base(id, entity, arguments)
    { }

    public abstract string Value { get; }

    protected override EntityAttributeExpression BuildExpression()
    {
        return new StringEntityAttributeExpression(this);
    }
}
