using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ApplyGroupNavigationRangeModifierEffect : Effect
{
    public const string Regex = @"^\s*apply_group_navigation_range_modifier\s*" +
        @":\s*(?<value>" + ModUtility033.NumberRegexPart + @")\s*$";
    
    public float RangeDelta;

    public ApplyGroupNavigationRangeModifierEffect(Match match, string id) :
        base(id)
    {
        string valueStr = match.Groups["value"].Value;

        if (!MathUtility.TryParseCultureInvariant(valueStr, out float value))
        {
            throw new System.ArgumentException(
                $"ApplyGroupNavigationRangeModifierEffect: Navigation range modifier can't be parsed into a valid floating point number: {valueStr}");
        }

        if (!value.IsInsideRange(-1, 1))
        {
            throw new System.ArgumentException(
                $"ApplyGroupNavigationRangeModifierEffect: Navigation range modifer is outside the range of -1 and 1: {valueStr}");
        }

        RangeDelta = value;
    }

    public override void Apply(CellGroup group) => group.ApplyNavigationRangeModifier(RangeDelta);

    public override bool IsDeferred() => false;

    public override string ToString() => $"'Apply Group Navigation Range Modifier' Effect, Value: {RangeDelta}";
}
