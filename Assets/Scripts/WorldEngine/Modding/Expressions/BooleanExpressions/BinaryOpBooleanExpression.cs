using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BinaryOpBooleanExpression : BinaryOpExpression, IBooleanExpression
{
    public BinaryOpBooleanExpression(string opStr, IExpression expressionA, IExpression expressionB)
        : base(opStr, expressionA, expressionB)
    {
    }

    public abstract bool Value { get; }

    public string GetString() => Value.ToString();
}
