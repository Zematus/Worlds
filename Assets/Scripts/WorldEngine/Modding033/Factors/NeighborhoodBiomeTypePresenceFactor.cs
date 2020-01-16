using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NeighborhoodBiomeTypePresenceFactor : Factor
{
    public const string Regex = @"^\s*neighborhood_biome_type_presence\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*$";

    public BiomeTerrainType TerrainType;

    public NeighborhoodBiomeTypePresenceFactor(Match match)
    {
        string type = match.Groups["type"].Value;

        switch (type)
        {
            case "land":
                TerrainType = BiomeTerrainType.Land;
                break;
            case "water":
                TerrainType = BiomeTerrainType.Water;
                break;
            case "ice":
                TerrainType = BiomeTerrainType.Ice;
                break;
            default:
                throw new System.ArgumentException("Unknown biome terrain type: " + type);
        }
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return cell.GetNeighborhoodBiomeTypePresence(TerrainType) / TerrainCell.MaxNeighborhoodCellCount;
    }

    public override string ToString()
    {
        return "'Neighborhood Biome Type Presence' Factor, Type: " + TerrainType;
    }
}
