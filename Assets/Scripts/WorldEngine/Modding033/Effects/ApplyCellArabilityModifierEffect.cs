﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ApplyCellArabilityModifierEffect : Effect
{
    public const string Regex = @"^\s*apply_cell_arability_modifier\s*" +
        @":\s*(?<value>" + ModUtility033.NumberRegexPart + @")\s*$";
    
    public float ArabilityDelta;

    public ApplyCellArabilityModifierEffect(Match match, string id) :
        base(id)
    {
        string valueStr = match.Groups["value"].Value;

        if (!MathUtility.TryParseCultureInvariant(valueStr, out float value))
        {
            throw new System.ArgumentException(
                $"ApplyCellArabilityModifierEffect: Arability modifier can't be parsed into a valid floating point number: {valueStr}");
        }

        if (!value.IsInsideRange(-1, 1))
        {
            throw new System.ArgumentException(
                $"ApplyCellArabilityModifierEffect: Arability modifer is outside the range of -1 and 1: {valueStr}");
        }

        ArabilityDelta = value;
    }

    public override void Apply(CellGroup group) => group.ApplyArabilityModifier(ArabilityDelta);

    public override bool IsDeferred() => false;

    public override string ToString() => $"'Apply Cell Arability Modifier' Effect, Value: {ArabilityDelta}";
}
