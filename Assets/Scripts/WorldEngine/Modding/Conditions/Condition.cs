using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Condition
{
    public const string BaseConditionRegexPart = @"(?<Cond>[^\(\)]+)";
    public const string BaseConditionRegex = @"^\s*" + BaseConditionRegexPart + @"\s*$";
    public const string InnerConditionRegexPart = @"(?:(?<Open>\()[^\(\)]*)+(?:(?<Cond-Open>\))[^\(\)]*)+(?(Open)(?!))";
    public const string InnerConditionRegex = @"^\s*" + InnerConditionRegexPart + @"\s*$";
    public const string MixedConditionRegexPart = "(?:" + InnerConditionRegexPart + "|" + BaseConditionRegexPart + ")";
    public const string MixedConditionRegex = @"^\s*" + MixedConditionRegexPart + @"\s*$";
    public const string NotConditionRegex = @"^\s*\[NOT\]\s*" + MixedConditionRegexPart + @"\s*$";
    public const string OrConditionRegex = @"^\s*" + MixedConditionRegexPart + @"\s*\[OR\]\s*(?<Cond2>.+)\s*$";

    public static Condition BuildCondition(string conditionStr)
    {
        Match match = Regex.Match(conditionStr, OrConditionRegex);
        if (match.Success == true)
        {
            string conditionAStr = match.Groups["Cond"].Value;
            string conditionBStr = match.Groups["Cond2"].Value;

            return new OrCondition(conditionAStr, conditionBStr);
        }

        match = Regex.Match(conditionStr, NotConditionRegex);
        if (match.Success == true)
        {
            conditionStr = match.Groups["Cond"].Value;

            return new NotCondition(conditionStr);
        }

        match = Regex.Match(conditionStr, InnerConditionRegex);
        if (match.Success == true)
        {
            conditionStr = match.Groups["Cond"].Value;

            return BuildCondition(conditionStr);
        }

        match = Regex.Match(conditionStr, BaseConditionRegex);
        if (match.Success != true)
        {
            conditionStr = match.Groups["Cond"].Value;
            throw new System.ArgumentException("Not a valid parseable condition: " + conditionStr);
        }

        return BuildBaseCondition(conditionStr);
    }

    private static Condition BuildBaseCondition(string conditionStr)
    {
        Match match = Regex.Match(conditionStr, GroupHasKnowledgeCondition.Regex);
        if (match.Success == true)
        {
            return new GroupHasKnowledgeCondition(match);
        }

        match = Regex.Match(conditionStr, CellIsSeaCondition.Regex);
        if (match.Success == true)
        {
            return new CellIsSeaCondition(match);
        }

        throw new System.ArgumentException("Not a recognized condition: " + conditionStr);
    }

    public abstract bool Evaluate(CellGroup group);
    public abstract bool Evaluate(TerrainCell cell);
}
