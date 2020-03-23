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

    public string GetString()
    {
        // This method should not be called for an entity expression
        throw new NotImplementedException();
    }

    public Entity Value { get; }
}
