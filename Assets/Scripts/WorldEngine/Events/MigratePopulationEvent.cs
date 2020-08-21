using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

/// <summary>
/// Identifies if the migration is over land or water
/// </summary>
public enum MigrationType
{
    Land = 0,
    Sea = 1
}

/// <summary>
/// Defines a generic population migration event
/// </summary>
public class MigratePopulationEvent : CellGroupEvent
{
    [XmlAttribute("TLon")]
    public int TargetCellLongitude;
    [XmlAttribute("TLat")]
    public int TargetCellLatitude;

    [XmlAttribute("MDir")]
    public int MigrationDirectionInt;

    [XmlAttribute("MTyp")]
    public int MigrationTypeInt;

    [XmlAttribute("MPTyp")]
    public int MigratingPopTypeInt;

    [XmlIgnore]
    public TerrainCell TargetCell;

    [XmlIgnore]
    public Direction MigrationDirection;

    [XmlIgnore]
    public MigrationType MigrationType;

    [XmlIgnore]
    public MigratingPopulationType MigratingPopType;

    /// <summary>
    /// Deserialization constructor
    /// </summary>
    public MigratePopulationEvent()
    {
        throw new System.InvalidOperationException("This type is not serializable");
    }

    /// <summary>
    /// constructs a new population migration event
    /// </summary>
    /// <param name="group">the group the migration originates from</param>
    /// <param name="targetCell">the cell where the migration will stop</param>
    /// <param name="migrationDirection">the direction the migration will arrive to the target</param>
    /// <param name="migrationType">the type of migration: 'land' or 'sea'</param>
    /// <param name="migratingPopType">the type of migrating population:
    /// 'unorganized bands' or 'tribe'</param>
    /// <param name="triggerDate">the date this event will trigger</param>
    /// <param name="originalSpawnDate">the date this event was initiated</param>
    public MigratePopulationEvent(
        CellGroup group,
        TerrainCell targetCell,
        Direction migrationDirection,
        MigrationType migrationType,
        MigratingPopulationType migratingPopType,
        long triggerDate,
        long originalSpawnDate = - 1) : 
        base(group, triggerDate, MigrateGroupEventId, originalSpawnDate: originalSpawnDate)
    {
        Set(targetCell, migrationDirection, migrationType, migratingPopType);

        DoNotSerialize = true;
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
            return false;

        if (Group.TotalMigrationValue <= 0)
            return false;

        return true;
    }

    public override void Trigger()
    {
        if (Group.TotalMigrationValue <= 0)
        {
            throw new System.Exception("Total Migration Value equal or less than zero: " + Group.TotalMigrationValue);
        }

        float randomFactor = Group.Cell.GetNextLocalRandomFloat(RngOffsets.EVENT_TRIGGER + unchecked((int)Id));
        float percentToMigrate = (1 - Group.MigrationValue / Group.TotalMigrationValue) * randomFactor;
        percentToMigrate = Mathf.Pow(percentToMigrate, 4);

        percentToMigrate = Mathf.Clamp01(percentToMigrate);

        Group.SetMigratingBands(percentToMigrate, TargetCell, MigrationDirection);

        World.AddMigratingBands(Group.MigratingBands);
    }

    public override void Synchronize()
    {
        MigrationDirectionInt = (int)MigrationDirection;
        MigrationTypeInt = (int)MigrationType;
        MigratingPopTypeInt = (int)MigratingPopType;
    }

    public override void FinalizeLoad()
    {
        MigrationDirection = (Direction)MigrationDirectionInt;
        MigrationType = (MigrationType)MigrationTypeInt;
        MigratingPopType = (MigratingPopulationType)MigratingPopTypeInt;

        base.FinalizeLoad();

        TargetCell = World.TerrainCells[TargetCellLongitude][TargetCellLatitude];

        Group.PopulationMigrationEvent = this;
    }

    protected override void DestroyInternal()
    {
        if (Group != null)
        {
            Group.HasMigrationEvent = false;

            if (MigrationType == MigrationType.Sea)
            {
                Group.ResetSeaMigrationRoute();
            }
        }

        base.DestroyInternal();
    }

    /// <summary>
    /// Resets all properties of the event to be able to reuse the object
    /// </summary>
    /// <param name="targetCell">the cell where the migration will stop</param>
    /// <param name="migrationDirection">the direction the migration will arrive to the target</param>
    /// <param name="migrationType">the type of migration: 'land' or 'sea'</param>
    /// <param name="migratingPopType">the type of migrating population:
    /// 'unorganized bands' or 'tribe'</param>
    /// <param name="triggerDate">the date this event will trigger</param>
    public void Reset(
        TerrainCell targetCell,
        Direction migrationDirection,
        MigrationType migrationType,
        MigratingPopulationType migratingPopType,
        long triggerDate)
    {
        Set(targetCell, migrationDirection, migrationType, migratingPopType);

        Reset(triggerDate);
    }

    /// <summary>
    /// Sets most properties of this event
    /// </summary>
    /// <param name="targetCell">the cell where the migration will stop</param>
    /// <param name="migrationDirection">the direction the migration will arrive to the target</param>
    /// <param name="migrationType">the type of migration: 'land' or 'sea'</param>
    /// <param name="migratingPopType">the type of migrating population:
    /// 'unorganized bands' or 'tribe'</param>
    private void Set(
        TerrainCell targetCell,
        Direction migrationDirection,
        MigrationType migrationType,
        MigratingPopulationType migratingPopType)
    {
        TargetCell = targetCell;

        TargetCellLongitude = TargetCell.Longitude;
        TargetCellLatitude = TargetCell.Latitude;

        MigrationDirection = migrationDirection;
        MigrationType = migrationType;
        MigratingPopType = migratingPopType;
    }
}
