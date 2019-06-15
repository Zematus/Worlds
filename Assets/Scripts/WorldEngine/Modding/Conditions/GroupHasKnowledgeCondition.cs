using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupHasKnowledgeCondition : GroupCondition
{
    public const string Regex = @"^\s*group_has_knowledge\s*:\s*(?<type>\S+)\s*,\s*(?<id>\S+)\s*$";

    public string KnowledgeId;

    public GroupHasKnowledgeCondition(Match match) : 
        this(match.Groups["type"].Value, match.Groups["id"].Value)
    {
    }

    public GroupHasKnowledgeCondition(string typeStr, string knowledgeId) : base(typeStr)
    {
        KnowledgeId = knowledgeId.Trim();
    }

    protected override bool EvaluateTarget(CellGroup targetGroup)
    {
        return targetGroup.Culture.HasKnowledge(KnowledgeId);
    }

    public override string ToString()
    {
        return "Group Has Knowledge Condition, Target Type: " + TargetType + ", Knowledge Id: " + KnowledgeId;
    }
}
