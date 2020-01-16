using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellRainfallCondition : CellCondition
{
    public const string Regex = @"^\s*cell_rainfall\s*" +
        @":\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";

    public float MinValue;

    public CellRainfallCondition(Match match)
    {
        string valueStr = match.Groups["value"].Value;

        if (!MathUtility.TryParseCultureInvariant(valueStr, out MinValue))
        {
            throw new System.ArgumentException("CellRainfallCondition: Min value can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!MinValue.IsInsideRange(0, World.MaxPossibleRainfall))
        {
            throw new System.ArgumentException("CellRainfallCondition: Min value is outside the range of " + 0 + " and " + World.MaxPossibleRainfall  + ": " + valueStr);
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return cell.Rainfall >= MinValue;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return null;
    }

    public override string ToString()
    {
        return "'Cell Rainfall' Condition, Min Value: " + MinValue;
    }
}
