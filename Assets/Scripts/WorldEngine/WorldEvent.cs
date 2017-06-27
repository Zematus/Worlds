using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class WorldEventMessage {

	public const string SailingDiscoveryMessagePrefix = "Sailing discovered";
	public const string TribalismDiscoveryMessagePrefix = "Tribalism discovered";
	public const string BoatMakingDiscoveryMessagePrefix = "Boat making discovered";
	public const string PlantCultivationDiscoveryMessagePrefix = "Plant cultivation discovered";

	[XmlAttribute]
	public long Id;

	public string Message;

	public WorldEventMessage () {
	
	}

	public WorldEventMessage (long id, string message) {
	
		Id = id;
		Message = message;
	}
}

public class CellEventMessage : WorldEventMessage {

	public WorldPosition Position;

	public CellEventMessage () {
	
	}

	public CellEventMessage (TerrainCell cell, long id, string message) : base (id, message) {

		Position = cell.Position;
	}
}

public class DiscoveryEventMessage : CellEventMessage {

	[XmlAttribute]
	public string DiscoveryId;

	public DiscoveryEventMessage () {

	}

	public DiscoveryEventMessage (string discoveryId, TerrainCell cell, long id, string message) : base (cell, id, message) {

		DiscoveryId = discoveryId;
	}
}

public class WorldEventSnapshot {

	public System.Type EventType;

	public int TriggerDate;
	public int SpawnDate;
	public long Id;

	public WorldEventSnapshot (WorldEvent e) {

		EventType = e.GetType ();
	
		TriggerDate = e.TriggerDate;
		SpawnDate = e.SpawnDate;
		Id = e.Id;
	}
}

public abstract class WorldEvent : ISynchronizable {

	public const long UpdateCellGroupEventId = 0;
	public const long MigrateGroupEventId = 1;
	public const long SailingDiscoveryEventId = 2;
	public const long TribalismDiscoveryEventId = 3;
	public const long TribeFormationEventId = 4;
	public const long BoatMakingDiscoveryEventId = 5;
	public const long PlantCultivationDiscoveryEventId = 6;
	public const long ClanSplitEventId = 7;
	public const long ExpandPolityInfluenceEventId = 8;
	public const long TribeSplitEventId = 9;

//	public static int EventCount = 0;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public bool DoNotSerialize = false;
	
	[XmlAttribute]
	public int TriggerDate;

	[XmlAttribute]
	public int SpawnDate;
	
	[XmlAttribute]
	public long Id;

	public WorldEvent () {

//		EventCount++;

		Manager.UpdateWorldLoadTrackEventCount ();
	}

	public WorldEvent (World world, int triggerDate, long id) {
		
//		EventCount++;

		World = world;
		TriggerDate = triggerDate;
		SpawnDate = World.CurrentDate;

		Id = id;

		#if DEBUG
		if (Manager.RegisterDebugEvent != null) {
			if (!this.GetType ().IsSubclassOf (typeof(CellGroupEvent))) {
				string eventId = "Id: " + id;

				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("WorldEvent - Id: " + eventId, "TriggerDate: " + TriggerDate);

				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
			}
		}
		#endif
	}

	public virtual WorldEventSnapshot GetSnapshot () {
	
		return new WorldEventSnapshot (this);
	}

	public virtual bool IsStillValid () {

		return true;
	}

	public virtual bool CanTrigger () {

		return IsStillValid ();
	}

	public virtual void Synchronize () {

	}

	public virtual void FinalizeLoad () {
		
	}

	public abstract void Trigger ();

	public virtual void TryGenerateEventMessage (long id, string message) {

		if (World.HasEventMessage (id))
			return;

		World.AddEventMessage (new WorldEventMessage (id, message));
	}

	public void Destroy () {
		
//		EventCount--;

		DestroyInternal ();
	}

	protected virtual void DestroyInternal () {
	
	}

	public virtual void Reset (int newTriggerDate, long newId) {

//		EventCount++;

		TriggerDate = newTriggerDate;
		Id = newId;
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

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			string cellLoc = "Long:" + cell.Longitude + "|Lat:" + cell.Latitude;
//
//			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("CellEvent - Cell: " + cellLoc, "TriggerDate: " + TriggerDate);
//
//			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//		}
//		#endif
	}

	public override void TryGenerateEventMessage (long id, string messagePrefix) {

		if (World.HasEventMessage (id))
			return;

		World.AddEventMessage (new CellEventMessage (Cell, id, messagePrefix + " at " + Cell.Position));
	}
}

public class SailingDiscoveryEvent : CellGroupEvent {

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

		int targetDate = (int)(group.World.CurrentDate + dateSpan);

		if (targetDate <= group.World.CurrentDate)
			targetDate = int.MinValue;

		return targetDate;
	}
	
	public static bool CanSpawnIn (CellGroup group) {

		if (group.IsFlagSet (EventSetFlag))
			return false;

		if (group.Culture.GetDiscovery (SailingDiscovery.SailingDiscoveryId) != null)
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

		TryGenerateEventMessage (SailingDiscoveryEventId, WorldEventMessage.SailingDiscoveryMessagePrefix);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}

	public override void TryGenerateEventMessage (long id, string messagePrefix) {

		if (World.HasEventMessage (id))
			return;

		World.AddEventMessage (new DiscoveryEventMessage (SailingDiscovery.SailingDiscoveryId, Group.Cell, id, messagePrefix + " at " + Group.Position));
	}
}

public class TribalismDiscoveryEvent : CellGroupEvent {

	public const long EventMessageId = 0;
	public const string EventMessagePrefix = "Tribalism Discovered";

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 100;

	public const int MinSocialOrganizationKnowledgeSpawnEventValue = SocialOrganizationKnowledge.MinValueForTribalismSpawnEvent;
	public const int MinSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.MinValueForTribalism;
	public const int OptimalSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.OptimalValueForTribalism;

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
		randomFactor = Mathf.Pow (randomFactor, 2);

		float socialOrganizationFactor = (socialOrganizationValue - MinSocialOrganizationKnowledgeValue) / (float)(OptimalSocialOrganizationKnowledgeValue - MinSocialOrganizationKnowledgeValue);
		socialOrganizationFactor = Mathf.Pow (socialOrganizationFactor, 2);
		socialOrganizationFactor = Mathf.Clamp (socialOrganizationFactor, 0.001f, 1);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / socialOrganizationFactor;

		int targetDate = (int)(group.World.CurrentDate + dateSpan);

		if (targetDate <= group.World.CurrentDate)
			targetDate = int.MinValue;

		return targetDate;
	}

	public static bool CanSpawnIn (CellGroup group) {

		if (group.IsFlagSet (EventSetFlag))
			return false;

		if (group.Culture.GetFoundDiscoveryOrToFind (TribalismDiscovery.TribalismDiscoveryId) != null)
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		CulturalDiscovery discovery = Group.Culture.GetFoundDiscoveryOrToFind (TribalismDiscovery.TribalismDiscoveryId);

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

			Tribe tribe = new Tribe (Group);

			World.AddPolity (tribe);
			World.AddPolityToUpdate (tribe);
		}

		World.AddGroupToUpdate (Group);

		TryGenerateEventMessage (TribalismDiscoveryEventId, WorldEventMessage.TribalismDiscoveryMessagePrefix);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}

	public override void TryGenerateEventMessage (long id, string messagePrefix) {

		if (World.HasEventMessage (id))
			return;

		World.AddEventMessage (new DiscoveryEventMessage (TribalismDiscovery.TribalismDiscoveryId, Group.Cell, id, messagePrefix + " at " + Group.Position));
	}
}

public class BoatMakingDiscoveryEvent : CellGroupEvent {
	
	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 10000;

	public const string EventSetFlag = "BoatMakingDiscoveryEvent_Set";
	
	public BoatMakingDiscoveryEvent () {
		
	}
	
	public BoatMakingDiscoveryEvent (CellGroup group, int triggerDate) : base (group, triggerDate, BoatMakingDiscoveryEventId) {

		Group.SetFlag (EventSetFlag);
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

		int targetDate = (int)(group.World.CurrentDate + dateSpan);

		if (targetDate <= group.World.CurrentDate)
			targetDate = int.MinValue;

		return targetDate;
	}
	
	public static bool CanSpawnIn (CellGroup group) {

		if (group.IsFlagSet (EventSetFlag))
			return false;
		
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

		TryGenerateEventMessage (BoatMakingDiscoveryEventId, WorldEventMessage.BoatMakingDiscoveryMessagePrefix);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}

	public override void TryGenerateEventMessage (long id, string messagePrefix) {

		if (World.HasEventMessage (id))
			return;

		World.AddEventMessage (new DiscoveryEventMessage (BoatMakingDiscovery.BoatMakingDiscoveryId, Group.Cell, id, messagePrefix + " at " + Group.Position));
	}
}

public class PlantCultivationDiscoveryEvent : CellGroupEvent {

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 600000;

	public const string EventSetFlag = "PlantCultivationDiscoveryEvent_Set";

	public PlantCultivationDiscoveryEvent () {

	}

	public PlantCultivationDiscoveryEvent (CellGroup group, int triggerDate) : base (group, triggerDate, PlantCultivationDiscoveryEventId) {

		Group.SetFlag (EventSetFlag);
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

		int targetDate = (int)(group.World.CurrentDate + dateSpan);

		if (targetDate <= group.World.CurrentDate)
			targetDate = int.MinValue;

		return targetDate;
	}

	public static bool CanSpawnIn (CellGroup group) {

		if (group.IsFlagSet (EventSetFlag))
			return false;

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

		TryGenerateEventMessage (PlantCultivationDiscoveryEventId, WorldEventMessage.PlantCultivationDiscoveryMessagePrefix);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}

	public override void TryGenerateEventMessage (long id, string messagePrefix) {

		if (World.HasEventMessage (id))
			return;

		World.AddEventMessage (new DiscoveryEventMessage (PlantCultivationDiscovery.PlantCultivationDiscoveryId, Group.Cell, id, messagePrefix + " at " + Group.Position));
	}
}
