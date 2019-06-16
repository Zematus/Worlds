using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Condition
{
    public static Condition BuildCondition(string conditionStr)
    {
        Match match = Regex.Match(conditionStr, ModUtility.OrStatementRegex);
        if (match.Success == true)
        {
            string conditionAStr = match.Groups["Statement"].Value;
            string conditionBStr = match.Groups["Statement2"].Value;

            return new OrCondition(conditionAStr, conditionBStr);
        }

        match = Regex.Match(conditionStr, ModUtility.NotStatementRegex);
        if (match.Success == true)
        {
            conditionStr = match.Groups["Statement"].Value;

            return new NotCondition(conditionStr);
        }

        match = Regex.Match(conditionStr, ModUtility.InnerStatementRegex);
        if (match.Success == true)
        {
            conditionStr = match.Groups["Statement"].Value;

            return BuildCondition(conditionStr);
        }

        match = Regex.Match(conditionStr, ModUtility.BaseStatementRegex);
        if (match.Success != true)
        {
            conditionStr = match.Groups["Statement"].Value;
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

        match = Regex.Match(conditionStr, CellHasSeaCondition.Regex);
        if (match.Success == true)
        {
            return new CellHasSeaCondition(match);
        }

        throw new System.ArgumentException("Not a recognized condition: " + conditionStr);
    }

    public static Condition[] BuildConditions(ICollection<string> conditionStrs)
    {
        Condition[] conditions = new Condition[conditionStrs.Count];

        int i = 0;
        foreach (string conditionStr in conditionStrs)
        {
            conditions[i++] = BuildCondition(conditionStr);
        }

        return conditions;
    }

    public abstract bool Evaluate(CellGroup group);
    public abstract bool Evaluate(TerrainCell cell);
}
