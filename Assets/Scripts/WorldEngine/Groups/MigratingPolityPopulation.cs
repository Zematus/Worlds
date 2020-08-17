using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

/// <summary>
/// Segment of polity population migrating from one cell to another
/// </summary>
public class MigratingPolityPopulation : MigratingPopulation
{
    public Polity Polity;

    /// <summary>
    /// Constructs a new migrating polity population object
    /// </summary>
    /// <param name="world">world this object belongs to</param>
    /// <param name="prominencePercent">percentage of the source prominence to migrate</param>
    /// <param name="sourceGroup">the cell group this originates from</param>
    /// <param name="polity">the polity to which the migrating population belongs</param>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is exiting from the source</param>
    public MigratingPolityPopulation(
        World world,
        float prominencePercent,
        CellGroup sourceGroup,
        Polity polity,
        TerrainCell targetCell,
        Direction migrationDirection)
        : base (world, prominencePercent, sourceGroup, targetCell, migrationDirection)
    {
        Polity = polity;
    }

    protected override int SplitFromGroup()
    {
        return SourceGroup.SplitPolityPopulation(this);
    }

    protected override BufferCulture CreateMigratingCulture()
    {
        float isolationPreference =
            Polity.GetPreferenceValue(CulturalPreference.IsolationPreferenceId);

        BufferCulture culture = new BufferCulture(SourceGroup.Culture);

        // The resulting culture should approximate the polity's culture by it's level
        // of isolation. Meaning that highly isolated polity cultures are less susceptible
        // to influence from the other cultures within the same cell group
        culture.MergeCulture(Polity.Culture, isolationPreference);

        return culture;
    }

    protected override void MergeIntoGroup(CellGroup targetGroup)
    {
        throw new System.NotImplementedException();
    }

    protected override CellGroup CreateGroupOnTarget()
    {
        throw new System.NotImplementedException();
    }
}
