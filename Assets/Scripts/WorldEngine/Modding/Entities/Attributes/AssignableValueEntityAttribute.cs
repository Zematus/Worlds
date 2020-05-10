using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class AssignableValueEntityAttribute<T> : EntityAttribute
{
    public AssignableValueEntityAttribute(
        string id, Entity entity, IExpression[] arguments)
        : base(id, entity, arguments)
    { }

    public abstract T Value { get; set; }

    protected override EntityAttributeExpression BuildExpression()
    {
        return new AssignableValueEntityAttributeExpression<T>(this);
    }

    public override string ToPartiallyEvaluatedString(bool evaluate)
    {
        if (evaluate)
        {
            return Value.ToString();
        }

        return base.ToPartiallyEvaluatedString(evaluate);
    }
}
