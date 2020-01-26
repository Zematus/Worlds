using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EntityExpression : Expression
{
    public abstract Entity GetEntity();

    public override void Reset()
    {
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
