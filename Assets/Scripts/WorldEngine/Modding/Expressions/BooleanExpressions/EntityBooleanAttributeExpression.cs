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
        Expression.GetValue().Attributes.TryGetValue(AttributeId, out EntityAtribute attribute);

        return (attribute as BooleanEntityAttribute).GetValue();
    }

    public override string ToString()
    {
        return Expression.ToString() + "." + AttributeId;
    }
}
