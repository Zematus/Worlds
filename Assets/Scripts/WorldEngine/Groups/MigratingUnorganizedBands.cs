using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

/// <summary>
/// Segment of unorganized bands migrating from one cell to another
/// </summary>
public class MigratingUnorganizedBands : MigratingPopulation
{
    /// <summary>
    /// Constructs a new migrating unorganized bands object
    /// </summary>
    /// <param name="world">world this object belongs to</param>
    /// <param name="prominencePercent">percentage of the source prominence to migrate</param>
    /// <param name="sourceGroup">the cell group this originates from</param>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is exiting from the source</param>
    public MigratingUnorganizedBands(
        World world,
        float prominencePercent,
        CellGroup sourceGroup,
        TerrainCell targetCell,
        Direction migrationDirection)
        : base (world, prominencePercent, sourceGroup, targetCell, migrationDirection)
    {
    }

    protected override int SplitFromGroup()
    {
        float ubProminenceValue = SourceGroup.GetUBandsProminenceValue();

        float ubProminenceValueDelta = ProminencePercent * ubProminenceValue;

        int splitPopulation =
            (int)(SourceGroup.Population * ubProminenceValueDelta);

        SourceGroup.AddUBandsProminenceValueDelta(-ubProminenceValueDelta);

        SourceGroup.ChangePopulation(-splitPopulation);

        return splitPopulation;
    }

    protected override BufferCulture CreateMigratingCulture()
    {
        return new BufferCulture(SourceGroup.Culture);
    }

    protected override void MergeIntoGroup(CellGroup targetGroup)
    {
        float prominenceDelta = Population / targetGroup.Population;

        float percentageOfPopulation = Population / (targetGroup.Population + Population);

        if (!percentageOfPopulation.IsInsideRange(0, 1))
        {
            throw new System.Exception(
                "Percentage increase outside of range (0,1): " + percentageOfPopulation +
                " - Group: " + targetGroup.Id);
        }

        targetGroup.ChangePopulation(Population);

        targetGroup.Culture.MergeCulture(Culture, percentageOfPopulation);

        targetGroup.AddUBandsProminenceValueDelta(prominenceDelta);

        targetGroup.TriggerInterference();
    }

    protected override CellGroup CreateGroupOnTarget()
    {
        return new CellGroup(this);
    }
}
