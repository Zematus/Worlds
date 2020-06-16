using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellArabilityFactor : Factor
{
    public const string Regex = @"^\s*cell_arability\s*$";

    public CellArabilityFactor(Match match)
    {
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return cell.BaseArability;
    }

    public override string ToString()
    {
        return "'Cell Arability' Factor";
    }
}
