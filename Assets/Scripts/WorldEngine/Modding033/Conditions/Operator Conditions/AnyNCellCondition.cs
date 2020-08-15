using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AnyNCellCondition : UnaryOpCellCondition
{
    public AnyNCellCondition(string conditionStr) : base(conditionStr)
    {
    }

    public override bool Evaluate(TerrainCell cell)
    {
        foreach (TerrainCell nCell in cell.NeighborList)
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
