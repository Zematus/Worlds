using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellWoodCoverageCondition : CellCondition
{
    public const float DefaultMinValue = 0.01f;
    
    public const string Regex = @"^\s*cell_wood_coverage\s*" +
        @"(?::\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*)?$";

    public float MinValue;

    public CellWoodCoverageCondition(Match match)
    {
        if (!string.IsNullOrEmpty(match.Groups["value"].Value))
        {
            string valueStr = match.Groups["value"].Value;

            if (!float.TryParse(valueStr, out MinValue))
            {
                throw new System.ArgumentException("CellWoodCoverageCondition: Min value can't be parsed into a valid floating point number: " + valueStr);
            }

            if (!MinValue.IsInsideRange(0, 1))
            {
                throw new System.ArgumentException("CellWoodCoverageCondition: Min value is outside the range of 0 and 1: " + valueStr);
            }
        }
        else
        {
            MinValue = DefaultMinValue;
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return cell.WoodCoverage >= MinValue;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return null;
    }

    public override string ToString()
    {
        return "'Cell Wood Coverage' Condition, Min Value: " + MinValue;
    }
}
