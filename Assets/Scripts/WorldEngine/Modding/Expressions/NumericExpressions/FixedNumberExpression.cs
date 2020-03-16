using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FixedNumberExpression : INumericExpression
{
    public const string Regex = @"^" + ModUtility.NumberRegexPart + @"\s*$";

    public float NumberValue;

    public static float ParseExpression(string numberStr)
    {
        if (!float.TryParse(numberStr, out float value))
        {
            throw new System.ArgumentException("Not a valid number: " + numberStr);
        }

        return value;
    }

    public FixedNumberExpression(string numberStr)
    {
        NumberValue = ParseExpression(numberStr);
    }

    public FixedNumberExpression(float value)
    {
        NumberValue = value;
    }

    public float Value => NumberValue;

    public override string ToString()
    {
        return NumberValue.ToString();
    }

    public string GetString() => Value.ToString();
}
