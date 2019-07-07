using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AllNCellsCondition : CellCondition
{
    public Condition Condition;

    public AllNCellsCondition(string conditionStr)
    {
        Condition = BuildCondition(conditionStr);
    }

    public override bool Evaluate(TerrainCell cell)
    {
        foreach (TerrainCell nCell in cell.Neighbors.Values)
        {
            if (!Condition.Evaluate(nCell))
                return false;
        }

        return true;
    }

    public override string ToString()
    {
        return "ALL_N_CELLS (" + Condition.ToString() + ")";
    }
}
