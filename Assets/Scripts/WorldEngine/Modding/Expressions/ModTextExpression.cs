using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class ModTextExpression : IValueExpression<string>
{
    public ModText ModText = null;

    public string Value => ModText.EvaluateString();

    public object ValueObject => ModText;

    public bool RequiresInput => ModText.RequiresInput;

    public ModTextExpression(Context context, Match match, bool allowInputRequesters = false)
    {
        string text = match.Groups["text"].Value.Trim();

        ModText = new ModText(context, text, allowInputRequesters);
    }

    public override string ToString() => ModText.ToString();

    public string GetFormattedString() => ModText.EvaluateString();

    public string ToPartiallyEvaluatedString(bool evaluate = true) =>
        ModText.ToPartiallyEvaluatedString(evaluate);

    public bool TryGetRequest(out InputRequest request) =>
        ModText.TryGetRequest(out request);
}
