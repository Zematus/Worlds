using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedBooleanValueExpression : IValueExpression<bool>
{
    public const string Regex = @"^" + ModUtility.BooleanRegexPart + @"\s*$";

    public bool BooleanValue;

    public static bool ParseExpression(string booleanStr)
    {
        if (!bool.TryParse(booleanStr, out bool value))
        {
            throw new System.ArgumentException("Not a valid boolean value: " + booleanStr);
        }

        return value;
    }

    public FixedBooleanValueExpression(string boolStr)
    {
        BooleanValue = ParseExpression(boolStr);
    }

    public FixedBooleanValueExpression(bool booleanValue)
    {
        BooleanValue = booleanValue;
    }

    public bool Value => BooleanValue;

    public override string ToString() => BooleanValue.ToString();

    public string GetString() => Value.ToString();
}
