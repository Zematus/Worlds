using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EvalToBooleanExpression : Expression
{
    public abstract bool Evaluate();

    protected EvalToBooleanExpression ValidateExpression(Expression expression)
    {
        if (!(expression is EvalToBooleanExpression boolExpression))
        {
            throw new System.ArgumentException(expression + " is not a valid boolean expression");
        }

        return boolExpression;
    }
}
