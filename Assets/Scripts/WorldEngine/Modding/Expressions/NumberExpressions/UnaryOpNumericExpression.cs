using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryOpNumericExpression : NumericExpression
{
    public NumericExpression Expression;

    public UnaryOpNumericExpression(Context context, string expressionStr)
    {
        Expression = ValidateExpression(BuildExpression(context, expressionStr));
    }

    public UnaryOpNumericExpression(Expression expression)
    {
        Expression = ValidateExpression(expression);
    }

    public override void Reset()
    {
        Expression.Reset();

        base.Reset();
    }
}
