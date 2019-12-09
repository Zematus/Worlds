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

public class MigrateGroupEvent : CellGroupEvent
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

    public MigrateGroupEvent()
    {
        DoNotSerialize = true;
    }

    public MigrateGroupEvent(
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

        //		#if DEBUG
        //		if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0)) {
        //			if ((Group.Id == Manager.TracingData.GroupId) ||
        //				((TargetCell.Group != null) && (TargetCell.Group.Id == Manager.TracingData.GroupId))) {
        //				CellGroup targetGroup = TargetCell.Group;
        //				string targetGroupId = "Id:" + targetGroup.Id + "|Long:" + targetGroup.Longitude + "|Lat:" + targetGroup.Latitude;
        //				string sourceGroupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;
        //
        //				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //					"MigrateGroupEvent.Trigger - Id: " + Id + ", targetGroup:" + targetGroupId + 
        //					", sourceGroup:" + sourceGroupId,
        //					"CurrentDate: " + World.CurrentDate + 
        //					", targetGroup.Population: " + targetGroup.Population + 
        //					", randomFactor: " + randomFactor + 
        //					", Group.MigrationValue: " + Group.MigrationValue + 
        //					", Group.TotalMigrationValue: " + Group.TotalMigrationValue + 
        //					", percentToMigrate: " + percentToMigrate + 
        //					"");
        //
        //				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
        //			}
        //		}
        //		#endif

        if (Group.MigratingGroup == null)
        {
            Group.MigratingGroup = new MigratingGroup(World, percentToMigrate, Group, TargetCell, MigrationDirection);
        }
        else
        {
            Group.MigratingGroup.Set(percentToMigrate, Group, TargetCell, MigrationDirection);
        }

        World.AddMigratingGroup(Group.MigratingGroup);
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

        Group.MigrationEvent = this;
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

        //		#if DEBUG
        //		GenerateDebugMessage ();
        //		#endif
    }
}
