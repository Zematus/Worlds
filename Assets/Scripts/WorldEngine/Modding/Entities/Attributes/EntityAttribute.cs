using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EntityAttribute
{
    public string Id;
    public Entity Entity;

    public EntityAttribute(string id, Entity entity)
    {
        Id = id;
        Entity = entity;
    }
}
