using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ApplyCellAccessibilityModifierEffect : Effect
{
    public const string Regex = @"^\s*apply_cell_accessibility_modifier\s*" +
        @":\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";

    public int AccessibilityDelta;

    public ApplyCellAccessibilityModifierEffect(Match match, string id) :
        base(id)
    {
        string valueStr = match.Groups["value"].Value;
        float value;

        if (!float.TryParse(valueStr, out value))
        {
            throw new System.ArgumentException("ApplyCellAccessibilityModifierEffect: Accessibility modifier can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!value.IsInsideRange(-1, 1))
        {
            throw new System.ArgumentException(
                "ApplyCellAccessibilityModifierEffect: Accessibility modifer is outside the range of " +
                -1 + " and " + 1 + ": " + valueStr);
        }

        AccessibilityDelta = (int)(value * MathUtility.FloatToIntScalingFactor);
    }

    public override void Apply(CellGroup group)
    {
        group.ApplyAccessibilityModifier(AccessibilityDelta);
    }

    public override bool IsDeferred()
    {
        return false;
    }

    public override string ToString()
    {
        return "'Apply Cell Accessibility Modifier' Effect, Value: " + (AccessibilityDelta * MathUtility.IntToFloatScalingFactor);
    }
}
