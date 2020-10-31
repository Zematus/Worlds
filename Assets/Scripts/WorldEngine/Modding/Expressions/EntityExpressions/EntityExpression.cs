using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class EntityExpression : IValueExpression<IEntity>
{
    public EntityExpression(IEntity entity)
    {
        Value = entity;
    }

    public override string ToString()
    {
        return Value.Id;
    }

    public string GetFormattedString()
    {
        return Value.GetFormattedString();
    }

    public IEntity Value { get; }

    public object ValueObject => Value;

    public string ToPartiallyEvaluatedString(bool evaluate)
    {
        return Value.ToPartiallyEvaluatedString(evaluate);
    }
}
