using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class CellCondition : Condition
{
    public enum CellType
    {
        This,
        Neighbor,
        ThisOrNeighbor
    }

    public CellType TargetType;

    public override bool Evaluate(CellGroup group)
    {
        return Evaluate(group.Cell);
    }

    public override bool Evaluate(TerrainCell cell)
    {
        switch (TargetType)
        {
            case CellType.This:
                return EvaluateTarget(cell);

            case CellType.Neighbor:
                foreach (TerrainCell nCell in cell.Neighbors.Values)
                {
                    if (EvaluateTarget(nCell))
                        return true;
                }

                return false;

            case CellType.ThisOrNeighbor:
                if (EvaluateTarget(cell))
                {
                    return true;
                }

                foreach (TerrainCell nCell in cell.Neighbors.Values)
                {
                    if (EvaluateTarget(nCell))
                        return true;
                }

                return false;

            default:
                throw new System.Exception("Unhandled CellType: " + TargetType);
        }
    }

    protected abstract bool EvaluateTarget(TerrainCell targetCell);
}
