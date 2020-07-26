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

    protected override EntityAttributeExpression BuildExpression()
    {
        return new ValueEntityAttributeExpression<T>(this);
    }

    public override string ToPartiallyEvaluatedString(bool evaluate)
    {
        if (Value is Entity e)
        {
            return e.ToPartiallyEvaluatedString(evaluate);
        }

        return Value.ToString();
    }
}
