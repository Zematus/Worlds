using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class UnaryOpNumericExpression : NumericExpression
{
    public NumericExpression Expression;

    public UnaryOpNumericExpression(string expressionStr)
    {
        Expression = ValidateExpression(BuildExpression(expressionStr));
    }

    public UnaryOpNumericExpression(Expression expression)
    {
        Expression = ValidateExpression(expression);
    }
}
