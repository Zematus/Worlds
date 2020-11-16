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
    /// <param name="prominenceValueDelta">how much the prominence value should change</param>
    /// <param name="population">population to migrate</param>
    /// <param name="sourceGroup">the cell group this originates from</param>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is exiting
    /// from the source</param>
    /// <param name="startDate">the migration start date</param>
    /// <param name="endDate">the migration end date</param>
    public MigratingUnorganizedBands(
        World world,
        float prominencePercent,
        float prominenceValueDelta,
        int population,
        CellGroup sourceGroup,
        TerrainCell targetCell,
        Direction migrationDirection,
        long startDate,
        long endDate)
        : base (
            world,
            prominencePercent,
            prominenceValueDelta,
            population,
            sourceGroup,
            null,
            targetCell,
            migrationDirection,
            startDate,
            endDate)
    {
    }

    /// <summary>
    /// Sets the object properties to use during a migration event
    /// </summary>
    /// <param name="prominencePercent">percentage of the source prominence to migrate</param>
    /// <param name="prominenceValueDelta">how much the prominence value should change</param>
    /// <param name="population">population to migrate</param>
    /// <param name="sourceGroup">the cell group this originates from</param>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is moving out
    /// from the source</param>
    /// <param name="startDate">the migration start date</param>
    /// <param name="endDate">the migration end date</param>
    public void Set(
        float prominencePercent,
        float prominenceValueDelta,
        int population,
        CellGroup sourceGroup,
        TerrainCell targetCell,
        Direction migrationDirection,
        long startDate,
        long endDate)
    {
        SetInternal(
            prominencePercent,
            prominenceValueDelta,
            population,
            sourceGroup,
            null,
            targetCell,
            migrationDirection,
            startDate,
            endDate);
    }

    protected override void ApplyProminenceChanges()
    {
        SourceGroup.AddUBandsProminenceValueDelta(-_prominenceValueDelta);
    }

    protected override BufferCulture CreateMigratingCulture()
    {
        return new BufferCulture(SourceGroup.Culture);
    }

    protected override void MergeIntoGroup(CellGroup targetGroup)
    {
        float prominenceDelta = Population / (float)targetGroup.Population;

        float percentageOfPopulation = Population / (float)(targetGroup.Population + Population);

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
