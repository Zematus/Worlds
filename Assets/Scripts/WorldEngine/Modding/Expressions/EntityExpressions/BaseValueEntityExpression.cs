using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public abstract class BaseValueEntityExpression : EntityExpression
{
    public readonly BaseValueEntity BaseValueEntity;

    public BaseValueEntityExpression(BaseValueEntity entity) : base(entity)
    {
        BaseValueEntity = entity;
    }
}
