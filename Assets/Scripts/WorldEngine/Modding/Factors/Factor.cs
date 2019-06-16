using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Factor
{
    public static Factor BuildFactor(string factorStr)
    {
        Match match = Regex.Match(factorStr, ModUtility.InvStatementRegex);
        if (match.Success == true)
        {
            factorStr = match.Groups["Statement"].Value;

            return new InvFactor(factorStr);
        }

        match = Regex.Match(factorStr, ModUtility.SqStatementRegex);
        if (match.Success == true)
        {
            factorStr = match.Groups["Statement"].Value;

            return new SqFactor(factorStr);
        }

        match = Regex.Match(factorStr, ModUtility.InnerStatementRegex);
        if (match.Success == true)
        {
            factorStr = match.Groups["Statement"].Value;

            return BuildFactor(factorStr);
        }

        match = Regex.Match(factorStr, ModUtility.BaseStatementRegex);
        if (match.Success != true)
        {
            factorStr = match.Groups["Statement"].Value;
            throw new System.ArgumentException("Not a valid parseable factor: " + factorStr);
        }

        return BuildBaseFactor(factorStr);
    }

    private static Factor BuildBaseFactor(string factorStr)
    {
        Match match = Regex.Match(factorStr, NeighborhoodSeaPresenceFactor.Regex);
        if (match.Success == true)
        {
            return new NeighborhoodSeaPresenceFactor(match);
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
