using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Entity
{
    readonly public Dictionary<string, EntityAtribute> Attributes = new Dictionary<string, EntityAtribute>();
}
