using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class StringExpression : Expression
{
    private bool _evaluated = false;
    private string _cachedValue;

    protected abstract string Evaluate();

    public virtual string GetValue()
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

    public static StringExpression ValidateExpression(Expression expression)
    {
        if (!(expression is StringExpression strExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid string expression");
        }

        return strExpression;
    }
}
