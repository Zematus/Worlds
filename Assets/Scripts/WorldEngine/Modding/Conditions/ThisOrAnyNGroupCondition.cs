using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ThisOrAnyNGroupCondition : GroupCondition
{
    public Condition Condition;

    public ThisOrAnyNGroupCondition(string conditionStr)
    {
        Condition = BuildCondition(conditionStr);
    }

    public override bool Evaluate(CellGroup group)
    {
        if (Condition.Evaluate(group))
            return true;

        foreach (CellGroup nGroup in group.NeighborGroups)
        {
            if (Condition.Evaluate(nGroup))
                return true;
        }

        return false;
    }

    public override string ToString()
    {
        return "THIS_OR_ANY_N_GROUP (" + Condition.ToString() + ")";
    }
}
