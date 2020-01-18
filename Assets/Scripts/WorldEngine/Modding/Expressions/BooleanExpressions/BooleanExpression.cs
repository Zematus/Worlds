using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BooleanExpression : Expression
{
    public abstract bool Evaluate();

    protected BooleanExpression ValidateExpression(Expression expression)
    {
        if (!(expression is BooleanExpression boolExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid boolean expression");
        }

        return boolExpression;
    }
}
