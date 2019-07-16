using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AddGroupAttributeEffect : GroupEffect
{
    public const string Regex = @"^\s*add_group_attribute\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility.IdentifierRegexPart + @")\s*$";

    public string Attribute;

    public AddGroupAttributeEffect(Match match, string id) :
        base(match.Groups["type"].Value, id)
    {
        Attribute = match.Groups["value"].Value;
    }

    public override void ApplyToTarget(CellGroup group)
    {
        group.AddAttribute(Attribute);
    }

    public override string ToString()
    {
        return "'Add Group Attribute' Effect, Target Type " + TargetType + ", Attribute Id: " + Attribute;
    }
}
