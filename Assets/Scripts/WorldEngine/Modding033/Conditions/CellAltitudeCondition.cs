using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellAltitudeCondition : CellCondition
{
    public const string Regex = @"^\s*cell_altitude\s*" +
        @":\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";

    public float MinValue;

    public CellAltitudeCondition(Match match)
    {
        string valueStr = match.Groups["value"].Value;

        if (!MathUtility.TryParseCultureInvariant(valueStr, out MinValue))
        {
            throw new System.ArgumentException("CellAltitudeCondition: Min value can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!MinValue.IsInsideRange(World.MinPossibleAltitude, World.MaxPossibleAltitude))
        {
            throw new System.ArgumentException("CellAltitudeCondition: Min value is outside the range of " + World.MinPossibleAltitude + " and " + World.MaxPossibleAltitude  + ": " + valueStr);
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return cell.Altitude >= MinValue;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return null;
    }

    public override string ToString()
    {
        return "'Cell Altitude' Condition, Min Value: " + MinValue;
    }
}
