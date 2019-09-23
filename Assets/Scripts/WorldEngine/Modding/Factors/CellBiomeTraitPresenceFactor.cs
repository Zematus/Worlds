using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CellBiomeTraitPresenceFactor : Factor
{
    public const string Regex = @"^\s*cell_biome_trait_presence\s*" +
        @":\s*(?<trait>" + ModUtility.IdentifierRegexPart + @")\s*$";

    public string Trait;

    public CellBiomeTraitPresenceFactor(Match match)
    {
        Trait = match.Groups["trait"].Value;
    }

    public override float Calculate(CellGroup group)
    {
        return Calculate(group.Cell);
    }

    public override float Calculate(TerrainCell cell)
    {
        return cell.GetBiomeTraitPresence(Trait);
    }

    public override string ToString()
    {
        return "'Cell Biome Trait Presence' Factor, Trait: " + Trait;
    }
}
