using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupHasKnowledgeCondition : GroupCondition
{
    public const int DefaultMinValue = 1;

    public const string Regex = @"^\s*group_has_knowledge\s*" +
        @":\s*(?<id>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @"(?:,\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*)?$";

    public string KnowledgeId;
    public int MinValue;

    public GroupHasKnowledgeCondition(Match match)
    {
        KnowledgeId = match.Groups["id"].Value;

        if (!string.IsNullOrEmpty(match.Groups["value"].Value))
        {
            string valueStr = match.Groups["value"].Value;
            float value;

            if (!float.TryParse(valueStr, out value))
            {
                throw new System.ArgumentException("GroupHasKnowledgeCondition: Min value can't be parsed into a valid floating point number: " + valueStr);
            }

            if (!value.IsInsideRange(0.01f, CulturalKnowledge.ScaledMaxLimitValue))
            {
                throw new System.ArgumentException("GroupHasKnowledgeCondition: Min value is outside the range of 0.01 and " + CulturalKnowledge.ScaledMaxLimitValue + ": " + valueStr);
            }

            MinValue = (int)(value / MathUtility.IntToFloatScalingFactor);
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
        return "'Group Has Knowledge' Condition, Knowledge Id: " + KnowledgeId + 
            ", Min Value: " + (MinValue * MathUtility.IntToFloatScalingFactor);
    }
}
