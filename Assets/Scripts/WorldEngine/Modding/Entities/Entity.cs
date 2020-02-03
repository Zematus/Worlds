using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Entity
{
    public string Id;

    public abstract EntityAttribute GetAttribute(string attributeId, Expression[] arguments = null);
}
