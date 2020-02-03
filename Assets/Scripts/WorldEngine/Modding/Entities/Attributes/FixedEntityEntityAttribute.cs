using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedEntityEntityAttribute : EntityEntityAttribute
{
    private readonly Entity _entity;

    public FixedEntityEntityAttribute(Entity entity)
    {
        _entity = entity;
    }

    public override Entity GetEntity()
    {
        return _entity;
    }
}
