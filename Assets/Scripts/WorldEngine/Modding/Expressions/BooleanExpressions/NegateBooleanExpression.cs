using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NegateBooleanExpression : UnaryBooleanOpExpression
{
    public NegateBooleanExpression(string expressionStr) : base(expressionStr)
    {
    }

    public NegateBooleanExpression(Expression expression) : base(expression)
    {
    }

    public static Expression Build(string expressionStr)
    {
        Expression expression = BuildExpression(expressionStr);

        if (expression is BooleanExpression)
        {
            BooleanExpression boolExp = expression as BooleanExpression;

            boolExp.BooleanValue = !boolExp.BooleanValue;

            return boolExp;
        }

        return new NegateBooleanExpression(expression);
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
