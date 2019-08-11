using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellHillinessFactor : Factor
{
    public const string Regex = @"^\s*cell_hilliness\s*$";

    public CellHillinessFactor(Match match)
    {
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return cell.Hilliness;
    }

    public override string ToString()
    {
        return "'Cell Hilliness' Factor";
    }
}
