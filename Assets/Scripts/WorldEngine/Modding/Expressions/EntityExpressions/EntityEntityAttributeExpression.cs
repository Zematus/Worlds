﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class EntityEntityAttributeExpression : EntityAttributeExpression, IEntityExpression
{
    private readonly EntityEntityAttribute _entAttribute;

    public EntityEntityAttributeExpression(
        EntityAttribute attribute, string args)
        : base(attribute, args)
    {
        _entAttribute = attribute as EntityEntityAttribute;
    }

    public Entity Entity => _entAttribute.AttributeEntity;
}
