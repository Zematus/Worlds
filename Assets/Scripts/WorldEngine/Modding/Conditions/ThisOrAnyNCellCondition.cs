using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ThisOrAnyNCellCondition : CellCondition
{
    public Condition Condition;

    public ThisOrAnyNCellCondition(string conditionStr)
    {
        Condition = BuildCondition(conditionStr);
    }

    public override bool Evaluate(TerrainCell cell)
    {
        if (Condition.Evaluate(cell))
            return true;

        foreach (TerrainCell nCell in cell.Neighbors.Values)
        {
            if (Condition.Evaluate(nCell))
                return true;
        }

        return false;
    }

    public override string ToString()
    {
        return "THIS_OR_ANY_N_CELL (" + Condition.ToString() + ")";
    }
}
