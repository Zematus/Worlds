using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ModifyGroupKnowledgeLimitEffect : Effect
{
    private const int _initialValue = 100;

    public const string Regex = @"^\s*modify_group_knowledge_limit\s*" +
        @":\s*(?<id>" + ModUtility033.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility033.NumberRegexPart + @")\s*$";

    public string KnowledgeId;
    public int LevelLimitDelta;

    public ModifyGroupKnowledgeLimitEffect(Match match, string id) :
        base(id)
    {
        KnowledgeId = match.Groups["id"].Value;
        
        string valueStr = match.Groups["value"].Value;
        float value;

        if (!MathUtility.TryParseCultureInvariant(valueStr, out value))
        {
            throw new System.ArgumentException("ModifyGroupKnowledgeLimitEffect: Level limit modifier can't be parsed into a valid floating point number: " + valueStr);
        }

        if (!value.IsInsideRange(-CulturalKnowledge.ScaledMaxLimitValue, CulturalKnowledge.ScaledMaxLimitValue))
        {
            throw new System.ArgumentException(
                "ModifyGroupKnowledgeLimitEffect: Level limit modifier is outside the range of " + 
                (-CulturalKnowledge.ScaledMaxLimitValue) + " and " + CulturalKnowledge.ScaledMaxLimitValue + ": " + valueStr);
        }

        LevelLimitDelta = (int)(value * MathUtility.FloatToIntScalingFactor);
    }

    public override void Apply(CellGroup group)
    {
        CellCulturalKnowledge k = group.Culture.GetKnowledge(KnowledgeId) as CellCulturalKnowledge;

        if (k == null)
            return;

        k.ModifyLevelLimit(LevelLimitDelta);
    }

    public override string ToString()
    {
        return "'Modify Group Knowledge Limit' Effect, Knowledge Id: " + KnowledgeId + 
            ", Level Limit Modifier: " + (LevelLimitDelta * MathUtility.IntToFloatScalingFactor);
    }

    public override bool IsDeferred()
    {
        return true;
    }
}
