using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EntityAttribute
{
    public string Id;
    public Entity Entity;
    public IExpression[] Arguments;

    private EntityAttributeExpression _attrExpression = null;

    public EntityAttribute(string id, Entity entity, IExpression[] arguments)
    {
        Id = id;
        Entity = entity;
        Arguments = arguments;
    }

    protected abstract EntityAttributeExpression BuildExpression();

    public EntityAttributeExpression GetExpression()
    {
        _attrExpression = _attrExpression ?? BuildExpression();

        return _attrExpression;
    }

    public override string ToString()
    {
        return Entity.Id + "." + Id;
    }

    public virtual string ToPartiallyEvaluatedString(bool evaluate) => ToString();
}
