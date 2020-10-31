using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public delegate T ValueGetterMethod<T>();
public delegate string PartiallyEvaluatedStringConverter(bool evaluate);

public class ValueGetterExpression<T> : IValueExpression<T>
{
    private readonly string _sourceId;

    private readonly ValueGetterMethod<T> _getterMethod;

    private readonly PartiallyEvaluatedStringConverter _partialEvalStringConverter;

    public T Value => _getterMethod();

    public object ValueObject => Value;

    public override string ToString()
    {
        return _sourceId;
    }

    public string GetFormattedString() => Value.ToString().ToBoldFormat();

    public ValueGetterExpression(
        string sourceId,
        ValueGetterMethod<T> getterMethod,
        PartiallyEvaluatedStringConverter partialEvalStringConverter = null)
    {
        _sourceId = sourceId;
        _getterMethod = getterMethod;
        _partialEvalStringConverter = partialEvalStringConverter;
    }

    public string ToPartiallyEvaluatedString(bool evaluate)
    {
        return _partialEvalStringConverter?.Invoke(evaluate) ?? Value.ToString();
    }
}
