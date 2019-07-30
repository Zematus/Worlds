using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AtLeastNCellsCondition : UnaryOpCellCondition
{
    private int _minQuantity;

    public AtLeastNCellsCondition(string conditionStr, string minQuantityStr) : base(conditionStr)
    {
        if (!int.TryParse(minQuantityStr, out _minQuantity))
        {
            throw new System.ArgumentException("AtLeastNCellsCondition: Unparseable integer parameter input: " + minQuantityStr);
        }

        if (!_minQuantity.IsInsideRange(0, 8))
        {
            throw new System.ArgumentException("AtLeastNCellsCondition: parameter input outside of range (0, 8): " + minQuantityStr);
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        int count = 0;

        foreach (TerrainCell nCell in cell.Neighbors.Values)
        {
            if (Condition.Evaluate(nCell))
            {
                count++;

                if (count >= _minQuantity)
                    return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        return "AT_LEAST_N_CELLS:" + _minQuantity + " (" + Condition.ToString() + ")";
    }
}
