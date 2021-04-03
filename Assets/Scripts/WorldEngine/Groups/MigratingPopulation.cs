using UnityEngine;

/// <summary>
/// Segment of unorganized bands migrating from one cell to another
/// </summary>
public abstract class MigratingPopulation
{
    public long StartDate = 0;
    public long EndDate = 0;

    public Polity Polity;

    public float ProminencePercent;

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

    protected float _prominenceValueDelta;

    /// <summary>
    /// Constructs a new migrating population object
    /// </summary>
    /// <param name="world">world this object belongs to</param>
    /// <param name="prominencePercent">percentage of the source prominence to migrate</param>
    /// <param name="prominenceValueDelta">how much the prominence value should change</param>
    /// <param name="population">population to migrate</param>
    /// <param name="sourceGroup">the cell group this originates from</param>
    /// <param name="polity">the polity to which the migrating population belongs</param>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is moving out from
    /// the source</param>
    /// <param name="startDate">the migration start date</param>
    /// <param name="endDate">the migration end date</param>
    public MigratingPopulation(
        World world,
        float prominencePercent,
        float prominenceValueDelta,
        int population,
        CellGroup sourceGroup,
        Polity polity,
        TerrainCell targetCell,
        Direction migrationDirection,
        long startDate,
        long endDate)
    {
        World = world;

        SetInternal(
            prominencePercent,
            prominenceValueDelta,
            population,
            sourceGroup,
            polity,
            targetCell,
            migrationDirection,
            startDate,
            endDate);
    }

    /// <summary>
    /// Sets the object properties to use during a migration event
    /// </summary>
    /// <param name="prominencePercent">percentage of the source prominence to migrate</param>
    /// <param name="prominenceValueDelta">how much the prominence value should change</param>
    /// <param name="population">population to migrate</param>
    /// <param name="sourceGroup">the cell group this originates from</param>
    /// <param name="targetCell">the cell group this migrates to</param>
    /// <param name="migrationDirection">the direction this group is moving out from
    /// the source</param>
    /// <param name="startDate">the migration start date</param>
    /// <param name="endDate">the migration end date</param>
    protected void SetInternal(
        float prominencePercent,
        float prominenceValueDelta,
        int population,
        CellGroup sourceGroup,
        Polity polity,
        TerrainCell targetCell,
        Direction migrationDirection,
        long startDate,
        long endDate)
    {
        MigrationDirection = migrationDirection;

        if (float.IsNaN(prominencePercent))
        {
            throw new System.Exception("percentPopulation value is invalid: " + prominencePercent);
        }

        ProminencePercent = prominencePercent;

        _prominenceValueDelta = prominenceValueDelta;

        Population = population;

        if (Population <= 0)
        {
            throw new System.Exception(
                "The population to migrate from the source group " +
                SourceGroupId + " is equal or less than 0: " + Population);
        }

        TargetCell = targetCell;
        SourceGroup = sourceGroup;

        Polity = polity;

        SourceGroupId = SourceGroup.Id;

        TargetCellLongitude = TargetCell.Longitude;
        TargetCellLatitude = TargetCell.Latitude;

        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Applies migration related changes to the source group
    /// </summary>
    public void SplitFromSourceGroup()
    {
        if ((SourceGroup == null) || !SourceGroup.StillPresent)
        {
            throw new System.Exception(
                "The source group " + SourceGroupId + " is null or no longer present");
        }

        Culture = CreateMigratingCulture();

        ApplyProminenceChanges();

        SourceGroup.ChangePopulation(-Population);
    }

    /// <summary>
    /// Modifies a source group based on the prominence changes
    /// </summary>
    protected abstract void ApplyProminenceChanges();

    /// <summary>
    /// Create a buffer culture from the source group
    /// </summary>
    /// <returns>a buffer culture object</returns>
    protected abstract BufferCulture CreateMigratingCulture();

    /// <summary>
    /// Applies migration related changes to the source group
    /// </summary>
    public void MoveToCell()
    {
        CellGroup targetGroup = TargetCell.Group;

        if ((targetGroup != null) && (targetGroup.StillPresent))
        {
            MergeIntoGroup(targetGroup);
        }
        else
        {
            targetGroup = CreateGroupOnTarget();

            World.AddGroup(targetGroup);
        }

        if (SourceGroup.MigrationTagged)
        {
            World.MigrationTagGroup(targetGroup);
        }

        if (targetGroup.LastPopulationMigration == null)
        {
            targetGroup.LastPopulationMigration = new MigratingPopulationSnapshot();
        }

        targetGroup.LastPopulationMigration.Set(
            Population, SourceGroup, Polity?.Info, StartDate, EndDate);
    }

    /// <summary>
    /// Merges the migrating population into the target group
    /// </summary>
    /// <param name="targetGroup">group to merge population into</param>
    protected abstract void MergeIntoGroup(CellGroup targetGroup);

    /// <summary>
    /// Creates a cell group on the target cell
    /// </summary>
    /// <returns>The newly created group</returns>
    protected abstract CellGroup CreateGroupOnTarget();
}
