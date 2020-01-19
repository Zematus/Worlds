using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BinaryOpNumericExpression : NumericExpression
{
    public NumericExpression ExpressionA;
    public NumericExpression ExpressionB;

    public BinaryOpNumericExpression(Context context, string expressionAStr, string expressionBStr)
    {
        ExpressionA = ValidateExpression(BuildExpression(context, expressionAStr));
        ExpressionB = ValidateExpression(BuildExpression(context, expressionBStr));
    }

    public BinaryOpNumericExpression(Expression expressionA, Expression expressionB)
    {
        ExpressionA = ValidateExpression(expressionA);
        ExpressionB = ValidateExpression(expressionB);
    }
}
