using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EvalToNumberExpression : Expression
{
    public abstract float Evaluate();

    protected EvalToNumberExpression ValidateExpression(Expression expression)
    {
        if (!(expression is EvalToNumberExpression numExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid number expression");
        }

        return numExpression;
    }
}
