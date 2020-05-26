using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Base class for all binary operation expressions (i.e. '2 + 2', 'A && B')
/// </summary>
public abstract class BinaryOpExpressionWithOutput<T> : BinaryOpExpression, IValueExpression<T>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="opStr">Operator string (i.e. '+', '-', '!', '*')</param>
    /// <param name="expressionA">First operand expression</param>
    /// <param name="expressionB">Second operand expression</param>
    public BinaryOpExpressionWithOutput(
        string opStr, IExpression expressionA, IExpression expressionB)
        : base(opStr, expressionA, expressionB)
    {
    }

    public string GetFormattedString() => Value.ToString().ToBoldFormat();

    public abstract T Value { get; }

    public object ValueObject => Value;
}
