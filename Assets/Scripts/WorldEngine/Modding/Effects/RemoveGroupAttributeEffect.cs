using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RemoveGroupAttributeEffect : GroupEffect
{
    public const string Regex = @"^\s*remove_group_attribute\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility.AttributeRegexPart + @")\s*$";

    public string Attribute;

    public RemoveGroupAttributeEffect(Match match, string id) :
        base(match.Groups["type"].Value, id)
    {
        Attribute = match.Groups["value"].Value;
    }

    public override void ApplyToTarget(CellGroup group)
    {
        group.RemoveAttribute(Attribute);
    }

    public override string ToString()
    {
        return "'Remove Group Attribute' Effect, Target Type " + TargetType + ", Attribute Id: " + Attribute;
    }
}
