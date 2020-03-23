using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public delegate T ValueGetterMethod<T>();

public class ValueGetterEntityAttribute<T> : ValueEntityAttribute<T>
{
    private readonly ValueGetterMethod<T> _getterMethod;

    public ValueGetterEntityAttribute(string id, Entity entity, ValueGetterMethod<T> getterMethod)
        : base(id, entity, null)
    {
        _getterMethod = getterMethod;
    }

    public override T Value => _getterMethod();
}
