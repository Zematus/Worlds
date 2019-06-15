using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellIsSeaCondition : CellCondition
{
    public const string Regex = @"^\s*cell_is_sea\s*:\s*(?<type>\S+)\s*$";

    public CellIsSeaCondition(Match match) : 
        this(match.Groups["type"].Value)
    {
    }

    public CellIsSeaCondition(string typeStr) : base(typeStr)
    {
    }

    protected override bool EvaluateTarget(TerrainCell targetCell)
    {
        return targetCell.SeaBiomePresence > 0;
    }

    public override string ToString()
    {
        return "Cell Is Sea Condition, Target Type: " + TargetType;
    }
}
