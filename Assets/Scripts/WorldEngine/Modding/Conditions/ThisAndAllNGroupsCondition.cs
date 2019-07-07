using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ThisAndAllNGroupsCondition : GroupCondition
{
    public Condition Condition;

    public ThisAndAllNGroupsCondition(string conditionStr)
    {
        Condition = BuildCondition(conditionStr);
    }

    public override bool Evaluate(CellGroup group)
    {
        if (!Condition.Evaluate(group))
            return false;

        foreach (CellGroup nGroup in group.NeighborGroups)
        {
            if (!Condition.Evaluate(nGroup))
                return false;
        }

        return true;
    }

    public override string ToString()
    {
        return "THIS_AND_ALL_N_GROUPS (" + Condition.ToString() + ")";
    }
}
