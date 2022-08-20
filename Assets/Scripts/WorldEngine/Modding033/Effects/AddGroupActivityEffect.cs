using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AddGroupActivityEffect : Effect
{
    public const string Regex = @"^\s*add_group_activity\s*" +
        @":\s*(?<id>" + ModUtility033.IdentifierRegexPart + @")\s*$";

    public string ActivityId;

    public AddGroupActivityEffect(Match match, string id) :
        base(id)
    {
        ActivityId = match.Groups["id"].Value;
    }

    public override void Apply(CellGroup group)
    {
        group.Culture.AddActivityToPerform(ActivityId);
    }

    public override bool IsDeferred()
    {
        return false;
    }

    public override string ToString()
    {
        return "'Add Group Activity' Effect, Activity Id: " + ActivityId;
    }
}
