using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NotCondition : Condition
{
    public Condition Condition;

    public override bool Evaluate(CellGroup group)
    {
        return !Condition.Evaluate(group);
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return !Condition.Evaluate(cell);
    }
}
