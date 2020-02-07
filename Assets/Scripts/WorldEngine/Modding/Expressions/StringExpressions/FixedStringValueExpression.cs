using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedStringValueExpression : StringExpression
{
    public const string Regex = @"^" + ModUtility.IdentifierRegexPart + @"\s*$";

    public string StringValue;

    public FixedStringValueExpression(string identifier)
    {
        StringValue = identifier;
    }

    protected override string Evaluate()
    {
        return StringValue;
    }

    public override string GetValue()
    {
        return StringValue;
    }

    public override string ToString()
    {
        return StringValue.ToString();
    }
}
