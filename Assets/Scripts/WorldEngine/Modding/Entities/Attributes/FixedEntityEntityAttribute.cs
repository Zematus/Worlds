using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedEntityEntityAttribute : EntityEntityAttribute
{
    private readonly Entity _atrEntity;

    public FixedEntityEntityAttribute(Entity attrEntity, string id, Entity entity)
        : base(id, entity)
    {
        _atrEntity = attrEntity;
    }

    public override Entity AttributeEntity => _atrEntity;
}
