using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ApplyGroupNavigationRangeModifierEffect : Effect
{
    public const string Regex = @"^\s*apply_group_navigation_range_modifier\s*" +
        @":\s*(?<value>" + ModUtility033.NumberRegexPart + @")\s*$";
    
    public int RangeDelta;

    public ApplyGroupNavigationRangeModifierEffect(Match match, string id) :
        base(id)
    {
        string valueStr = match.Groups["value"].Value;
        float value;

        if (!MathUtility.TryParseCultureInvariant(valueStr, out value))
        {
            throw new System.ArgumentException("ApplyGroupNavigationRangeModifierEffect: Navigation range modifier can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!value.IsInsideRange(-1, 1))
        {
            throw new System.ArgumentException(
                "ApplyGroupNavigationRangeModifierEffect: Navigation range modifer is outside the range of " +
                -1 + " and " + 1 + ": " + valueStr);
        }

        RangeDelta = (int)(value * MathUtility.FloatToIntScalingFactor);
    }

    public override void Apply(CellGroup group)
    {
        group.ApplyNavigationRangeModifier(RangeDelta);
    }

    public override bool IsDeferred()
    {
        return false;
    }

    public override string ToString()
    {
        return "'Apply Group Navigation Range Modifier' Effect, Value: " + (RangeDelta * MathUtility.IntToFloatScalingFactor);
    }
}
