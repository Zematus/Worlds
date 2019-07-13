using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AddGroupKnowledgeEffect : GroupEffect
{
    private const int _initialValue = 100;

    public const string Regex = @"^\s*add_group_knowledge\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<id>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";

    public string KnowledgeId;
    public int LimitLevel;

    public AddGroupKnowledgeEffect(Match match, string id) :
        base(match.Groups["type"].Value, id)
    {
        KnowledgeId = match.Groups["id"].Value;
        
        string valueStr = match.Groups["value"].Value;
        float value;

        if (!float.TryParse(valueStr, out value))
        {
            throw new System.ArgumentException("AddGroupKnowledgeEffect: Level limit can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!value.IsInsideRange(1, 10000))
        {
            throw new System.ArgumentException("AddGroupKnowledgeEffect: Level limit is outside the range of 1 and 10000: " + valueStr);
        }

        LimitLevel = (int)(value / CulturalKnowledge.ValueScaleFactor);
    }

    public override void ApplyToTarget(CellGroup group)
    {
        group.Culture.TryAddKnowledgeToLearn(KnowledgeId, group, _initialValue, LimitLevel);
    }

    public override string ToString()
    {
        return "'Add Group Knowledge' Effect, Knowledge Id: " + KnowledgeId + 
            ", Level Limit: " + (LimitLevel * CulturalKnowledge.ValueScaleFactor);
    }
}
