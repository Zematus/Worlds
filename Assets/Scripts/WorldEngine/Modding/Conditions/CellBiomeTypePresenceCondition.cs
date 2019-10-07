using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellBiomeTypePresenceCondition : CellCondition
{
    public const float DefaultMinValue = 0.01f;
    
    public const string Regex = @"^\s*cell_biome_type_presence\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @"(?:,\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*)?$";

    public BiomeTerrainType TerrainType;
    public float MinValue;

    public CellBiomeTypePresenceCondition(Match match)
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

        if (!string.IsNullOrEmpty(match.Groups["value"].Value))
        {
            string valueStr = match.Groups["value"].Value;

            if (!float.TryParse(valueStr, out MinValue))
            {
                throw new System.ArgumentException("CellBiomeTypePresenceCondition: Min value can't be parsed into a valid floating point number: " + valueStr);
            }

            if (!MinValue.IsInsideRange(DefaultMinValue, CulturalKnowledge.ScaledMaxLimitValue))
            {
                throw new System.ArgumentException("CellBiomeTypePresenceCondition: Min value is outside the range of " + DefaultMinValue + " and 1: " + valueStr);
            }
        }
        else
        {
            MinValue = DefaultMinValue;
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return cell.GetBiomeTypePresence(TerrainType) >= MinValue;
    }

    public override string ToString()
    {
        return "'Cell Biome Type Presence' Condition, Terrain Type: " + TerrainType + ", Min Value: " + MinValue;
    }
}
