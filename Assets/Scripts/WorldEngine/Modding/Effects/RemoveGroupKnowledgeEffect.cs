using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RemoveGroupKnowledgeEffect : GroupEffect
{
    public const string Regex = @"^\s*remove_group_knowledge\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<id>" + ModUtility.IdentifierRegexPart + @")\s*$";

    public string KnowledgeId;

    public RemoveGroupKnowledgeEffect(Match match, string id) :
        base(match.Groups["type"].Value, id)
    {
        KnowledgeId = match.Groups["id"].Value;
    }

    public override void ApplyToTarget(CellGroup group)
    {
        group.Culture.AddKnowledgeToLose(KnowledgeId);
    }

    public override string ToString()
    {
        return "'Add Group Knowledge' Effect, Target Type " + TargetType + ", Knowledge Id: " + KnowledgeId;
    }
}
