using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Base class for all unary operation expressions (i.e. -3, !true)
/// </summary>
public abstract class UnaryOpExpression<T> : IValueExpression<T>
{
    protected IExpression _expression;

    private readonly string _opStr;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="opStr">Operator string (i.e. '+', '-', '!', '*')</param>
    /// <param name="expression">Operand expression</param>
    public UnaryOpExpression(string opStr, IExpression expression)
    {
        _opStr = opStr;
        _expression = expression;
    }

    public override string ToString()
    {
        return "(" + _opStr + _expression + ")";
    }

    public virtual string ToPartiallyEvaluatedString(bool evaluate)
    {
        return "(" + _opStr + _expression.ToPartiallyEvaluatedString(evaluate) + ")";
    }

    public string GetFormattedString() => Value.ToString().ToBoldFormat();

    public bool TryGetRequest(out InputRequest request) =>
        _expression.TryGetRequest(out request);

    public abstract T Value { get; }

    public object ValueObject => Value;

    public bool RequiresInput => _expression.RequiresInput;
}
