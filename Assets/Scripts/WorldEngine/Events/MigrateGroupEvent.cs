using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class MigrateGroupEvent : CellGroupEvent {

	#if DEBUG
	public static int MigrationEventCount = 0;
	#endif

	[XmlAttribute("TLon")]
	public int TargetCellLongitude;
	[XmlAttribute("TLat")]
	public int TargetCellLatitude;

	[XmlAttribute("MigDir")]
	public int MigrationDirectionInt;

	[XmlIgnore]
	public TerrainCell TargetCell;

	[XmlIgnore]
	public Direction MigrationDirection;

	public MigrateGroupEvent () {

		#if DEBUG
		MigrationEventCount++;
		#endif

		DoNotSerialize = true;
	}

	public MigrateGroupEvent (CellGroup group, TerrainCell targetCell, Direction migrationDirection, long triggerDate) : base (group, triggerDate, MigrateGroupEventId) {

		#if DEBUG
		MigrationEventCount++;
		#endif

		TargetCell = targetCell;

		TargetCellLongitude = TargetCell.Longitude;
		TargetCellLatitude = TargetCell.Latitude;

		MigrationDirection = migrationDirection;

		DoNotSerialize = true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		if (Group.TotalMigrationValue <= 0)
			return false;

		return true;
	}

	public override void Trigger () {

		if (Group.TotalMigrationValue <= 0) {

			throw new System.Exception ("Total Migration Value equal or less than zero: " + Group.TotalMigrationValue);
		}

		//		#if DEBUG
		//		if (Manager.RegisterDebugEvent != null) {
		//			if ((Group.Id == Manager.TracingData.GroupId) && (World.CurrentDate == 321798)) {
		//				bool debug = true;
		//			}
		//		}
		//		#endif

		float randomFactor = Group.Cell.GetNextLocalRandomFloat (RngOffsets.EVENT_TRIGGER + (int)Id);
		float percentToMigrate = (1 - Group.MigrationValue/Group.TotalMigrationValue) * randomFactor;
		percentToMigrate = Mathf.Pow (percentToMigrate, 4);

		percentToMigrate = Mathf.Clamp01 (percentToMigrate);

		//		#if DEBUG
		//		if (Manager.RegisterDebugEvent != null) {
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

		MigratingGroup migratingGroup = new MigratingGroup (World, percentToMigrate, Group, TargetCell, MigrationDirection);

		World.AddMigratingGroup (migratingGroup);
	}

	public override void Synchronize ()
	{
		MigrationDirectionInt = (int)MigrationDirection;
	}

	public override void FinalizeLoad () {

		MigrationDirection = (Direction)MigrationDirectionInt;

		base.FinalizeLoad ();

		TargetCell = World.TerrainCells[TargetCellLongitude][TargetCellLatitude];

		Group.MigrationEvent = this;
	}

	protected override void DestroyInternal () {

		#if DEBUG
		MigrationEventCount--;
		#endif

		if (Group != null) {
			Group.HasMigrationEvent = false;
		}

		base.DestroyInternal ();
	}

	public void Reset (TerrainCell targetCell, Direction migrationDirection, long triggerDate) {

		#if DEBUG
		MigrationEventCount++;
		#endif

		TargetCell = targetCell;

		TargetCellLongitude = TargetCell.Longitude;
		TargetCellLatitude = TargetCell.Latitude;

		MigrationDirection = migrationDirection;

		Reset (triggerDate);

		//		#if DEBUG
		//		GenerateDebugMessage ();
		//		#endif
	}
}
