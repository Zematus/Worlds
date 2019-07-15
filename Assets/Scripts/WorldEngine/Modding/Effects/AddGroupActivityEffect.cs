using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AddGroupActivityEffect : GroupEffect
{
    public const string Regex = @"^\s*add_group_activity\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<id>" + ModUtility.IdentifierRegexPart + @")\s*$";

    public string ActivityId;

    public AddGroupActivityEffect(Match match, string id) :
        base(match.Groups["type"].Value, id)
    {
        ActivityId = match.Groups["id"].Value;
    }

    public override void ApplyToTarget(CellGroup group)
    {
        group.Culture.AddActivityToPerform(CellCulturalActivity.CreateActivity(ActivityId, group));
    }

    public override string ToString()
    {
        return "'Add Group Activity' Effect, Activity Id: " + ActivityId;
    }
}
