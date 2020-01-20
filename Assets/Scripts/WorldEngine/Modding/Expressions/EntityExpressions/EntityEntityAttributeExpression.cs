using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class EntityEntityAttributeExpression : EntityExpression
{
    public EntityExpression Expression;

    public string AttributeId;

    public EntityEntityAttributeExpression(EntityExpression expression, string attributeId)
    {
        Expression = expression;

        AttributeId = attributeId;
    }

    protected override Entity Evaluate()
    {
        EntityEntityAttribute attribute =
            Expression.GetValue().GetAttribute(AttributeId) as EntityEntityAttribute;

        return attribute.GetValue();
    }

    public override string ToString()
    {
        return Expression.ToString() + "." + AttributeId;
    }

    public override Type GetAttributeType(string attributeId)
    {
        throw new NotImplementedException();
    }
}
