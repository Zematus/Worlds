using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellTemperatureCondition : CellCondition
{
    public const string Regex = @"^\s*cell_temperature\s*" +
        @":\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";

    public float MinValue;

    public CellTemperatureCondition(Match match)
    {
        string valueStr = match.Groups["value"].Value;

        if (!MathUtility.TryParseCultureInvariant(valueStr, out MinValue))
        {
            throw new System.ArgumentException("CellTemperatureCondition: Min value can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!MinValue.IsInsideRange(World.MinPossibleTemperature, World.MaxPossibleTemperature))
        {
            throw new System.ArgumentException("CellTemperatureCondition: Min value is outside the range of " + World.MinPossibleTemperature + " and " + World.MaxPossibleTemperature + ": " + valueStr);
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return cell.Temperature >= MinValue;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return null;
    }

    public override string ToString()
    {
        return "'Cell Temperature' Condition, Min Value: " + MinValue;
    }
}
