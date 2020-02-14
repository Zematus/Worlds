using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedStringValueExpression : IStringExpression
{
    public const string Regex = @"^" + ModUtility.IdentifierRegexPart + @"\s*$";

    public string StringValue;

    public FixedStringValueExpression(string identifier)
    {
        StringValue = identifier;
    }

    public string Value => StringValue;

    public override string ToString()
    {
        return StringValue.ToString();
    }
}
