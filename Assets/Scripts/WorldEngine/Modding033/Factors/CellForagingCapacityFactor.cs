using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellForagingCapacityFactor : Factor
{
    public const string Regex = @"^\s*cell_foraging_capacity\s*$";

    public CellForagingCapacityFactor(Match match)
    {
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return cell.ForagingCapacity;
    }

    public override string ToString()
    {
        return "'Cell Foraging Capacity' Factor";
    }
}
