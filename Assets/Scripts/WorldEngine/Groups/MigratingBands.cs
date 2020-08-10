using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

/// <summary>
/// Abstraction for unorganized bands migrating from one cell to another
/// </summary>
public class MigratingBands
{
    public float PercentPopulation;

    public int TargetCellLongitude;
    public int TargetCellLatitude;

    public int Population = 0;

    public int MigrationDirectionInt;

    public Identifier SourceGroupId;

    public BufferCulture Culture;

    public TerrainCell TargetCell;

    public CellGroup SourceGroup;

    public Direction MigrationDirection;

    public World World;

    /// <summary>
    /// Constructs a new migrating bands object
    /// </summary>
    /// <param name="world">world this object belongs to</param>
    /// <param name="percentPopulation">percentage of the source group's population to migrate</param>
    /// <param name="sourceGroup">the cell group this originates from</param>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is exiting from the source</param>
    public MigratingBands(
        World world,
        float percentPopulation,
        CellGroup sourceGroup,
        TerrainCell targetCell,
        Direction migrationDirection)
    {
        World = world;

        Set(percentPopulation, sourceGroup, targetCell, migrationDirection);
    }

    /// <summary>
    /// Sets the object properties (efectively resets it)
    /// </summary>
    /// <param name="percentPopulation">percentage of the source group's population to migrate</param>
    /// <param name="sourceGroup">the cell group this originates from</param>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is exiting from the source</param>
    public void Set(
        float percentPopulation,
        CellGroup sourceGroup,
        TerrainCell targetCell,
        Direction migrationDirection)
    {
        MigrationDirection = migrationDirection;

        PercentPopulation = percentPopulation;

        if (float.IsNaN(percentPopulation))
        {
            throw new System.Exception("float.IsNaN(percentPopulation)");
        }

        TargetCell = targetCell;
        SourceGroup = sourceGroup;

        SourceGroupId = SourceGroup.Id;

        TargetCellLongitude = TargetCell.Longitude;
        TargetCellLatitude = TargetCell.Latitude;
    }

    /// <summary>
    /// Initiates the migration process
    /// </summary>
    /// <returns>'false' if the process can't take place anymore</returns>
    public bool SplitFromSourceGroup()
    {
        if (SourceGroup == null)
            return false;

        if (!SourceGroup.StillPresent)
            return false;

        Population = SourceGroup.SplitUnorganizedBands(this);

        if (Population <= 0)
            return false;

        Culture = new BufferCulture(SourceGroup.Culture);

        return true;
    }

    /// <summary>
    /// Finalizes the migration process
    /// </summary>
    public void MoveToCell()
    {
        if (Population <= 0)
            return;

        CellGroup targetGroup = TargetCell.Group;

        if (targetGroup != null)
        {
            if (targetGroup.StillPresent)
            {
                Profiler.BeginSample("targetGroup.MergeGroup");

                targetGroup.MergeUnorganizedBands(this);

                Profiler.EndSample();

                if (SourceGroup.MigrationTagged)
                {
                    World.MigrationTagGroup(TargetCell.Group);
                }
            }
        }
        else
        {
            Profiler.BeginSample("targetGroup = new CellGroup");

            targetGroup = new CellGroup(this, Population);

            Profiler.EndSample();

            Profiler.BeginSample("World.AddGroup");

            World.AddGroup(targetGroup);

            Profiler.EndSample();

            if (SourceGroup.MigrationTagged)
            {
                World.MigrationTagGroup(targetGroup);
            }
        }
    }
}
