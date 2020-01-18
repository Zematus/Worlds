using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NegateBooleanValueExpression : UnaryOpBooleanExpression
{
    public NegateBooleanValueExpression(string expressionStr) : base(expressionStr)
    {
    }

    public NegateBooleanValueExpression(Expression expression) : base(expression)
    {
    }

    public static Expression Build(string expressionStr)
    {
        Expression expression = BuildExpression(expressionStr);

        if (expression is BooleanValueExpression)
        {
            BooleanValueExpression boolExp = expression as BooleanValueExpression;

            boolExp.BooleanValue = !boolExp.BooleanValue;

            return boolExp;
        }

        return new NegateBooleanValueExpression(expression);
    }

    public override bool Evaluate()
    {
        return !Expression.Evaluate();
    }

    public override string ToString()
    {
        return "!(" + Expression.ToString() + ")";
    }
}
