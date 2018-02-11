using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class WorldEventMessage {

	[XmlAttribute]
	public long Id;

	[XmlAttribute]
	public long Date;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public string Message {
		get { return GenerateMessage (); }
	}

	public WorldEventMessage () {
	
	}

	public WorldEventMessage (World world, long id, long date) {
	
		World = world;
		Id = id;
		Date = date;
	}

	protected abstract string GenerateMessage ();
}

public abstract class CellEventMessage : WorldEventMessage {

	public WorldPosition Position;

	public CellEventMessage () {
	
	}

	public CellEventMessage (TerrainCell cell, long id, long date) : base (cell.World, id, date) {

		Position = cell.Position;
	}
}

public class DiscoveryEventMessage : CellEventMessage {

	public const string SailingDiscoveryMessagePrefix = "Sailing discovered";
	public const string TribalismDiscoveryMessagePrefix = "Tribalism discovered";
	public const string BoatMakingDiscoveryMessagePrefix = "Boat making discovered";
	public const string PlantCultivationDiscoveryMessagePrefix = "Plant cultivation discovered";

	[XmlAttribute]
	public string DiscoveryId;

	public DiscoveryEventMessage () {

	}

	public DiscoveryEventMessage (string discoveryId, TerrainCell cell, long id, long date) : base (cell, id, date) {

		DiscoveryId = discoveryId;
	}

	protected override string GenerateMessage ()
	{
		string prefix = null;

		if (DiscoveryId == SailingDiscovery.SailingDiscoveryId) {
			prefix = SailingDiscoveryMessagePrefix;
		} else if (DiscoveryId == TribalismDiscovery.TribalismDiscoveryId) {
			prefix = TribalismDiscoveryMessagePrefix;
		} else if (DiscoveryId == BoatMakingDiscovery.BoatMakingDiscoveryId) {
			prefix = BoatMakingDiscoveryMessagePrefix;
		} else if (DiscoveryId == PlantCultivationDiscovery.PlantCultivationDiscoveryId) {
			prefix = PlantCultivationDiscoveryMessagePrefix;
		} 

		if (prefix == null) {
			Debug.LogError ("Unhandled DiscoveryId: " + DiscoveryId);
		}

		Territory territory = World.GetCell (Position).EncompassingTerritory;

		if (territory != null) {
			return prefix + " in " + territory.Polity.Name.Text + " at " + Position;
		}

		return prefix + " at " + Position;
	}
}

public class PolityFormationEventMessage : CellEventMessage {

	[XmlAttribute]
	public bool First = false;

	[XmlAttribute]
	public long PolityId;

	public PolityFormationEventMessage () {

	}

	public PolityFormationEventMessage (Polity polity, long date) : base (polity.CoreGroup.Cell, WorldEvent.PolityFormationEventId, date) {

		PolityId = polity.Id;
	}

	protected override string GenerateMessage ()
	{
		Polity polity = World.GetPolity (PolityId);

		if (First) {
			return "The first polity, " + polity.Name.Text + ", formed at " + Position;
		} else {
			return "A new polity, " + polity.Name.Text + ", formed at " + Position;
		}
	}
}

public class WorldEventSnapshot {

	public System.Type EventType;

	public long TriggerDate;
	public long SpawnDate;
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
	public const long PreventClanSplitEventId = 12;
	public const long ExpandPolityInfluenceEventId = 8;
	public const long TribeSplitEventId = 9;
	public const long PolityFormationEventId = 10;
	public const long ClanCoreMigrationEventId = 11;
	public const long FactionUpdateEventId = 11;

//	public static int EventCount = 0;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public bool DoNotSerialize = false;

	[XmlIgnore]
	public bool FailedToTrigger = false;

	[XmlIgnore]
	public BinaryTreeNode<long, WorldEvent> Node = null;
	
	[XmlAttribute]
	public long TriggerDate;

	[XmlAttribute]
	public long SpawnDate;
	
	[XmlAttribute]
	public long Id;

	public WorldEvent () {

//		EventCount++;

		Manager.UpdateWorldLoadTrackEventCount ();
	}

	public WorldEvent (World world, long triggerDate, long id) {
		
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

	public void AssociateNode (BinaryTreeNode<long, WorldEvent> node) {
	
		if (Node != null) {
			Node.Valid = false;
		}

		Node = node;
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

	public void Destroy () {
		
//		EventCount--;

		DestroyInternal ();
	}

	protected virtual void DestroyInternal () {
	
	}

	public virtual void Reset (long newTriggerDate, long newId) {

//		EventCount++;

		TriggerDate = newTriggerDate;
		Id = newId;

		FailedToTrigger = false;
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

	public CellEvent (TerrainCell cell, long triggerDate, long eventTypeId) : base (cell.World, triggerDate, cell.GenerateUniqueIdentifier (triggerDate, 100L, eventTypeId)) {

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
}

public abstract class DiscoveryEvent : CellGroupEvent {

	public DiscoveryEvent () {

	}

	public DiscoveryEvent (CellGroup group, long triggerDate, long eventTypeId) : base (group, triggerDate, eventTypeId) {
	
	}

	public void TryGenerateEventMessage (long discoveryEventId, string discoveryId) {

		DiscoveryEventMessage eventMessage = null;

		if (!World.HasEventMessage (discoveryEventId)) {
			eventMessage = new DiscoveryEventMessage (discoveryId, Group.Cell, discoveryEventId, TriggerDate);

			World.AddEventMessage (eventMessage);
		}

		if (Group.Cell.EncompassingTerritory != null) {

			Polity encompassingPolity = Group.Cell.EncompassingTerritory.Polity;

			if (!encompassingPolity.HasEventMessage (discoveryEventId)) {
				if (eventMessage == null)
					eventMessage = new DiscoveryEventMessage (discoveryId, Group.Cell, discoveryEventId, TriggerDate);

				encompassingPolity.AddEventMessage (eventMessage);
			}
		}
	}
}

public class SailingDiscoveryEvent : DiscoveryEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 10000;

	public const int MinShipBuildingKnowledgeSpawnEventValue = ShipbuildingKnowledge.MinKnowledgeValueForSailingSpawnEvent;
	public const int MinShipBuildingKnowledgeValue = ShipbuildingKnowledge.MinKnowledgeValueForSailing;
	public const int OptimalShipBuildingKnowledgeValue = ShipbuildingKnowledge.OptimalKnowledgeValueForSailing;

	public const string EventSetFlag = "SailingDiscoveryEvent_Set";

	public SailingDiscoveryEvent () {
		
	}
	
	public SailingDiscoveryEvent (CellGroup group, long triggerDate) : base (group, triggerDate, SailingDiscoveryEventId) {
		
		Group.SetFlag (EventSetFlag);
	}
	
	public static long CalculateTriggerDate (CellGroup group) {
		
		float shipBuildingValue = 0;
		
		CulturalKnowledge shipbuildingKnowledge = group.Culture.GetKnowledge (ShipbuildingKnowledge.ShipbuildingKnowledgeId);
		
		if (shipbuildingKnowledge != null)
			shipBuildingValue = shipbuildingKnowledge.Value;

		float randomFactor = group.Cell.GetNextLocalRandomFloat (RngOffsets.SAILING_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = randomFactor * randomFactor;

		float shipBuildingFactor = (shipBuildingValue - MinShipBuildingKnowledgeValue) / (float)(OptimalShipBuildingKnowledgeValue - MinShipBuildingKnowledgeValue);
		shipBuildingFactor = Mathf.Clamp01 (shipBuildingFactor) + 0.001f;

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / shipBuildingFactor;

		long targetDate = (long)(group.World.CurrentDate + dateSpan) + 1;

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

		TryGenerateEventMessage (SailingDiscoveryEventId, SailingDiscovery.SailingDiscoveryId);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}
}

public class TribalismDiscoveryEvent : DiscoveryEvent {

	public const long EventMessageId = 0;
	public const string EventMessagePrefix = "Tribalism Discovered";

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 100;

	public const int MinSocialOrganizationKnowledgeForTribalismDiscovery = SocialOrganizationKnowledge.MinValueForTribalismDiscovery;
	public const int MinSocialOrganizationKnowledgeForHoldingTribalism = SocialOrganizationKnowledge.MinValueForHoldingTribalism;
	public const int OptimalSocialOrganizationKnowledgeValue = SocialOrganizationKnowledge.OptimalValueForTribalism;

	public const string EventSetFlag = "TribalismDiscoveryEvent_Set";

	public TribalismDiscoveryEvent () {

	}

	public TribalismDiscoveryEvent (CellGroup group, long triggerDate) : base (group, triggerDate, TribalismDiscoveryEventId) {

		Group.SetFlag (EventSetFlag);
	}

	public static long CalculateTriggerDate (CellGroup group) {

		float socialOrganizationValue = 0;

		CulturalKnowledge socialOrganizationKnowledge = group.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge != null)
			socialOrganizationValue = socialOrganizationKnowledge.Value;

		float randomFactor = group.Cell.GetNextLocalRandomFloat (RngOffsets.TRIBALISM_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float socialOrganizationFactor = (socialOrganizationValue - MinSocialOrganizationKnowledgeForHoldingTribalism) / (float)(OptimalSocialOrganizationKnowledgeValue - MinSocialOrganizationKnowledgeForHoldingTribalism);
		socialOrganizationFactor = Mathf.Pow (socialOrganizationFactor, 2);
		socialOrganizationFactor = Mathf.Clamp (socialOrganizationFactor, 0.001f, 1);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / socialOrganizationFactor;

		long targetDate = (long)(group.World.CurrentDate + dateSpan) + 1;

		return targetDate;
	}

	public static bool CanSpawnIn (CellGroup group) {

		if (group.Population < Tribe.MinPopulationForTribeCore)
			return false;

		if (group.IsFlagSet (EventSetFlag))
			return false;

		if (group.Culture.GetFoundDiscoveryOrToFind (TribalismDiscovery.TribalismDiscoveryId) != null)
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		if (Group.Population < Tribe.MinPopulationForTribeCore)
			return false;

		CulturalDiscovery discovery = Group.Culture.GetFoundDiscoveryOrToFind (TribalismDiscovery.TribalismDiscoveryId);

		if (discovery != null)
			return false;

		CulturalKnowledge socialOrganizationKnowledge = Group.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge == null)
			return false;

		if (socialOrganizationKnowledge.Value < MinSocialOrganizationKnowledgeForTribalismDiscovery)
			return false;

		return true;
	}

	public override void Trigger () {

		Group.Culture.AddDiscoveryToFind (new TribalismDiscovery ());

		Tribe newTribe = null;

		if (Group.GetPolityInfluencesCount () <= 0) {

			newTribe = new Tribe (Group);
			newTribe.Initialize ();

			World.AddPolity (newTribe);
			World.AddPolityToUpdate (newTribe);
		}

		World.AddGroupToUpdate (Group);

		TryGenerateEventMessages (newTribe);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}

	public void TryGenerateEventMessages (Tribe newTribe) {

		TryGenerateEventMessage (TribalismDiscoveryEventId, TribalismDiscovery.TribalismDiscoveryId);

		if (newTribe != null) {
			PolityFormationEventMessage formationEventMessage = null;

			if (!World.HasEventMessage (WorldEvent.PolityFormationEventId)) {
				formationEventMessage = new PolityFormationEventMessage (newTribe, TriggerDate);

				World.AddEventMessage (formationEventMessage);
				formationEventMessage.First = true;
			}

			if (Group.Cell.EncompassingTerritory != null) {
				Polity encompassingPolity = Group.Cell.EncompassingTerritory.Polity;

				if (formationEventMessage == null) {
					formationEventMessage = new PolityFormationEventMessage (newTribe, TriggerDate);
				}

				encompassingPolity.AddEventMessage (formationEventMessage);
			}
		}
	}
}

public class BoatMakingDiscoveryEvent : DiscoveryEvent {
	
	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 10000;

	public const string EventSetFlag = "BoatMakingDiscoveryEvent_Set";
	
	public BoatMakingDiscoveryEvent () {
		
	}
	
	public BoatMakingDiscoveryEvent (CellGroup group, long triggerDate) : base (group, triggerDate, BoatMakingDiscoveryEventId) {

		Group.SetFlag (EventSetFlag);
	}
	
	public static long CalculateTriggerDate (CellGroup group) {
		
		float oceanPresence = ShipbuildingKnowledge.CalculateNeighborhoodOceanPresenceIn (group);

		float randomFactor = group.Cell.GetNextLocalRandomFloat (RngOffsets.BOAT_MAKING_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = randomFactor * randomFactor;

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant;

		if (oceanPresence > 0) {

			dateSpan /= oceanPresence;
		} else {

			throw new System.Exception ("Can't calculate valid trigger date");
		}

		long targetDate = (long)(group.World.CurrentDate + dateSpan) + 1;

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

		TryGenerateEventMessage (BoatMakingDiscoveryEventId, BoatMakingDiscovery.BoatMakingDiscoveryId);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}
}

public class PlantCultivationDiscoveryEvent : DiscoveryEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 600000;

	public const string EventSetFlag = "PlantCultivationDiscoveryEvent_Set";

	public PlantCultivationDiscoveryEvent () {

	}

	public PlantCultivationDiscoveryEvent (CellGroup group, long triggerDate) : base (group, triggerDate, PlantCultivationDiscoveryEventId) {

		Group.SetFlag (EventSetFlag);
	}

	public static long CalculateTriggerDate (CellGroup group) {

		float terrainFactor = AgricultureKnowledge.CalculateTerrainFactorIn (group.Cell);

		float randomFactor = group.Cell.GetNextLocalRandomFloat (RngOffsets.PLANT_CULTIVATION_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = randomFactor * randomFactor;

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant;

		if (terrainFactor > 0) {

			dateSpan /= terrainFactor;
		} else {

			throw new System.Exception ("Can't calculate valid trigger date");
		}

		long targetDate = (long)(group.World.CurrentDate + dateSpan) + 1;

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

		TryGenerateEventMessage (PlantCultivationDiscoveryEventId, PlantCultivationDiscovery.PlantCultivationDiscoveryId);
	}

	protected override void DestroyInternal ()
	{
		if (Group != null) {
			Group.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}
}
