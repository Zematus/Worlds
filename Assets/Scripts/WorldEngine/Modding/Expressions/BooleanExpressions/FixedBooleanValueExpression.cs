using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedBooleanValueExpression : FixedValueExpression<bool>
{
    public const string Regex = @"^" + ModUtility.BooleanRegexPart + @"\s*$";

    public static bool ParseExpression(string booleanStr)
    {
        if (!bool.TryParse(booleanStr, out bool value))
        {
            throw new System.ArgumentException("Not a valid boolean value: " + booleanStr);
        }

        return value;
    }

    public FixedBooleanValueExpression(string boolStr)
        : base(ParseExpression(boolStr))
    {
    }

    public FixedBooleanValueExpression(bool value) : base(value)
    {
    }
}
