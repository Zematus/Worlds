using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ThisAndAllNCellsCondition : CellCondition
{
    public Condition Condition;

    public ThisAndAllNCellsCondition(string conditionStr)
    {
        Condition = BuildCondition(conditionStr);
    }

    public override bool Evaluate(TerrainCell cell)
    {
        if (!Condition.Evaluate(cell))
            return false;

        foreach (TerrainCell nCell in cell.Neighbors.Values)
        {
            if (!Condition.Evaluate(nCell))
                return false;
        }

        return true;
    }

    public override string ToString()
    {
        return "THIS_AND_ALL_N_CELLS (" + Condition.ToString() + ")";
    }
}
