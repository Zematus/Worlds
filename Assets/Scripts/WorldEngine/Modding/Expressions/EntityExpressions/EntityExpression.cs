using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class EntityExpression : IValueExpression<Entity>
{
    public EntityExpression(Entity entity)
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

    public Entity Value { get; }

    public object ValueObject => Value.GetObject();
}
