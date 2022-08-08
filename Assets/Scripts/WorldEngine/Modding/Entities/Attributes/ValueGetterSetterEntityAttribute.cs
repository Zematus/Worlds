using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ValueGetterSetterEntityAttribute<T> : AssignableValueEntityAttribute<T>
{
    private readonly ValueGetterMethod<T> _getterMethod;
    private readonly ValueSetterMethod<T> _setterMethod;

    private readonly PartiallyEvaluatedStringConverter _partialEvalStringConverter;

    public ValueGetterSetterEntityAttribute(
        string id,
        Entity entity,
        ValueGetterMethod<T> getterMethod,
        ValueSetterMethod<T> setterMethod,
        PartiallyEvaluatedStringConverter converter = null)
        : base(id, entity, null)
    {
        _getterMethod = getterMethod;
        _setterMethod = setterMethod;
        _partialEvalStringConverter = converter;
    }

    public override T Value 
    {
        get => _getterMethod();
        set => _setterMethod(value); 
    }

    public override string ToPartiallyEvaluatedString(int depth = -1)
    {
        return _partialEvalStringConverter?.Invoke(depth) ?? Value.ToString();
    }
}
