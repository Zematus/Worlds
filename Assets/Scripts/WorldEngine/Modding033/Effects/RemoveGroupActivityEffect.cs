using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RemoveGroupActivityEffect : Effect
{
    public const string Regex = @"^\s*remove_group_activity\s*" +
        @":\s*(?<id>" + ModUtility.IdentifierRegexPart + @")\s*$";

    public string ActivityId;

    public RemoveGroupActivityEffect(Match match, string id) :
        base(id)
    {
        ActivityId = match.Groups["id"].Value;
    }

    public override void Apply(CellGroup group)
    {
        group.Culture.AddActivityToStop(ActivityId);
    }

    public override bool IsDeferred()
    {
        return false;
    }

    public override string ToString()
    {
        return "'Remove Group Activity' Effect, Activity Id: " + ActivityId;
    }
}
