using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellSurvivabilityFactor : Factor
{
    public const string Regex = @"^\s*cell_survivability\s*$";

    public CellSurvivabilityFactor(Match match)
    {
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return cell.Survivability;
    }

    public override string ToString()
    {
        return "'Cell Survivability' Factor";
    }
}
