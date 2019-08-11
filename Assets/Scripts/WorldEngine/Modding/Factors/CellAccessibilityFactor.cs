using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellAccessibilityFactor : Factor
{
    public const string Regex = @"^\s*cell_accessibility\s*$";

    public CellAccessibilityFactor(Match match)
    {
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return cell.BaseAccessibility;
    }

    public override string ToString()
    {
        return "'Cell Accessibility' Factor";
    }
}
