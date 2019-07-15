using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ModifyGroupKnowledgeLimitEffect : GroupEffect
{
    private const int _initialValue = 100;

    public const string Regex = @"^\s*modify_group_knowledge_limit\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<id>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";

    public string KnowledgeId;
    public int LevelLimitDelta;

    public ModifyGroupKnowledgeLimitEffect(Match match, string id) :
        base(match.Groups["type"].Value, id)
    {
        KnowledgeId = match.Groups["id"].Value;
        
        string valueStr = match.Groups["value"].Value;
        float value;

        if (!float.TryParse(valueStr, out value))
        {
            throw new System.ArgumentException("ModifyGroupKnowledgeLimitEffect: Level limit delta can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!value.IsInsideRange(-CulturalKnowledge.ScaledMaxLevelValue, CulturalKnowledge.ScaledMaxLevelValue))
        {
            throw new System.ArgumentException(
                "ModifyGroupKnowledgeLimitEffect: Level limit delta is outside the range of " + 
                (-CulturalKnowledge.ScaledMaxLevelValue) + " and " + CulturalKnowledge.ScaledMaxLevelValue + ": " + valueStr);
        }

        LevelLimitDelta = (int)(value / CulturalKnowledge.ValueScaleFactor);
    }

    public override void ApplyToTarget(CellGroup group)
    {
        CellCulturalKnowledge k = group.Culture.GetKnowledge(KnowledgeId) as CellCulturalKnowledge;

        if (k == null)
        {
            throw new System.ArgumentException("ModifyGroupKnowledgeLimitEffect: Target group doesn't have knowledge: " + KnowledgeId + ", Group Position: " + group.Position);
        }

        k.ModifyLevelLimit(LevelLimitDelta);
    }

    public override string ToString()
    {
        return "'Increase Group Knowledge Limit' Effect, Knowledge Id: " + KnowledgeId + 
            ", Level Limit Increase: " + (LevelLimitDelta * CulturalKnowledge.ValueScaleFactor);
    }
}
