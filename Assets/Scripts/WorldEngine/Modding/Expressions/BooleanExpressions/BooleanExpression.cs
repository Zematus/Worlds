using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BooleanExpression : Expression
{
    private bool Evaluated = false;
    private bool CachedValue;

    protected abstract bool Evaluate();

    public virtual bool GetValue()
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

    protected BooleanExpression ValidateExpression(Expression expression)
    {
        if (!(expression is BooleanExpression boolExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid boolean expression");
        }

        return boolExpression;
    }
}
