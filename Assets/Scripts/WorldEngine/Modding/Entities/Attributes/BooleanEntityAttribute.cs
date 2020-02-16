using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class BooleanEntityAttribute : EntityAttribute
{
    public BooleanEntityAttribute(string id, Entity entity) : base(id, entity)
    { }

    public abstract bool Value { get; }
}
