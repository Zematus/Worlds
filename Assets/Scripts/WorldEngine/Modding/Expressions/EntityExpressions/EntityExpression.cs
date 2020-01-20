using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EntityExpression : Expression
{
    private bool Evaluated = false;
    private Entity CachedValue;

    protected abstract Entity Evaluate();

    public abstract System.Type GetAttributeType(string attributeId);

    public abstract bool HasAttribute(string attribute);

    public virtual Entity GetValue()
    {
        if (!Evaluated)
        {
            CachedValue = Evaluate();
            Evaluated = true;
        }

        return CachedValue;
    }

    public override void ResetCache()
    {
        Evaluated = false;
    }

    protected EntityExpression ValidateExpression(Expression expression)
    {
        if (!(expression is EntityExpression entExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid entity expression");
        }

        return entExpression;
    }
}
