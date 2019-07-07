using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AnyNGroupCondition : GroupCondition
{
    public Condition Condition;

    public AnyNGroupCondition(string conditionStr)
    {
        Condition = BuildCondition(conditionStr);
    }

    public override bool Evaluate(CellGroup group)
    {
        foreach (CellGroup nGroup in group.NeighborGroups)
        {
            if (Condition.Evaluate(nGroup))
                return true;
        }

        return false;
    }

    public override string ToString()
    {
        return "ANY_N_GROUP (" + Condition.ToString() + ")";
    }
}
