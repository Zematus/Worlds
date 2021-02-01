using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class ModTextExpression : IValueExpression<ModText>
{
    private ModText _modText = null;

    public ModText Value => _modText;

    public object ValueObject => _modText;

    public bool RequiresInput => _modText.RequiresInput;

    public ModTextExpression(Context context, Match match, bool allowInputRequesters = false)
    {
        string text = match.Groups["text"].Value.Trim();

        _modText = new ModText(context, text, allowInputRequesters);
    }

    public override string ToString() => _modText.ToString();

    public string GetFormattedString() => _modText.EvaluateString();

    public string ToPartiallyEvaluatedString(bool evaluate = true) =>
        _modText.ToPartiallyEvaluatedString(evaluate);

    public bool TryGetRequest(out InputRequest request) =>
        _modText.TryGetRequest(out request);
}
