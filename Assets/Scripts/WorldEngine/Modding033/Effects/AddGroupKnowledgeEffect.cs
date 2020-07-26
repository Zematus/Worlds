using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AddGroupKnowledgeEffect : Effect
{
    private const int _defaultLimitLevel = 100;

    public const string Regex = @"^\s*add_group_knowledge\s*" +
        @":\s*(?<id>" + ModUtility033.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility033.NumberRegexPart + @")\s*$";

    public string KnowledgeId;
    public int LimitLevel;

    public AddGroupKnowledgeEffect(Match match, string id) :
        base(id)
    {
        KnowledgeId = match.Groups["id"].Value;
        
        string valueStr = match.Groups["value"].Value;
        float value;

        if (!MathUtility.TryParseCultureInvariant(valueStr, out value))
        {
            throw new System.ArgumentException("AddGroupKnowledgeEffect: Level limit can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!value.IsInsideRange(1, 10000))
        {
            throw new System.ArgumentException("AddGroupKnowledgeEffect: Level limit is outside the range of 1 and 10000: " + valueStr);
        }

        LimitLevel = (int)(value / MathUtility.IntToFloatScalingFactor);
    }

    public override void Apply(CellGroup group)
    {
        group.Culture.TryAddKnowledgeToLearn(KnowledgeId, _defaultLimitLevel, LimitLevel);
    }

    public override string ToString()
    {
        return "'Add Group Knowledge' Effect, Knowledge Id: " + KnowledgeId + 
            ", Level Limit: " + (LimitLevel * MathUtility.IntToFloatScalingFactor);
    }

    public override bool IsDeferred()
    {
        return false;
    }
}
