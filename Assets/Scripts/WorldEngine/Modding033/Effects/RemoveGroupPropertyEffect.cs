using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RemoveGroupPropertyEffect : Effect
{
    public const string Regex = @"^\s*remove_group_property\s*" +
        @":\s*(?<value>" + ModUtility.AttributeRegexPart + @")\s*$";

    public string Property;

    public RemoveGroupPropertyEffect(Match match, string id) :
        base(id)
    {
        Property = match.Groups["value"].Value;
    }

    public override void Apply(CellGroup group)
    {
        group.AddPropertyToLose(Property);
    }

    public override bool IsDeferred()
    {
        return false;
    }

    public override string ToString()
    {
        return "'Remove Group Property' Effect, Property: " + Property;
    }
}
