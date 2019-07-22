using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FormPolityOnGroupEffect : GroupEffect
{
    public const string Regex = @"^\s*form_polity_on_group\s*" +
        @":\s*(?<type>" + ModUtility.IdentifierRegexPart + @")\s*" +
        @",\s*(?<value>" + ModUtility.IdentifierRegexPart + @")\s*$";

    public PolityType PolityType;

    public FormPolityOnGroupEffect(Match match, string id) :
        base(match.Groups["type"].Value, id)
    {
        PolityType = PolityInfo.GetPolityType(match.Groups["value"].Value);
    }

    public override void ApplyToTarget(CellGroup group)
    {
        Polity.TryGenerateNewPolity(PolityType, group);
    }

    public override bool IsDeferred()
    {
        return false;
    }

    public override string ToString()
    {
        return "'Form Polity On Group' Effect, Target Type " + TargetType + ", Polity Type: " + PolityType;
    }
}
