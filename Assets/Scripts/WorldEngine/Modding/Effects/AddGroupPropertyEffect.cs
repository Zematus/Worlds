using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AddGroupPropertyEffect : GroupEffect
{
    public const string Regex = @"^\s*add_group_property\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility.AttributeRegexPart + @")\s*$";

    public string Property;

    public AddGroupPropertyEffect(Match match, string id) :
        base(match.Groups["type"].Value, id)
    {
        Property = match.Groups["value"].Value;
    }

    public override void ApplyToTarget(CellGroup group)
    {
        group.Culture.AddPropertyToAquire(Property);
    }

    public override bool IsDeferred()
    {
        return false;
    }

    public override string ToString()
    {
        return "'Add Group Property' Effect, Target Type " + TargetType + ", Property: " + Property;
    }
}
