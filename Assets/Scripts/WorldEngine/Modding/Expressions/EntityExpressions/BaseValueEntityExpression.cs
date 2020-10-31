using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public abstract class BaseValueEntityExpression : EntityExpression
{
    public readonly IBaseValueEntity BaseValueEntity;

    public BaseValueEntityExpression(IBaseValueEntity entity) : base(entity)
    {
        BaseValueEntity = entity;
    }
}
