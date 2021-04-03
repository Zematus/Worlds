using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ModText : IInputRequester, IFormattedStringGenerator
{
    private string _partsString;

    private readonly List<IFormattedStringGenerator> _textParts =
        new List<IFormattedStringGenerator>();

    public bool RequiresInput
    {
        get
        {
            foreach (IFormattedStringGenerator part in _textParts)
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

                _textParts.Add(new StringTextPart(value));
            }
            else if (!string.IsNullOrEmpty(value = match.Groups["expression"].Value))
            {
                //Debug.Log("expression: " + value);

                IBaseValueExpression exp =
                    ValueExpressionBuilder.BuildValueExpression(
                        context, value, allowInputRequesters);

                if (exp is IFormattedStringGenerator)
                {
                    _textParts.Add(exp);
                }
                else
                {
                    throw new System.Exception(
                        "Error: Text part '" + value +
                        "' is not an expression that can generate a formatted string element");
                }
            }
            else
            {
                throw new System.Exception(
                    "Error: Text part '" + value +
                    "' could not be matched to a string or expression");
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

    public string GetFormattedString()
    {
        string output = "";

        foreach (IFormattedStringGenerator part in _textParts)
        {
            output += part.GetFormattedString();
        }

        return output;
    }

    public string ToPartiallyEvaluatedString(bool evaluate = true)
    {
        string output = "";

        foreach (IFormattedStringGenerator part in _textParts)
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
        foreach (IFormattedStringGenerator part in _textParts)
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
