using System.Text.RegularExpressions;

public class ModifyGroupKnowledgeLimitEffect : Effect
{
    private const float _initialValue = 1;

    public const string Regex = @"^\s*modify_group_knowledge_limit\s*" +
        @":\s*(?<id>" + ModUtility033.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility033.NumberRegexPart + @")\s*$";

    public string KnowledgeId;
    public float LevelLimitDelta;

    public ModifyGroupKnowledgeLimitEffect(Match match, string id) :
        base(id)
    {
        KnowledgeId = match.Groups["id"].Value;
        
        string valueStr = match.Groups["value"].Value;
        float value;

        if (!MathUtility.TryParseCultureInvariant(valueStr, out value))
        {
            throw new System.ArgumentException($"ModifyGroupKnowledgeLimitEffect: Level limit modifier can't be parsed into a valid floating point number: {valueStr}");
        }

        if (!value.IsInsideRange(-KnowledgeLimit.MaxLimitValue, KnowledgeLimit.MaxLimitValue))
        {
            throw new System.ArgumentException(
                $"ModifyGroupKnowledgeLimitEffect: Level limit modifier is outside the range " +
                $"of {-KnowledgeLimit.MaxLimitValue} and {KnowledgeLimit.MaxLimitValue}: {valueStr}");
        }

        LevelLimitDelta = value;
    }

    public override void Apply(CellGroup group)
    {
        var k = group.Culture.GetKnowledgeLimit(KnowledgeId);

        k.SetValue(k.Value + LevelLimitDelta);
    }

    public override string ToString()
    {
        return $"'Modify Group Knowledge Limit' Effect, Knowledge Id: {KnowledgeId}, Level Limit Modifier: {LevelLimitDelta}";
    }

    public override bool IsDeferred()
    {
        return true;
    }
}
