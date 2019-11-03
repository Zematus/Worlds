using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupPopulationCondition : GroupCondition
{
    public const string Regex = @"^\s*group_population\s*" +
        @":\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";
    
    public int MinPopulation;

    public GroupPopulationCondition(Match match)
    {
        string valueStr = match.Groups["value"].Value;
        int value;

        if (!MathUtility.TryParseCultureInvariant(valueStr, out value))
        {
            throw new System.ArgumentException("GroupPopulationCondition: Min value can't be parsed into a valid integer point number: " + valueStr);
        }

        if (!value.IsInsideRange(1, int.MaxValue))
        {
            throw new System.ArgumentException("GroupPopulationCondition: Min value is outside the range of 1 and " + int.MaxValue + ": " + valueStr);
        }

        MinPopulation = value;
    }

    public override bool Evaluate(CellGroup group)
    {
        return group.Population >= MinPopulation;
    }

    public override string ToString()
    {
        return "'Group Population' Condition, Min Population: " + MinPopulation;
    }
}
