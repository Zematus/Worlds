using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NeighborhoodSeaPresenceFactor : Factor
{
    public const string Regex = @"^\s*neighborhood_sea_presence\s*$";

    public NeighborhoodSeaPresenceFactor(Match match)
    {
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return cell.NeighborhoodWaterBiomePresence / TerrainCell.MaxNeighborhoodSeaPresence;
    }

    public override string ToString()
    {
        return "'Neighborhood Sea Presence' Factor";
    }
}
