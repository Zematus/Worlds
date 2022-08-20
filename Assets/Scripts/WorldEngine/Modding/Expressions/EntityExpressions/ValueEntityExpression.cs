using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class ValueEntityExpression<T> : BaseValueEntityExpression
{
    public readonly IValueEntity<T> ValueEntity;

    public ValueEntityExpression(IValueEntity<T> entity) : base(entity)
    {
        ValueEntity = entity;
    }
}
