using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupGainsKnowledgeEffect : GroupEffect
{
    private const int _initialValue = 100;

    public const string Regex = @"^\s*group_gains_knowledge\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<id>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";

    public string KnowledgeId;
    public int AsymptoteLevel;

    public GroupGainsKnowledgeEffect(Match match, string id) :
        base(match.Groups["type"].Value, id)
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

        AsymptoteLevel = (int)(value / CulturalKnowledge.ValueScaleFactor);

        World.KnowledgeAsymptoteLevels[id] = AsymptoteLevel;
    }

    public override void ApplyToTarget(CellGroup group)
    {
        CellCulturalKnowledge k = group.Culture.TryAddKnowledgeToLearn(KnowledgeId, group, _initialValue);

        k.AddAsymptoteLevel(Id, AsymptoteLevel);
    }

    public override string ToString()
    {
        return "'Group Gains Knowledge' Effect, Knowledge Id: " + KnowledgeId + 
            ", Start Value: " + (AsymptoteLevel * CulturalKnowledge.ValueScaleFactor);
    }
}
