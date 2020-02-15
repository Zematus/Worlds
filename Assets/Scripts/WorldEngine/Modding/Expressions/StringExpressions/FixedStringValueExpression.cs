using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedStringValueExpression : IStringExpression
{
    public const string Regex = @"^" + ModUtility.IdentifierRegexPart + @"\s*$";

    private string _value;

    public FixedStringValueExpression(string identifier)
    {
        _value = identifier;
    }

    public string Value => _value;

    public override string ToString()
    {
        return _value;
    }
}
