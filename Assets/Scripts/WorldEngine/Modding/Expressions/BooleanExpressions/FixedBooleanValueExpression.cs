using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedBooleanValueExpression : BooleanExpression
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

    public FixedBooleanValueExpression(string numberStr)
    {
        BooleanValue = ParseExpression(numberStr);
    }

    public override bool Evaluate()
    {
        return BooleanValue;
    }

    public override string ToString()
    {
        return BooleanValue.ToString();
    }
}
