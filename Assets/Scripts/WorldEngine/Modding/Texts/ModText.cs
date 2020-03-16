using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ModText
{
    private string _partsString;

    private List<IModTextPart> textParts = new List<IModTextPart>();

    public ModText(Context context, string textStr)
    {
        _partsString = "";

        foreach (Match match in Regex.Matches(textStr, ModUtility.ModTextRegexPart))
        {
            string value = match.Groups["string"].Value;

            if (!string.IsNullOrEmpty(value))
            {
                //Debug.Log("string: " + value);

                textParts.Add(new StringTextPart(value));
            }
            else
            {
                value = match.Groups["expression"].Value;

                if (!string.IsNullOrEmpty(value))
                {
                    //Debug.Log("expression: " + value);

                    IExpression exp =
                        ExpressionBuilder.BuildExpression(context, value);

                    if (exp is IStringExpression)
                    {
                        textParts.Add(exp as IStringExpression);
                    }
                    else if (exp is INumericExpression)
                    {
                        textParts.Add(exp as INumericExpression);
                    }
                    else if (exp is IBooleanExpression)
                    {
                        textParts.Add(exp as IBooleanExpression);
                    }
                    else
                    {
                        throw new System.Exception(
                            "Error: Text part '" + value + "' is not an expression that can be evaluated into a text element");
                    }
                }
                else
                {
                    throw new System.Exception(
                        "Error: Text part '" + value + "' could not be matched to a string or expression");
                }
            }

            _partsString += match.Value;
        }

        if (string.Compare(_partsString, textStr) != 0)
        {
            throw new System.Exception(
                "Error: Original mod text string '" + textStr
                + "' doesn't match parsed text '" + _partsString
                + "'");
        }
    }

    public override string ToString()
    {
        return _partsString;
    }

    public string EvaluateString()
    {
        string output = "";

        foreach (IModTextPart part in textParts)
        {
            output += part.GetString();
        }

        return output;
    }
}
