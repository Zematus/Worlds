using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AnyNCellCondition : CellCondition
{
    public Condition Condition;

    public AnyNCellCondition(string conditionStr)
    {
        Condition = BuildCondition(conditionStr);
    }

    public override bool Evaluate(TerrainCell cell)
    {
        foreach (TerrainCell nCell in cell.Neighbors.Values)
        {
            if (Condition.Evaluate(nCell))
                return true;
        }

        return false;
    }

    public override string ToString()
    {
        return "ANY_N_CELL (" + Condition.ToString() + ")";
    }
}
