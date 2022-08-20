using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellBiomeTypePresenceFactor : Factor
{
    public const string Regex = @"^\s*cell_biome_type_presence\s*" +
        @":\s*(?<type>" + ModUtility033.IdentifierRegexPart + @")\s*$";

    public BiomeTerrainType TerrainType;

    public CellBiomeTypePresenceFactor(Match match)
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
        return cell.GetBiomeTypePresence(TerrainType);
    }

    public override string ToString()
    {
        return "'Cell Biome Type Presence' Factor, Type: " + TerrainType;
    }
}
