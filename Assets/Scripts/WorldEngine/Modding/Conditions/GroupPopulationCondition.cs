using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupPopulationCondition : GroupCondition
{
    public const int DefaultMinValue = 1;

    public const string Regex = @"^\s*group_population\s*" +
        @":\s*(?<value>" + ModUtility.NumberRegexPart + @")\s*$";
    
    public int MinPopulation;

    public GroupPopulationCondition(Match match)
    {
        if (!string.IsNullOrEmpty(match.Groups["value"].Value))
        {
            string valueStr = match.Groups["value"].Value;
            int value;

            if (!int.TryParse(valueStr, out value))
            {
                throw new System.ArgumentException("GroupPopulationCondition: Min value can't be parsed into a valid integer point number: " + valueStr);
            }

            if (!value.IsInsideRange(1, int.MaxValue))
            {
                throw new System.ArgumentException("GroupPopulationCondition: Min value is outside the range of 1 and " + int.MaxValue + ": " + valueStr);
            }

            MinPopulation = value;
        }
        else
        {
            MinPopulation = DefaultMinValue;
        }
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
