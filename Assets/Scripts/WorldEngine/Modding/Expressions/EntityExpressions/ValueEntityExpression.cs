using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class ValueEntityExpression<T> : BaseValueEntityExpression
{
    public readonly ValueEntity<T> ValueEntity;

    public ValueEntityExpression(ValueEntity<T> entity) : base(entity)
    {
        ValueEntity = entity;
    }
}
