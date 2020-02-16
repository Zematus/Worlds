using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class NumericEntityAttribute : EntityAttribute
{
    public NumericEntityAttribute(string id, Entity entity) : base(id, entity)
    { }

    public abstract float Value { get; }
}
