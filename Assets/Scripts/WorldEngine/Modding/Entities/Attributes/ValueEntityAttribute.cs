using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class ValueEntityAttribute<T> : EntityAttribute
{
    public ValueEntityAttribute(string id, Entity entity, IExpression[] arguments)
        : base(id, entity, arguments)
    { }

    public abstract T Value { get; }

    protected override EntityAttributeExpression BuildExpression()
    {
        return new ValueEntityAttributeExpression<T>(this);
    }

    public override string ToPartiallyEvaluatedString(bool evaluate)
    {
        return Value.ToString();
    }
}
