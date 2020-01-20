using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Entity
{
    public abstract EntityAttribute GetAttribute(string attributeId);
    public abstract System.Type GetAttributeType(string attributeId);
}
