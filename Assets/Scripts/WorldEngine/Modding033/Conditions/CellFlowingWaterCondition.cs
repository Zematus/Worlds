using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellFlowingWaterCondition : CellCondition
{
    public const float MaxPossibleValue = 1000000f;
    public const float MinPossibleValue = 1f;

    public const string Regex = @"^\s*cell_flowing_water\s*" +
        @":\s*(?<value>" + ModUtility033.NumberRegexPart + @")\s*$";

    public float MinValue;

    public CellFlowingWaterCondition(Match match)
    {
        string valueStr = match.Groups["value"].Value;

        if (!MathUtility.TryParseCultureInvariant(valueStr, out MinValue))
        {
            throw new System.ArgumentException("CellFlowingWaterCondition: Min value can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!MinValue.IsInsideRange(MinPossibleValue, MaxPossibleValue))
        {
            throw new System.ArgumentException("CellFlowingWaterCondition: Min value is outside the range of " + MinPossibleValue + " and " + MaxPossibleValue + ": " + valueStr);
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return cell.FlowingWater >= MinValue;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return null;
    }

    public override string ToString()
    {
        return "'Cell Flowing Water' Condition, Min Value: " + MinValue;
    }
}
