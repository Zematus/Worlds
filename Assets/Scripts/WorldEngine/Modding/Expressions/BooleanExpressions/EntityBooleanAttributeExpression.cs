using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class EntityBooleanAttributeExpression : BooleanExpression
{
    public EntityExpression Expression;

    public string AttributeId;

    public EntityBooleanAttributeExpression(EntityExpression expression, string attributeId)
    {
        Expression = expression;

        AttributeId = attributeId;
    }

    protected override bool Evaluate()
    {
        BooleanEntityAttribute attribute =
            Expression.GetValue().GetAttribute(AttributeId) as BooleanEntityAttribute;

        return attribute.GetValue();
    }

    public override string ToString()
    {
        return Expression.ToString() + "." + AttributeId;
    }
}
