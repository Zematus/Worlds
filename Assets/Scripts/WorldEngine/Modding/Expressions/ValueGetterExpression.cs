using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public delegate T ValueGetterMethod<T>();
public delegate string PartiallyEvaluatedStringConverter(bool evaluate);

public class ValueGetterExpression<T> : IValueExpression<T>
{
    private readonly ValueGetterMethod<T> _getterMethod;

    private readonly PartiallyEvaluatedStringConverter _partialEvalStringConverter;

    public T Value => _getterMethod();

    public object ValueObject => Value;

    public override string ToString() => Value.ToString();

    public string GetFormattedString() => Value.ToString();

    public ValueGetterExpression(
        ValueGetterMethod<T> getterMethod,
        PartiallyEvaluatedStringConverter partialEvalStringConverter = null)
    {
        _getterMethod = getterMethod;
        _partialEvalStringConverter = partialEvalStringConverter;
    }

    public string ToPartiallyEvaluatedString(bool evaluate)
    {
        return _partialEvalStringConverter?.Invoke(evaluate) ?? Value.ToString();
    }
}
