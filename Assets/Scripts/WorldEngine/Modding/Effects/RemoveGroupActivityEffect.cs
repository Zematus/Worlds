using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RemoveGroupActivityEffect : GroupEffect
{
    public const string Regex = @"^\s*remove_group_activity\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<id>" + ModUtility.IdentifierRegexPart + @")\s*$";

    public string ActivityId;

    public RemoveGroupActivityEffect(Match match, string id) :
        base(match.Groups["type"].Value, id)
    {
        ActivityId = match.Groups["id"].Value;
    }

    public override void ApplyToTarget(CellGroup group)
    {
        group.Culture.AddActivityToStop(ActivityId);
    }

    public override string ToString()
    {
        return "'Remove Group Activity' Effect, Target Type " + TargetType + ", Activity Id: " + ActivityId;
    }
}
