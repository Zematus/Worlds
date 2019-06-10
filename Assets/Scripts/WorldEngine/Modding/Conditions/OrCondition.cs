using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class OrCondition : Condition
{
    public Condition ConditionA;
    public Condition ConditionB;

    public override bool Evaluate(CellGroup group)
    {
        return ConditionA.Evaluate(group) || ConditionB.Evaluate(group);
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return ConditionA.Evaluate(cell) || ConditionB.Evaluate(cell);
    }
}
