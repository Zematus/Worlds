using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedValueExpression<T> : IValueExpression<T>
{
    public T FixedValue;

    public T Value => FixedValue;

    public object ValueObject => FixedValue;

    public override string ToString() => FixedValue.ToString();

    public string GetFormattedString() => Value.ToString();

    public string ToPartiallyEvaluatedString(bool evaluate) => ToString();

    public FixedValueExpression(T value)
    {
        FixedValue = value;
    }
}
