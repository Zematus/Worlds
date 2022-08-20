using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NeighborhoodBiomeTraitPresenceFactor : Factor
{
    public const string Regex = @"^\s*neighborhood_biome_trait_presence\s*" +
        @":\s*(?<trait>" + ModUtility033.IdentifierRegexPart + @")\s*$";

    public string Trait;

    public NeighborhoodBiomeTraitPresenceFactor(Match match)
    {
        Trait = match.Groups["trait"].Value;
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return cell.GetNeighborhoodBiomeTraitPresence(Trait) / TerrainCell.MaxNeighborhoodCellCount;
    }

    public override string ToString()
    {
        return "'Neighborhood Biome Trait Presence' Factor";
    }
}
