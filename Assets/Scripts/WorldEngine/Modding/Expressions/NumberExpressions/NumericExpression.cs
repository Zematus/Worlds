using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class NumericExpression : Expression
{
    private bool _evaluated = false;
    private float _cachedValue;

    protected abstract float Evaluate();

    public virtual float GetValue()
    {
        if (!_evaluated)
        {
            _cachedValue = Evaluate();
            _evaluated = true;
        }

        return _cachedValue;
    }

    public override void Reset()
    {
        _evaluated = false;
    }

    public static NumericExpression ValidateExpression(Expression expression)
    {
        if (!(expression is NumericExpression numExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid number expression");
        }

        return numExpression;
    }
}
