using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class GroupCondition : Condition
{
    public enum GroupType
    {
        This,
        Neighbor,
        ThisOrNeighbor
    }

    public GroupType TargetType;

    public GroupCondition(string typeStr)
    {
        switch (typeStr.Trim().ToLower())
        {
            case "this":
                TargetType = GroupType.This;
                break;

            case "neighbor":
                TargetType = GroupType.Neighbor;
                break;

            case "thisorneighbor":
                TargetType = GroupType.ThisOrNeighbor;
                break;
        }
    }

    public override bool Evaluate(CellGroup group)
    {
        switch (TargetType)
        {
            case GroupType.This:
                return EvaluateTarget(group);

            case GroupType.Neighbor:
                foreach (CellGroup nGroup in group.NeighborGroups)
                {
                    if (EvaluateTarget(nGroup))
                        return true;
                }

                return false;

            case GroupType.ThisOrNeighbor:
                if (EvaluateTarget(group))
                {
                    return true;
                }

                foreach (CellGroup nGroup in group.NeighborGroups)
                {
                    if (EvaluateTarget(nGroup))
                        return true;
                }

                return false;

            default:
                throw new System.Exception("Unhandled GroupType: " + TargetType);
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        throw new System.Exception("Can't target cells using a GroupCondition of type: " + GetType());
    }

    protected abstract bool EvaluateTarget(CellGroup targetGroup);
}
