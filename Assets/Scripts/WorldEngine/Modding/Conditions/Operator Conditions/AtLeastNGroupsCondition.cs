using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AtLeastNGroupsCondition : UnaryOpGroupCondition
{
    private int _minQuantity;

    public AtLeastNGroupsCondition(string conditionStr, string minQuantityStr) : base(conditionStr)
    {
        if (!int.TryParse(minQuantityStr, out _minQuantity))
        {
            throw new System.ArgumentException("AtLeastNGroupsCondition: Unparseable integer parameter input: " + minQuantityStr);
        }

        if (!_minQuantity.IsInsideRange(0, 8))
        {
            throw new System.ArgumentException("AtLeastNGroupsCondition: parameter input outside of range (0, 8): " + minQuantityStr);
        }
    }

    public override bool Evaluate(CellGroup group)
    {
        int count = 0;

        foreach (CellGroup nGroup in group.NeighborGroups)
        {
            if (Condition.Evaluate(nGroup))
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
        return "AT_LEAST_N_GROUPS:" + _minQuantity + " (" + Condition.ToString() + ")";
    }
}
