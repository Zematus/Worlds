using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ValueGetterExpression<T> : IValueExpression<T>
{
    private readonly ValueGetterMethod<T> _getterMethod;

    public T Value => _getterMethod();

    public object ValueObject => Value;

    public override string ToString() => Value.ToString();

    public string GetFormattedString() => Value.ToString();

    public string ToPartiallyEvaluatedString(bool evaluate) => ToString();

    public ValueGetterExpression(ValueGetterMethod<T> getterMethod)
    {
        _getterMethod = getterMethod;
    }
}
