using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class CellCondition : Condition
{
    public CellCondition()
    {
        ConditionType = ConditionType.TerrainCell;
    }

    public override bool Evaluate(CellGroup group)
    {
        return Evaluate(group.Cell);
    }
}
