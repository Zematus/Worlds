using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedStringValueExpression : FixedValueExpression<string>
{
    public const string Regex = @"^" + ModParseUtility.IdentifierRegexPart + @"\s*$";

    public FixedStringValueExpression(string value) : base (value)
    {
    }
}
