using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Factor
{
    public static Factor BuildFactor(string factorStr)
    {
        //Debug.Log("parsing: " + factorStr);

        Match match = Regex.Match(factorStr, ModUtility.UnaryOpStatementRegex);
        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("statement: " + ModUtility.Debug_CapturesToString(match.Groups["statement"]));
            //Debug.Log("unaryOp: " + ModUtility.Debug_CapturesToString(match.Groups["unaryOp"]));

            return BuildUnaryOpFactor(match);
        }

        match = Regex.Match(factorStr, ModUtility.InnerStatementRegex);
        if (match.Success == true)
        {
            //Debug.Log("match: " + match.Value);
            //Debug.Log("innerStatement: " + ModUtility.Debug_CapturesToString(match.Groups["innerStatement"]));

            factorStr = match.Groups["innerStatement"].Value;

            return BuildFactor(factorStr);
        }

        match = Regex.Match(factorStr, ModUtility.BaseStatementRegex);
        if (match.Success == true)
        {
            factorStr = match.Groups["statement"].Value;

            return BuildBaseFactor(factorStr);
        }

        throw new System.ArgumentException("Not a valid parseable factor: " + factorStr);
    }

    private static Factor BuildUnaryOpFactor(Match match)
    {
        string factorStr = match.Groups["statement"].Value;
        string unaryOp = match.Groups["unaryOp"].Value.Trim().ToUpper();

        switch (unaryOp)
        {
            case "[INV]":
                return new InvFactor(factorStr);
            case "[SQ]":
                return new SqFactor(factorStr);
        }

        throw new System.ArgumentException("Unrecognized unary op: " + unaryOp);
    }

    private static Factor BuildBaseFactor(string factorStr)
    {
        Match match = Regex.Match(factorStr, NeighborhoodSeaPresenceFactor.Regex);
        if (match.Success == true)
        {
            return new NeighborhoodSeaPresenceFactor(match);
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

        match = Regex.Match(factorStr, CellWoodPresenceFactor.Regex);
        if (match.Success == true)
        {
            return new CellWoodPresenceFactor(match);
        }

        throw new System.ArgumentException("Not a recognized factor: " + factorStr);
    }

    public static Factor[] BuildFactors(ICollection<string> factorStrs)
    {
        Factor[] factors = new Factor[factorStrs.Count];

        int i = 0;
        foreach (string factorStr in factorStrs)
        {
            factors[i++] = BuildFactor(factorStr);
        }

        return factors;
    }

    public abstract float Calculate(CellGroup group);

    public abstract float Calculate(TerrainCell cell);
}
