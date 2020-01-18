using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class NumericExpression : Expression
{
    public abstract float Evaluate();

    protected NumericExpression ValidateExpression(Expression expression)
    {
        if (!(expression is NumericExpression numExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid number expression");
        }

        return numExpression;
    }
}
