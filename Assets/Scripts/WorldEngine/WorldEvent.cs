using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class WorldEvent : ISynchronizable {

	public static int EventCount = 0;

	[XmlIgnore]
	public World World;
	
	[XmlAttribute]
	public int TriggerDate;
	
	[XmlAttribute]
	public long Id;

	public WorldEvent () {

		EventCount++;

		Manager.UpdateWorldLoadTrackEventCount ();
	}

	public WorldEvent (World world, int triggerDate, long id) {
		
		EventCount++;

		World = world;
		TriggerDate = triggerDate;

//		Id = World.GenerateEventId ();
		Id = id;
	}

	public virtual bool CanTrigger () {

		return true;
	}

	public virtual void Synchronize () {

	}

	public virtual void FinalizeLoad () {
		
	}

	public abstract void Trigger ();

	public void Destroy () {
		
		EventCount--;

		DestroyInternal ();
	}

	protected virtual void DestroyInternal () {
	
	}
}

public abstract class CellEvent : WorldEvent {

	[XmlAttribute]
	public int CellLongitude;
	[XmlAttribute]
	public int CellLatitude;

	[XmlIgnore]
	public TerrainCell Cell;

	public CellEvent () {

	}

	public CellEvent (TerrainCell cell, int triggerDate, long eventTypeId) : base (cell.World, triggerDate, cell.GenerateUniqueIdentifier (1000, eventTypeId)) {

		Cell = cell;
		CellLongitude = cell.Longitude;
		CellLatitude = cell.Latitude;

		#if DEBUG
		if (Manager.RegisterDebugEvent != null) {
			string cellLoc = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;

			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("CellEvent - Cell: " + cellLoc, "TriggerDate: " + TriggerDate);

			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
		}
		#endif
	}
}

public class FarmDegradationEvent : CellEvent {

	public const long FarmDegradationEventId = 0;

	public const string EventSetFlag = "FarmDegradationEvent_Set";
	
	public const int MaxDateSpanToTrigger = CellGroup.GenerationTime * 8;

	public const float DegradationFactor = 0.25f;
	public const float MinFarmLandPercentage = 0.001f;

	public FarmDegradationEvent () {

	}

	public FarmDegradationEvent (TerrainCell cell, int triggerDate) : base (cell, triggerDate, FarmDegradationEventId) {

		cell.SetFlag (EventSetFlag);
	}

	public static bool CanSpawnIn (TerrainCell cell) {

		if (cell.IsFlagSet (EventSetFlag))
			return false;

		if (cell.FarmlandPercentage <= 0)
			return false;

		CellGroup cellGroup = cell.Group;

		if (cellGroup == null)
			return true;

		if (!cellGroup.StillPresent)
			return true;

		if (cellGroup.Culture.GetKnowledge (AgricultureKnowledge.AgricultureKnowledgeId) == null)
			return true;

		return false;
	}

	public static int CalculateTriggerDate (TerrainCell cell) {

		float randomFactor = cell.GetNextLocalRandomFloat (RngOffsets.FARM_DEGRADATION_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = randomFactor * randomFactor;

		return cell.World.CurrentDate + (int)Mathf.Max(1, (MaxDateSpanToTrigger * (1 - randomFactor)));
	}

	public override void Trigger ()
	{
		float farmlandDegradation = DegradationFactor * Cell.FarmlandPercentage;


		float farmlandPercentage = Cell.FarmlandPercentage - farmlandDegradation;

		if (farmlandPercentage < MinFarmLandPercentage)
			farmlandPercentage = 0;

		#if DEBUG
		if (float.IsNaN(farmlandPercentage)) {

			Debug.Break ();
		}
		#endif

		Cell.FarmlandPercentage = farmlandPercentage;
	}

	public override bool CanTrigger () {

		return (Cell.FarmlandPercentage > 0);
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Cell = World.GetCell (CellLongitude, CellLatitude);

		if (Cell == null) {
		
			throw new System.Exception ("Cell is null");
		}
	}

	protected override void DestroyInternal ()
	{
		Cell.UnsetFlag (EventSetFlag);

		if (CanSpawnIn (Cell)) {

			int nextTriggerDate = CalculateTriggerDate (Cell);

			World.InsertEventToHappen (new FarmDegradationEvent (Cell, nextTriggerDate));
		}

		base.DestroyInternal ();
	}
}

public abstract class CellGroupEvent : WorldEvent {
	
	[XmlAttribute]
	public long GroupId;
	
	[XmlIgnore]
	public CellGroup Group;
	
	public CellGroupEvent () {
		
	}
	
	public CellGroupEvent (CellGroup group, int triggerDate, long eventTypeId) : base (group.World, triggerDate, group.GenerateUniqueIdentifier (1000, eventTypeId)) {
		
		Group = group;
		GroupId = Group.Id;

		//TODO: Evaluate if necessary or remove
//		Group.AddAssociatedEvent (this);

		#if DEBUG
		if (Manager.RegisterDebugEvent != null) {
//			if (Group.Id == Manager.TracingData.GroupId) {
				string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("CellGroupEvent - Group:" + groupId + ", Type: " + this.GetType (), "TriggerDate: " + TriggerDate);

				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
		}
		#endif
	}

	public override bool CanTrigger () {

		if (Group == null)
			return false;
		
		return Group.StillPresent;
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();
		
		Group = World.GetGroup (GroupId);

		//TODO: Evaluate if necessary or remove
//		Group.AddAssociatedEvent (this);
	}
	
	protected override void DestroyInternal ()
	{
		if (Group == null)
			return;

		//TODO: Evaluate if necessary or remove
//		Group.RemoveAssociatedEvent (Id);
	}
}

public class UpdateCellGroupEvent : CellGroupEvent {

	public const long UpdateCellGroupEventId = 1;

	public UpdateCellGroupEvent () {

	}

	public UpdateCellGroupEvent (CellGroup group, int triggerDate) : base (group, triggerDate, UpdateCellGroupEventId) {
		
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ()) {

			return false;
		}

		if (Group.NextUpdateDate != TriggerDate) {

			return false;
		}

		return true;
	}

	public override void Trigger () {

		World.AddGroupToUpdate (Group);
	}
}

public class MigrateGroupEvent : CellGroupEvent {

	public const long MigrateGroupEventId = 2;
	
	public static int MigrationEventCount = 0;

	[XmlAttribute]
	public int TargetCellLongitude;
	[XmlAttribute]
	public int TargetCellLatitude;

	[XmlIgnore]
	public TerrainCell TargetCell;
	
	public MigrateGroupEvent () {
		
		MigrationEventCount++;
	}
	
	public MigrateGroupEvent (CellGroup group, TerrainCell targetCell, int triggerDate) : base (group, triggerDate, MigrateGroupEventId) {
		
		MigrationEventCount++;

		TargetCell = targetCell;

		TargetCellLongitude = TargetCell.Longitude;
		TargetCellLatitude = TargetCell.Latitude;
	}
	
	public override bool CanTrigger () {

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
//					"MigrateGroupEvent.Trigger - targetGroup:" + targetGroupId + 
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

		MigratingGroup migratingGroup = new MigratingGroup (World, percentToMigrate, Group, TargetCell);

		World.AddMigratingGroup (migratingGroup);
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		TargetCell = World.TerrainCells[TargetCellLongitude][TargetCellLatitude];
	}

	protected override void DestroyInternal ()
	{
		MigrationEventCount--;

		if (Group != null) {
			
			Group.HasMigrationEvent = false;
		}
	}
}

public class SailingDiscoveryEvent : CellGroupEvent {

	public const long SailingDiscoveryEventId = 3;

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 10000;

	public const int MinShipBuildingKnowledgeSpawnEventValue = ShipbuildingKnowledge.MinKnowledgeValueForSailingSpawnEvent;
	public const int MinShipBuildingKnowledgeValue = ShipbuildingKnowledge.MinKnowledgeValueForSailing;
	public const int OptimalShipBuildingKnowledgeValue = ShipbuildingKnowledge.OptimalKnowledgeValueForSailing;

	public const string EventSetFlag = "SailingDiscoveryEvent_Set";
	
	public SailingDiscoveryEvent () {
		
	}
	
	public SailingDiscoveryEvent (CellGroup group, int triggerDate) : base (group, triggerDate, SailingDiscoveryEventId) {
		
		Group.SetFlag (EventSetFlag);
	}
	
	public static int CalculateTriggerDate (CellGroup group) {
		
		float shipBuildingValue = 0;
		
		CulturalKnowledge shipbuildingKnowledge = group.Culture.GetKnowledge (ShipbuildingKnowledge.ShipbuildingKnowledgeId);
		
		if (shipbuildingKnowledge != null)
			shipBuildingValue = shipbuildingKnowledge.Value;

		float randomFactor = group.Cell.GetNextLocalRandomFloat (RngOffsets.SAILING_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = randomFactor * randomFactor;

		float shipBuildingFactor = (shipBuildingValue - MinShipBuildingKnowledgeValue) / (float)(OptimalShipBuildingKnowledgeValue - MinShipBuildingKnowledgeValue);
		shipBuildingFactor = Mathf.Clamp01 (shipBuildingFactor) + 0.001f;

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / shipBuildingFactor;

		int targetCurrentDate = (int)Mathf.Min (int.MaxValue, group.World.CurrentDate + dateSpan);

		if (targetCurrentDate <= group.World.CurrentDate)
			targetCurrentDate = int.MaxValue;

		return targetCurrentDate;
	}
	
	public static bool CanSpawnIn (CellGroup group) {

		if (group.Culture.GetDiscovery (SailingDiscovery.SailingDiscoveryId) != null)
			return false;

		if (group.IsFlagSet (EventSetFlag))
			return false;
		
		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;
		
		CulturalDiscovery discovery = Group.Culture.GetDiscovery (SailingDiscovery.SailingDiscoveryId);
		
		if (discovery != null)
			return false;
		
		CulturalKnowledge shipbuildingKnowledge = Group.Culture.GetKnowledge (ShipbuildingKnowledge.ShipbuildingKnowledgeId);
		
		if (shipbuildingKnowledge == null)
			return false;

		if (shipbuildingKnowledge.Value < MinShipBuildingKnowledgeValue)
		    return false;

		return true;
	}

	public override void Trigger () {

		Group.Culture.AddDiscoveryToFind (new SailingDiscovery ());
		World.AddGroupToUpdate (Group);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}
}

public class TribalismDiscoveryEvent : CellGroupEvent {

	public const int TribalismDiscoveryEventId = 4;

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 20000;

	public const int MinSocialOrganizationKnowledgeSpawnEventValue = SocialOrganizationKnowledge.MinKnowledgeValueForTribalismSpawnEvent;
	public const int MinSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.MinKnowledgeValueForTribalism;
	public const int OptimalSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.OptimalKnowledgeValueForTribalism;

	public const string EventSetFlag = "TribalismDiscoveryEvent_Set";

	public TribalismDiscoveryEvent () {

	}

	public TribalismDiscoveryEvent (CellGroup group, int triggerDate) : base (group, triggerDate, TribalismDiscoveryEventId) {

		Group.SetFlag (EventSetFlag);
	}

	public static int CalculateTriggerDate (CellGroup group) {

		float socialOrganizationValue = 0;

		CulturalKnowledge socialOrganizationKnowledge = group.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge != null)
			socialOrganizationValue = socialOrganizationKnowledge.Value;

		float randomFactor = group.Cell.GetNextLocalRandomFloat (RngOffsets.TRIBALISM_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = randomFactor * randomFactor;

		float socialOrganizationFactor = (socialOrganizationValue - MinSocialOrganizationKnowledgeValue) / (float)(OptimalSocialOrganizationKnowledgeValue - MinSocialOrganizationKnowledgeValue);
		socialOrganizationFactor = Mathf.Clamp01 (socialOrganizationFactor) + 0.001f;

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / socialOrganizationFactor;

		int targetCurrentDate = (int)Mathf.Min (int.MaxValue, group.World.CurrentDate + dateSpan);

		if (targetCurrentDate <= group.World.CurrentDate)
			targetCurrentDate = int.MaxValue;

		return targetCurrentDate;
	}

	public static bool CanSpawnIn (CellGroup group) {

		if (group.Culture.GetDiscovery (TribalismDiscovery.TribalismDiscoveryId) != null)
			return false;

		if (group.IsFlagSet (EventSetFlag))
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		CulturalDiscovery discovery = Group.Culture.GetDiscovery (TribalismDiscovery.TribalismDiscoveryId);

		if (discovery != null)
			return false;

		CulturalKnowledge socialOrganizationKnowledge = Group.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge == null)
			return false;

		if (socialOrganizationKnowledge.Value < MinSocialOrganizationKnowledgeValue)
			return false;

		return true;
	}

	public override void Trigger () {

		Group.Culture.AddDiscoveryToFind (new TribalismDiscovery ());

		if (Group.GetPolityInfluencesCount () <= 0) {

			World.AddPolity (Tribe.GenerateNewTribe (Group));
		}

		World.AddGroupToUpdate (Group);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}
}

public class TribeFormationEvent : CellGroupEvent {

	public const long TribeFormationEventId = 5;

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 1000;

	public const int MinSocialOrganizationKnowledgeSpawnEventValue = SocialOrganizationKnowledge.MinKnowledgeValueForTribalismSpawnEvent;
	public const int MinSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.MinKnowledgeValueForTribalism;
	public const int OptimalSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.OptimalKnowledgeValueForTribalism;

	public const string EventSetFlag = "TribeFormationEvent_Set";

	public TribeFormationEvent () {

	}

	public TribeFormationEvent (CellGroup group, int triggerDate) : base (group, triggerDate, TribeFormationEventId) {

		Group.SetFlag (EventSetFlag);
	}

	public static int CalculateTriggerDate (CellGroup group) {

		float socialOrganizationValue = 0;

		CulturalKnowledge socialOrganizationKnowledge = group.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge != null)
			socialOrganizationValue = socialOrganizationKnowledge.Value;

		float randomFactor = group.Cell.GetNextLocalRandomFloat (RngOffsets.TRIBE_FORMATION_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = randomFactor * randomFactor;

		float socialOrganizationFactor = (socialOrganizationValue - MinSocialOrganizationKnowledgeValue) / (OptimalSocialOrganizationKnowledgeValue - MinSocialOrganizationKnowledgeValue);
		socialOrganizationFactor = Mathf.Clamp01 (socialOrganizationFactor) + 0.001f;

		float influenceFactor = group.TotalPolityInfluenceValue;
		influenceFactor = Mathf.Pow(1 - influenceFactor * 0.95f, 4);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / (socialOrganizationFactor * influenceFactor);

		int targetDate = (int)Mathf.Min (int.MaxValue, group.World.CurrentDate + dateSpan);

		if (targetDate <= group.World.CurrentDate)
			targetDate = int.MaxValue;

		return targetDate;
	}

	public static bool CanSpawnIn (CellGroup group) {

		if (group.IsFlagSet (EventSetFlag))
			return false;

		if (group.Culture.GetDiscovery (TribalismDiscovery.TribalismDiscoveryId) == null)
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		CulturalDiscovery discovery = Group.Culture.GetDiscovery (TribalismDiscovery.TribalismDiscoveryId);

		if (discovery == null)
			return false;

		float influenceFactor = Mathf.Min(1, Group.TotalPolityInfluenceValue * 3f);
		influenceFactor = Mathf.Pow (1 - influenceFactor, 4);

		float rollValue = Group.Cell.GetNextLocalRandomFloat (RngOffsets.EVENT_CAN_TRIGGER + (int)Id);

		if (rollValue > influenceFactor)
			return false;

		return true;
	}

	public override void Trigger () {

		World.AddPolity (Tribe.GenerateNewTribe (Group));

		World.AddGroupToUpdate (Group);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}
}

public class BoatMakingDiscoveryEvent : CellGroupEvent {

	public const long BoatMakingDiscoveryEventId = 6;
	
	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 10000;
	
	public BoatMakingDiscoveryEvent () {
		
	}
	
	public BoatMakingDiscoveryEvent (CellGroup group, int triggerDate) : base (group, triggerDate, BoatMakingDiscoveryEventId) {

	}
	
	public static int CalculateTriggerDate (CellGroup group) {
		
		float oceanPresence = ShipbuildingKnowledge.CalculateNeighborhoodOceanPresenceIn (group);

		float randomFactor = group.Cell.GetNextLocalRandomFloat (RngOffsets.BOAT_MAKING_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = randomFactor * randomFactor;

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant;

		if (oceanPresence > 0) {

			dateSpan /= oceanPresence;
		} else {

			throw new System.Exception ("Can't calculate valid trigger date");
		}

		int targetCurrentDate = (int)Mathf.Min (int.MaxValue, group.World.CurrentDate + dateSpan);

		if (targetCurrentDate <= group.World.CurrentDate)
			targetCurrentDate = int.MaxValue;

		return targetCurrentDate;
	}
	
	public static bool CanSpawnIn (CellGroup group) {
		
		if (group.Culture.GetKnowledge (ShipbuildingKnowledge.ShipbuildingKnowledgeId) != null)
			return false;
		
		float oceanPresence = ShipbuildingKnowledge.CalculateNeighborhoodOceanPresenceIn (group);
		
		return (oceanPresence > 0);
	}
	
	public override bool CanTrigger () {
		
		if (!base.CanTrigger ())
			return false;
		
		if (Group.Culture.GetKnowledge (ShipbuildingKnowledge.ShipbuildingKnowledgeId) != null)
			return false;
		
		return true;
	}
	
	public override void Trigger () {
		
		Group.Culture.AddDiscoveryToFind (new BoatMakingDiscovery ());
		Group.Culture.AddKnowledgeToLearn (new ShipbuildingKnowledge (Group));
		World.AddGroupToUpdate (Group);
	}
}

public class PlantCultivationDiscoveryEvent : CellGroupEvent {

	public const long PlantCultivationDiscoveryEventId = 7;

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 600000;

	public PlantCultivationDiscoveryEvent () {

	}

	public PlantCultivationDiscoveryEvent (CellGroup group, int triggerDate) : base (group, triggerDate, PlantCultivationDiscoveryEventId) {

	}

	public static int CalculateTriggerDate (CellGroup group) {

		float terrainFactor = AgricultureKnowledge.CalculateTerrainFactorIn (group.Cell);

		float randomFactor = group.Cell.GetNextLocalRandomFloat (RngOffsets.PLANT_CULTIVATION_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = randomFactor * randomFactor;

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant;

		if (terrainFactor > 0) {

			dateSpan /= terrainFactor;
		} else {

			throw new System.Exception ("Can't calculate valid trigger date");
		}

		int targetCurrentDate = (int)Mathf.Min (int.MaxValue, group.World.CurrentDate + dateSpan);

		if (targetCurrentDate <= group.World.CurrentDate)
			targetCurrentDate = int.MaxValue;

		return targetCurrentDate;
	}

	public static bool CanSpawnIn (CellGroup group) {

		if (group.Culture.GetKnowledge (AgricultureKnowledge.AgricultureKnowledgeId) != null)
			return false;

		float terrainFactor = AgricultureKnowledge.CalculateTerrainFactorIn (group.Cell);

		return (terrainFactor > 0);
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		if (Group.Culture.GetKnowledge (AgricultureKnowledge.AgricultureKnowledgeId) != null)
			return false;

		return true;
	}

	public override void Trigger () {

		Group.Culture.AddActivityToPerform (CellCulturalActivity.CreateFarmingActivity (Group));
		Group.Culture.AddDiscoveryToFind (new PlantCultivationDiscovery ());
		Group.Culture.AddKnowledgeToLearn (new AgricultureKnowledge (Group));
		World.AddGroupToUpdate (Group);
	}
}
