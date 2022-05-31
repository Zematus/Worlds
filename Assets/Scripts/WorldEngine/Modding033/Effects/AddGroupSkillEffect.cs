using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AddGroupSkillEffect : Effect
{
    public const string Regex = @"^\s*add_group_skill\s*" +
        @":\s*(?<id>" + ModUtility033.IdentifierRegexPart + @")\s*$";

    public string SkillId;

    public AddGroupSkillEffect(Match match, string id) :
        base(id)
    {
        SkillId = match.Groups["id"].Value;
    }

    public override void Apply(CellGroup group)
    {
        group.Culture.AddSkillToLearn(SkillId);
    }

    public override bool IsDeferred()
    {
        return false;
    }

    public override string ToString()
    {
        return "'Add Group Skill' Effect, Skill Id: " + SkillId;
    }
}
