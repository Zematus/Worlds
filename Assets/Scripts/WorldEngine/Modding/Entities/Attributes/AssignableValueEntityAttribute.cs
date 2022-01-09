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

    protected override IExpression BuildExpression()
    {
        return new AssignableValueEntityAttributeExpression<T>(this);
    }

    public override string ToPartiallyEvaluatedString(int depth = -1)
    {
        if (depth == 0)
        {
            return Value.ToString();
        }

        depth = (depth > 0) ? depth - 1 : depth;

        return base.ToPartiallyEvaluatedString(depth);
    }
}
