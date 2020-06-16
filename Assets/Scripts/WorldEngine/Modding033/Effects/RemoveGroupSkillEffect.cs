using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RemoveGroupSkillEffect : Effect
{
    public const string Regex = @"^\s*remove_group_skill\s*" +
        @":\s*(?<id>" + ModUtility033.IdentifierRegexPart + @")\s*$";

    public string SkillId;

    public RemoveGroupSkillEffect(Match match, string id) :
        base(id)
    {
        SkillId = match.Groups["id"].Value;
    }

    public override void Apply(CellGroup group)
    {
        group.Culture.AddSkillToLose(SkillId);
    }

    public override bool IsDeferred()
    {
        return false;
    }

    public override string ToString()
    {
        return "'Remove Group Skill' Effect, Skill Id: " + SkillId;
    }
}
