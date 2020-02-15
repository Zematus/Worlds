using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class FixedEntityExpression : IEntityExpression
{
    private readonly Entity _entity;

    public FixedEntityExpression(Entity entity)
    {
        _entity = entity;
    }

    public override string ToString()
    {
        return _entity.Id;
    }

    public Entity Entity => _entity;
}
