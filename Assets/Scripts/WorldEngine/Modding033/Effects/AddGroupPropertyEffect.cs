using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AddGroupPropertyEffect : Effect
{
    public const string Regex = @"^\s*add_group_property\s*" +
        @":\s*(?<value>" + ModUtility033.AttributeRegexPart + @")\s*$";

    public string Property;

    public AddGroupPropertyEffect(Match match, string id) :
        base(id)
    {
        Property = match.Groups["value"].Value;
    }

    public override void Apply(CellGroup group)
    {
        group.AddPropertyToAquire(Property);
    }

    public override bool IsDeferred()
    {
        return false;
    }

    public override string ToString()
    {
        return "'Add Group Property' Effect, Property: " + Property;
    }
}
