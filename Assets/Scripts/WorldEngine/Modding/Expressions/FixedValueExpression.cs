using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedValueExpression<T> : IValueExpression<T>
{
    public T FixedValue;

    public T Value => FixedValue;

    public object ValueObject => FixedValue;

    public bool RequiresInput => false;

    public override string ToString() => FixedValue.ToString();

    public string GetFormattedString() => Value.ToFormattedString();

    public string ToPartiallyEvaluatedString(int depth = -1) => ToString();

    public bool TryGetRequest(out InputRequest request)
    {
        request = null;

        return false;
    }

    public FixedValueExpression(T value)
    {
        FixedValue = value;
    }
}
