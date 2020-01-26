using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BooleanExpression : Expression
{
    private bool _evaluated = false;
    private bool _cachedValue;

    protected abstract bool Evaluate();

    public virtual bool GetValue()
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

    protected BooleanExpression ValidateExpression(Expression expression)
    {
        if (!(expression is BooleanExpression boolExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid boolean expression");
        }

        return boolExpression;
    }
}
