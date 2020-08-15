using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public enum MigrationType
{
    Land = 0,
    Sea = 1
}

public class MigrateBandsEvent : CellGroupEvent
{
    [XmlAttribute("TLon")]
    public int TargetCellLongitude;
    [XmlAttribute("TLat")]
    public int TargetCellLatitude;

    [XmlAttribute("MigDir")]
    public int MigrationDirectionInt;

    [XmlAttribute("MigType")]
    public int MigrationTypeInt;

    [XmlIgnore]
    public TerrainCell TargetCell;

    [XmlIgnore]
    public Direction MigrationDirection;

    [XmlIgnore]
    public MigrationType MigrationType;

    public MigrateBandsEvent()
    {
        DoNotSerialize = true;
    }

    public MigrateBandsEvent(
        CellGroup group,
        TerrainCell targetCell,
        Direction migrationDirection,
        MigrationType migrationType,
        long triggerDate,
        long originalSpawnDate = - 1) : 
        base(group, triggerDate, MigrateGroupEventId, originalSpawnDate: originalSpawnDate)
    {
        TargetCell = targetCell;

        TargetCellLongitude = TargetCell.Longitude;
        TargetCellLatitude = TargetCell.Latitude;

        MigrationDirection = migrationDirection;
        MigrationType = migrationType;

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
    }

    public override void FinalizeLoad()
    {
        MigrationDirection = (Direction)MigrationDirectionInt;
        MigrationType = (MigrationType)MigrationTypeInt;

        base.FinalizeLoad();

        TargetCell = World.TerrainCells[TargetCellLongitude][TargetCellLatitude];

        Group.BandMigrationEvent = this;
    }

    protected override void DestroyInternal()
    {
        if (Group != null)
        {
            Group.HasBandMigrationEvent = false;

            if (MigrationType == MigrationType.Sea)
            {
                Group.ResetSeaMigrationRoute();
            }
        }

        base.DestroyInternal();
    }

    public void Reset(
        TerrainCell targetCell,
        Direction migrationDirection,
        MigrationType migrationType,
        long triggerDate)
    {
        TargetCell = targetCell;

        TargetCellLongitude = TargetCell.Longitude;
        TargetCellLatitude = TargetCell.Latitude;

        MigrationDirection = migrationDirection;
        MigrationType = migrationType;

        Reset(triggerDate);
    }
}
