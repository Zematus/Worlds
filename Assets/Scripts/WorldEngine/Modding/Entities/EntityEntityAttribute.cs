using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EntityEntityAttribute : EntityAttribute
{
    public abstract Entity GetValue();
    public abstract System.Type GetAttributeType(string attributeId);
}
