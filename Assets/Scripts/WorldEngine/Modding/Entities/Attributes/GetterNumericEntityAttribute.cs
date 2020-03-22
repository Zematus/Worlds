using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public delegate float ValueGetterMethod();

public class GetterNumericEntityAttribute : NumericEntityAttribute
{
    private readonly ValueGetterMethod _getterMethod;

    public GetterNumericEntityAttribute(string id, Entity entity, ValueGetterMethod getterMethod)
        : base(id, entity, null)
    {
        _getterMethod = getterMethod;
    }

    public override float Value => _getterMethod();
}
