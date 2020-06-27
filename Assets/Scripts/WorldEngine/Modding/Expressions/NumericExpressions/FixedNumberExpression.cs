using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedNumberExpression : FixedValueExpression<float>
{
    public const string Regex = @"^" + ModParseUtility.NumberRegexPart + @"\s*$";

    public static float ParseExpression(string numberStr)
    {
        if (!float.TryParse(numberStr.Trim(), out float value))
        {
            throw new System.ArgumentException("Not a valid number: " + numberStr);
        }

        return value;
    }

    public FixedNumberExpression(string numberStr)
        : base(ParseExpression(numberStr))
    {
    }

    public FixedNumberExpression(float value) : base(value)
    {
    }
}
