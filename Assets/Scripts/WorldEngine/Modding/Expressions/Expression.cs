using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Expression
{
    public static Expression BuildExpression(string expressionStr)
    {
        //Debug.Log("parsing: " + expressionStr);

        Match match = Regex.Match(expressionStr, ModUtility.UnaryOpStatementRegex);
        if (match.Success == true)
        {
            Debug.Log("match: " + match.Value);
            Debug.Log("statement: " + ModUtility.Debug_CapturesToString(match.Groups["statement"]));
            Debug.Log("unaryOp: " + ModUtility.Debug_CapturesToString(match.Groups["unaryOp"]));

            return BuildUnaryOpExpression(match);
        }

        match = Regex.Match(expressionStr, ModUtility.InnerStatementRegex);
        if (match.Success == true)
        {
            Debug.Log("match: " + match.Value);
            Debug.Log("innerStatement: " + ModUtility.Debug_CapturesToString(match.Groups["innerStatement"]));

            expressionStr = match.Groups["innerStatement"].Value;

            return BuildExpression(expressionStr);
        }

        match = Regex.Match(expressionStr, ModUtility.BaseStatementRegex);
        if (match.Success == true)
        {
            expressionStr = match.Groups["statement"].Value;

            return BuildBaseExpression(expressionStr);
        }

        throw new System.ArgumentException("Not a valid parseable expression: " + expressionStr);
    }

    private static Expression BuildUnaryOpExpression(Match match)
    {
        string expressionStr = match.Groups["statement"].Value;
        string unaryOp = match.Groups["unaryOp"].Value.Trim().ToUpper();

        switch (unaryOp)
        {
            case "!":
                return new InvFactor(expressionStr);
            case "-":
                return new SqFactor(expressionStr);
        }

        throw new System.ArgumentException("Unrecognized unary op: " + unaryOp);
    }

    private static Expression BuildBaseExpression(string factorStr)
    {
        Match match = Regex.Match(factorStr, NeighborhoodBiomeTypePresenceFactor.Regex);
        if (match.Success == true)
        {
            return new NeighborhoodBiomeTypePresenceFactor(match);
        }

        match = Regex.Match(factorStr, NeighborhoodBiomeTraitPresenceFactor.Regex);
        if (match.Success == true)
        {
            return new NeighborhoodBiomeTraitPresenceFactor(match);
        }

        match = Regex.Match(factorStr, CellAccessibilityFactor.Regex);
        if (match.Success == true)
        {
            return new CellAccessibilityFactor(match);
        }

        match = Regex.Match(factorStr, CellArabilityFactor.Regex);
        if (match.Success == true)
        {
            return new CellArabilityFactor(match);
        }

        match = Regex.Match(factorStr, CellForagingCapacityFactor.Regex);
        if (match.Success == true)
        {
            return new CellForagingCapacityFactor(match);
        }

        match = Regex.Match(factorStr, CellSurvivabilityFactor.Regex);
        if (match.Success == true)
        {
            return new CellSurvivabilityFactor(match);
        }

        match = Regex.Match(factorStr, CellBiomePresenceFactor.Regex);
        if (match.Success == true)
        {
            return new CellBiomePresenceFactor(match);
        }

        match = Regex.Match(factorStr, CellHillinessFactor.Regex);
        if (match.Success == true)
        {
            return new CellHillinessFactor(match);
        }

        match = Regex.Match(factorStr, CellFlowingWaterFactor.Regex);
        if (match.Success == true)
        {
            return new CellFlowingWaterFactor(match);
        }

        match = Regex.Match(factorStr, CellBiomeTraitPresenceFactor.Regex);
        if (match.Success == true)
        {
            return new CellBiomeTraitPresenceFactor(match);
        }

        match = Regex.Match(factorStr, CellBiomeTypePresenceFactor.Regex);
        if (match.Success == true)
        {
            return new CellBiomeTypePresenceFactor(match);
        }

        throw new System.ArgumentException("Not a recognized factor: " + factorStr);
    }

    public static Expression[] BuildExpressions(ICollection<string> expressionStrs)
    {
        Expression[] expression = new Expression[expressionStrs.Count];

        int i = 0;
        foreach (string expressionStr in expressionStrs)
        {
            expression[i++] = BuildExpression(expressionStr);
        }

        return expression;
    }

    public abstract float Calculate(CellGroup group);

    public abstract float Calculate(TerrainCell cell);
}
