using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class ValueEntityAttribute<T> : EntityAttribute
{
    public ValueEntityAttribute(
        string id, Entity entity, IExpression[] arguments, int minArguments = 0)
        : base(id, entity, arguments)
    {
        if ((minArguments > 0) && ((arguments == null) || (arguments.Length < minArguments)))
        {
            throw new System.ArgumentException(
                ToString() + ": number of arguments given less than " + minArguments);
        }
    }

    public abstract T Value { get; }

    protected override IExpression BuildExpression()
    {
        return new ValueEntityAttributeExpression<T>(this);
    }

    public override string ToPartiallyEvaluatedString(int depth = -1)
    {
        if (Value is IEntity e)
        {
            return e.ToPartiallyEvaluatedString(depth);
        }

        return Value.ToString();
    }
}
