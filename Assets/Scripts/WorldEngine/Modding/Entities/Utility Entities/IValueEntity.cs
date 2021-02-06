using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public interface IValueEntity<T> : IBaseValueEntity
{
    T Value { get; }

    void Set(T v);

    IValueExpression<T> ValueExpression { get; }
}
