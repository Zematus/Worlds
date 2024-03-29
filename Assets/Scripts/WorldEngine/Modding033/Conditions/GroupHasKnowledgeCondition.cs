﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupHasKnowledgeCondition : GroupCondition
{
    public const float DefaultMinValue = 0.01f;

    public const string Regex = @"^\s*group_has_knowledge\s*" +
        @":\s*(?<id>" + ModUtility033.IdentifierRegexPart + @")\s*" +
        @"(?:,\s*(?<value>" + ModUtility033.NumberRegexPart + @")\s*)?$";

    public string KnowledgeId;
    public float MinValue;

    public GroupHasKnowledgeCondition(Match match)
    {
        KnowledgeId = match.Groups["id"].Value;

        if (!string.IsNullOrEmpty(match.Groups["value"].Value))
        {
            string valueStr = match.Groups["value"].Value;
            float value;

            if (!MathUtility.TryParseCultureInvariant(valueStr, out value))
            {
                throw new System.ArgumentException($"GroupHasKnowledgeCondition: Min value can't be parsed into a valid floating point number: {valueStr}");
            }

            if (!value.IsInsideRange(DefaultMinValue, KnowledgeLimit.MaxLimitValue))
            {
                throw new System.ArgumentException(
                    $"GroupHasKnowledgeCondition: Min value is outside the range of {DefaultMinValue} and {KnowledgeLimit.MaxLimitValue}: {valueStr}");
            }

            MinValue = value;
        }
        else
        {
            MinValue = DefaultMinValue;
        }

        ConditionType |= ConditionType.Knowledge;
    }

    public override bool Evaluate(CellGroup group)
    {
        CulturalKnowledge knowledge = group.Culture.GetKnowledge(KnowledgeId);

        if (knowledge != null)
        {
            return knowledge.Value >= MinValue;
        }

        return false;
    }

    public override string GetPropertyValue(string propertyId)
    {
        switch (propertyId)
        {
            case Property_KnowledgeId:
                return KnowledgeId;
        }

        return null;
    }

    public override string ToString()
    {
        return $"'Group Has Knowledge' Condition, Knowledge Id: {KnowledgeId}, Min Value: {MinValue}";
    }
}
