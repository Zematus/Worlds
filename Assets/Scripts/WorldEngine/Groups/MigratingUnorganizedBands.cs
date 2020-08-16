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
    /// <param name="percentPopulation">percentage of the source group's population to migrate</param>
    /// <param name="sourceGroup">the cell group this originates from</param>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is exiting from the source</param>
    public MigratingUnorganizedBands(
        World world,
        float percentPopulation,
        CellGroup sourceGroup,
        TerrainCell targetCell,
        Direction migrationDirection)
        : base (world, percentPopulation, sourceGroup, targetCell, migrationDirection)
    {
    }

    protected override int SplitFromGroup()
    {
        return SourceGroup.SplitUnorganizedBands(this);
    }

    protected override BufferCulture CreateBufferCulture()
    {
        return new BufferCulture(SourceGroup.Culture);
    }

    protected override void MergeIntoGroup(CellGroup targetGroup)
    {
        targetGroup.MergeUnorganizedBands(this);
    }

    protected override CellGroup CreateGroupOnTarget()
    {
        return new CellGroup(this, Population);
    }
}
