using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AllNCellsCondition : UnaryOpCellCondition
{
    public AllNCellsCondition(string conditionStr) : base(conditionStr)
    {
    }

    public override bool Evaluate(TerrainCell cell)
    {
        foreach (TerrainCell nCell in cell.NeighborList)
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
