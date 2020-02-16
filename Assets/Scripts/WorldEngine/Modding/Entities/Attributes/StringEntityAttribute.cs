using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class StringEntityAttribute : EntityAttribute
{
    public StringEntityAttribute(string id, Entity entity) : base(id, entity)
    { }

    public abstract string Value { get; }
}
