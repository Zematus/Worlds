using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellBiomePresenceFactor : Factor
{
    public const string Regex = @"^\s*cell_biome_presence\s*" +
        @":\s* (?<id>" + ModUtility033.IdentifierRegexPart + @")\s*$";

    private string _biomeId;

    public CellBiomePresenceFactor(Match match)
    {
        _biomeId = match.Groups["id"].Value;

        if (!Biome.Biomes.ContainsKey(_biomeId))
        {
            throw new System.ArgumentException("CellBiomePresenceFactor: Unable to find biome with id: " + _biomeId);
        }
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return cell.GetBiomePresence(_biomeId);
    }

    public override string ToString()
    {
        return "'Cell Biome Presence' Factor, Biome Id: " + _biomeId;
    }
}
