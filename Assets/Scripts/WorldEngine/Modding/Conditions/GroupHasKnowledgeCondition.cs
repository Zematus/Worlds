using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupHasKnowledgeCondition : GroupCondition
{
    public string KnowledgeId; 

    protected override bool EvaluateTarget(CellGroup targetGroup)
    {
        return targetGroup.Culture.HasKnowledge(KnowledgeId);
    }
}
