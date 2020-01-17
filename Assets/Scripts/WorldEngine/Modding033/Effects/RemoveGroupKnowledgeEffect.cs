using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RemoveGroupKnowledgeEffect : Effect
{
    public const string Regex = @"^\s*remove_group_knowledge\s*" +
        @":\s*(?<id>" + ModUtility033.IdentifierRegexPart + @")\s*$";

    public string KnowledgeId;

    public RemoveGroupKnowledgeEffect(Match match, string id) :
        base(id)
    {
        KnowledgeId = match.Groups["id"].Value;
    }

    public override void Apply(CellGroup group)
    {
        group.Culture.AddKnowledgeToLose(KnowledgeId);
    }

    public override bool IsDeferred()
    {
        return false;
    }

    public override string ToString()
    {
        return "'Add Group Knowledge' Effect, Knowledge Id: " + KnowledgeId;
    }
}
