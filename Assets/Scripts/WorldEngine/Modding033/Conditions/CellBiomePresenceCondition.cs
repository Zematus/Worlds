using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellBiomePresenceCondition : CellCondition
{
    public const float DefaultMinValue = 0.01f;
    
    public const string Regex = @"^\s*cell_biome_presence\s*" +
        @":\s*(?<id>" + ModUtility033.IdentifierRegexPart + @")\s*" +
        @"(?:,\s*(?<value>" + ModUtility033.NumberRegexPart + @")\s*)?$";

    private string _biomeId;

    public float MinValue;

    public CellBiomePresenceCondition(Match match)
    {
        _biomeId = match.Groups["id"].Value;

        if (!Biome.Biomes.ContainsKey(_biomeId))
        {
            throw new System.ArgumentException("CellBiomePresenceCondition: Unable to find biome with id: " + _biomeId);
        }

        if (!string.IsNullOrEmpty(match.Groups["value"].Value))
        {
            string valueStr = match.Groups["value"].Value;

            if (!MathUtility.TryParseCultureInvariant(valueStr, out MinValue))
            {
                throw new System.ArgumentException("CellBiomePresenceCondition: Min value can't be parsed into a valid floating point number: " + valueStr);
            }

            if (!MinValue.IsInsideRange(DefaultMinValue, 1))
            {
                throw new System.ArgumentException("CellBiomePresenceCondition: Min value is outside the range of " + DefaultMinValue + " and 1: " + valueStr);
            }
        }
        else
        {
            MinValue = DefaultMinValue;
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return cell.GetBiomePresence(_biomeId) >= MinValue;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return null;
    }

    public override string ToString()
    {
        return "'Cell Biome Pesence' Condition, Biome Id: " + _biomeId + ", Min Value: " + MinValue;
    }
}
