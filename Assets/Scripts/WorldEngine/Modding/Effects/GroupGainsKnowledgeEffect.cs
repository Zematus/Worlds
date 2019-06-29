using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupGainsKnowledgeEffect : GroupEffect
{
    public const string Regex = @"^\s*group_gains_knowledge\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<id>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";

    public string KnowledgeId;
    public int LevelAsymptote;

    public GroupGainsKnowledgeEffect(Match match) :
        base(match.Groups["type"].Value)
    {
        KnowledgeId = match.Groups["id"].Value;
        
        string valueStr = match.Groups["value"].Value;
        float value;

        if (!float.TryParse(valueStr, out value))
        {
            throw new System.ArgumentException("GroupGainsKnowledgeEffect: Level asymptote can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!value.IsInsideRange(1, 10000))
        {
            throw new System.ArgumentException("GroupGainsKnowledgeEffect: Level asymptote is outside the range of 1 and 10000: " + valueStr);
        }

        LevelAsymptote = (int)(value / CulturalKnowledge.ValueScaleFactor);
    }

    public override void ApplyToTarget(CellGroup group)
    {
        group.Culture.TryAddKnowledgeToLearn(KnowledgeId, group, 1);
    }

    public override string ToString()
    {
        return "'Group Gains Knowledge' Effect, Knowledge Id: " + KnowledgeId + 
            ", Start Value: " + (LevelAsymptote * CulturalKnowledge.ValueScaleFactor);
    }
}
