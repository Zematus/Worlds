using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class GroupCondition : Condition
{
    public GroupCondition()
    {
        ConditionType = ConditionType.Group;
    }

    public override bool Evaluate(TerrainCell cell)
    {
        throw new System.Exception("Can't target cells using a GroupCondition of type: " + GetType());
    }
}
