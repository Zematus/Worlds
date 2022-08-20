using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellBiomeMostPresentCondition : CellCondition
{
    public const string Regex = @"^\s*cell_biome_most_present\s*" +
        @":\s*(?<id>" + ModUtility033.IdentifierRegexPart + @")\s*$";

    private string _biomeId;

    public CellBiomeMostPresentCondition(Match match)
    {
        _biomeId = match.Groups["id"].Value;

        if (!Biome.Biomes.ContainsKey(_biomeId))
        {
            throw new System.ArgumentException("CellBiomeMostPresentCondition: Unable to find biome with id: " + _biomeId);
        }
    }

    public override bool Evaluate(TerrainCell cell)
    {
        return cell.BiomeWithMostPresence == _biomeId;
    }

    public override string GetPropertyValue(string propertyId)
    {
        return null;
    }

    public override string ToString()
    {
        return "'Cell Biome Most Present' Condition, Biome Id: " + _biomeId;
    }
}
