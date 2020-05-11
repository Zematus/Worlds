using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ValueGetterEntityAttribute<T> : ValueEntityAttribute<T>
{
    private readonly ValueGetterMethod<T> _getterMethod;

    private readonly PartiallyEvaluatedStringConverter _partialEvalStringConverter;

    public ValueGetterEntityAttribute(
        string id,
        Entity entity,
        ValueGetterMethod<T> getterMethod,
        PartiallyEvaluatedStringConverter converter = null)
        : base(id, entity, null)
    {
        _getterMethod = getterMethod;
        _partialEvalStringConverter = converter;
    }

    public override T Value => _getterMethod();

    public override string ToPartiallyEvaluatedString(bool evaluate)
    {
        return _partialEvalStringConverter?.Invoke(evaluate) ?? Value.ToString();
    }
}
