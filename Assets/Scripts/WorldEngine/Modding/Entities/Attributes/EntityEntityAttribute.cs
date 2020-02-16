using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EntityEntityAttribute : EntityAttribute
{
    public EntityEntityAttribute(string id, Entity entity) : base(id, entity)
    { }

    public abstract Entity AttributeEntity { get; }
}
