using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BinaryOpNumericExpression : BinaryOpExpression, INumericExpression
{
    protected INumericExpression _numExpressionA;
    protected INumericExpression _numExpressionB;

    public BinaryOpNumericExpression(string opStr, IExpression expressionA, IExpression expressionB)
        : base(opStr, expressionA, expressionB)
    {
        _numExpressionA = ExpressionBuilder.ValidateNumericExpression(expressionA);
        _numExpressionA = ExpressionBuilder.ValidateNumericExpression(expressionB);
    }

    public abstract float GetValue();
}
