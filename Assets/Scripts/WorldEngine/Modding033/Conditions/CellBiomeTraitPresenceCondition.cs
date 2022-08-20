using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellBiomeTraitPresenceCondition : CellCondition
{
    public const float DefaultMinValue = 0.01f;

    public const string Regex = @"^\s*cell_biome_trait_presence\s*" +
        @":\s*(?<trait>" + ModUtility033.IdentifierRegexPart + @")\s*" +
        @"(?:,\s*(?<value>" + ModUtility033.NumberRegexPart + @")\s*)?$";

    public string Trait;
    public float MinValue;

    public CellBiomeTraitPresenceCondition(Match match)
    {
        Trait = match.Groups["trait"].Value;

        if (!string.IsNullOrEmpty(match.Groups["value"].Value))
        {
            string valueStr = match.Groups["value"].Value;

            if (!MathUtility.TryParseCultureInvariant(valueStr, out MinValue))
            {
                throw new System.ArgumentException($"CellBiomeTraitPresenceCondition: Min value can't be parsed into a valid floating point number: {valueStr}");
            }

            if (!MinValue.IsInsideRange(DefaultMinValue, 1))
            {
                throw new System.ArgumentException($"CellBiomeTraitPresenceCondition: Min value is outside the range of {DefaultMinValue} and 1: {valueStr}");
            }
        }
        else
        {
            MinValue = DefaultMinValue;
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return cell.GetBiomeTraitPresence(Trait) >= MinValue;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return null;
    }

    public override string ToString()
    {
        return $"'Cell Biome Trait Presence' Condition, Trait: {Trait}, Min Value: {MinValue}";
    }
}
