using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellFlowingWaterFactor : Factor
{
    public const float MaxPossibleValue = 100000f;
    public const float MinPossibleValue = 10f;

    public const string Regex = @"^\s*cell_flowing_water\s*" +
        @":\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";

    public float MaxValue;

    public CellFlowingWaterFactor(Match match)
    {
        string valueStr = match.Groups["value"].Value;

        if (!float.TryParse(valueStr, out MaxValue))
        {
            throw new System.ArgumentException("CellFlowingWaterFactor: Max value can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!MaxValue.IsInsideRange(MinPossibleValue, MaxPossibleValue))
        {
            throw new System.ArgumentException("CellFlowingWaterFactor: Max value is outside the range of " + MinPossibleValue + " and " + MaxPossibleValue + ": " + valueStr);
        }
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return Mathf.Clamp01(cell.FlowingWater / MaxValue);
    }

    public override string ToString()
    {
        return "'Cell Flowing Water' Factor";
    }
}
