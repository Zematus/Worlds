using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ModText : IInputRequester
{
    private string _partsString;

    private List<IModTextPart> textParts = new List<IModTextPart>();

    public bool RequiresInput
    {
        get
        {
            foreach (IModTextPart part in textParts)
            {
                if ((part is IExpression exp) && exp.RequiresInput)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public ModText(Context context, string textStr, bool allowInputRequesters = false)
    {
        _partsString = "";

        foreach (Match match in Regex.Matches(textStr, ModParseUtility.ModTextRegexPart))
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
                        ExpressionBuilder.BuildExpression(context, value, allowInputRequesters);

                    if (exp is IModTextPart)
                    {
                        textParts.Add(exp as IModTextPart);
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
            output += part.GetFormattedString();
        }

        return output;
    }

    public string ToPartiallyEvaluatedString(bool evaluate = true)
    {
        string output = "";

        foreach (IModTextPart part in textParts)
        {
            if (part is IExpression exp)
            {
                output += exp.ToPartiallyEvaluatedString(evaluate);
            }
            else
            {
                output += part.GetFormattedString();
            }
        }

        return output;
    }

    public bool TryGetRequest(out InputRequest request)
    {
        foreach (IModTextPart part in textParts)
        {
            if ((part is IExpression exp) && exp.TryGetRequest(out request))
            {
                return true;
            }
        }

        request = null;

        return false;
    }
}
