using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public delegate T ValueGetterMethod<T>();
public delegate string PartiallyEvaluatedStringConverter(int depth = -1);

public class ValueGetterExpression<T> : IValueExpression<T>
{
    private readonly string _sourceId;

    private readonly ValueGetterMethod<T> _getterMethod;
    private readonly PartiallyEvaluatedStringConverter _partialEvalStringConverter;

    public T Value => _getterMethod();

    public object ValueObject => Value;

    public virtual bool RequiresInput => false;

    public override string ToString()
    {
        return _sourceId;
    }

    public string GetFormattedString()
    {
        if (Value is IFormattedStringGenerator generator)
        {
            return generator.GetFormattedString();
        }

        return Value.ToString().ToBoldFormat();
    }

    public ValueGetterExpression(
        string sourceId,
        ValueGetterMethod<T> getterMethod,
        PartiallyEvaluatedStringConverter partialEvalStringConverter = null)
    {
        _sourceId = sourceId;
        _getterMethod = getterMethod;
        _partialEvalStringConverter = partialEvalStringConverter;
    }

    public string ToPartiallyEvaluatedString(int depth = -1)
    {
        return _partialEvalStringConverter?.Invoke(depth) ?? Value.ToString();
    }

    public virtual bool TryGetRequest(out InputRequest request)
    {
        request = null;

        return false;
    }
}
