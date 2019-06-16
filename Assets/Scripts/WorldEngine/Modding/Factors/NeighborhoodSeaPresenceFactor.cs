using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NeighborhoodSeaPresenceFactor : Factor
{
    public const float MaxSeaPresence = 9;

    public const string Regex = @"^\s*neighborhood_sea_presence\s*$";

    public string KnowledgeId;
    public int InitialValue;

    public NeighborhoodSeaPresenceFactor(Match match)
    {
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return cell.NeighborhoodSeaBiomePresence / MaxSeaPresence;
    }

    public override string ToString()
    {
        return "'Neighborhood Sea Presence' Factor";
    }
}
