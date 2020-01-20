using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class NumericExpression : Expression
{
    private bool Evaluated = false;
    private float CachedValue;

    protected abstract float Evaluate();

    public virtual float GetValue()
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

    protected NumericExpression ValidateExpression(Expression expression)
    {
        if (!(expression is NumericExpression numExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid number expression");
        }

        return numExpression;
    }
}
