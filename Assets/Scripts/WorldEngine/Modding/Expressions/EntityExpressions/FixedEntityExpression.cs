using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class FixedEntityExpression : EntityExpression
{
    public const string Regex = ModUtility.IdentifierRegex;

    public Entity Entity;

    public FixedEntityExpression(Context context, string entityId)
    {
        if (context.Entities.TryGetValue(entityId, out Entity))
        {
            throw new System.ArgumentException(
                "context '" + context.Id + "' doesn't contain entity '" + entityId + "'");
        }
    }

    protected override Entity Evaluate()
    {
        return Entity;
    }

    public override Entity GetValue()
    {
        return Entity;
    }

    public override string ToString()
    {
        return Entity.ToString();
    }

    public override Type GetAttributeType(string attributeId)
    {
        return Entity.GetAttributeType(attributeId);
    }
}
