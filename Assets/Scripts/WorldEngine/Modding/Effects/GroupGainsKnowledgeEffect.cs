using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupGainsKnowledgeEffect : GroupEffect
{
    public const int DefaultInitialValue = 100;

    public const string Regex = @"^\s*group_gains_knowledge\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<id>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @"(?:,\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*)?$";

    public string KnowledgeId;
    public int InitialValue;

    public GroupGainsKnowledgeEffect(Match match) :
        base(match.Groups["type"].Value)
    {
        KnowledgeId = match.Groups["id"].Value;

        if (!string.IsNullOrEmpty(match.Groups["value"].Value))
        {
            string valueStr = match.Groups["value"].Value;
            float value;

            if (!float.TryParse(valueStr, out value))
            {
                throw new System.ArgumentException("GroupGainsKnowledgeEffect: Min value can't be parsed into a valid floating point number: " + valueStr);
            }

            if (!value.IsInsideRange(1, 10000))
            {
                throw new System.ArgumentException("GroupGainsKnowledgeEffect: Min value is outside the range of 1 and 10000: " + valueStr);
            }

            InitialValue = (int)(value / CulturalKnowledge.ValueScaleFactor);
        }
        else
        {
            InitialValue = DefaultInitialValue;
        }
    }

    public override void ApplyToTarget(CellGroup group)
    {
        group.Culture.TryAddKnowledgeToLearn(KnowledgeId, group, InitialValue);
    }

    public override string ToString()
    {
        return "'Group Gains Knowledge' Effect, Knowledge Id: " + KnowledgeId + 
            ", Start Value: " + (InitialValue * CulturalKnowledge.ValueScaleFactor);
    }
}
