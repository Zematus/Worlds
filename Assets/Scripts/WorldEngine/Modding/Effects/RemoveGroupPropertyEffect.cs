using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RemoveGroupPropertyEffect : GroupEffect
{
    public const string Regex = @"^\s*remove_group_property\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility.AttributeRegexPart + @")\s*$";

    public string Property;

    public RemoveGroupPropertyEffect(Match match, string id) :
        base(match.Groups["type"].Value, id)
    {
        Property = match.Groups["value"].Value;
    }

    public override void ApplyToTarget(CellGroup group)
    {
        group.RemoveProperty(Property);
    }

    public override string ToString()
    {
        return "'Remove Group Property' Effect, Target Type " + TargetType + ", Property: " + Property;
    }
}
