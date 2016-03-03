using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class WorldEvent {

	public static int EventCount = 0;

	[XmlIgnore]
	public World World;
	
	[XmlAttribute]
	public int TriggerDate;
	
	[XmlAttribute]
	public int Id;

	public WorldEvent () {

		EventCount++;

		Manager.UpdateWorldLoadTrackEventCount ();
	}

	public WorldEvent (World world, int triggerDate) {
		
		EventCount++;

		World = world;
		TriggerDate = triggerDate;

		Id = World.GenerateEventId ();
	}

	public virtual bool CanTrigger () {

		return true;
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

public abstract class CellGroupEvent : WorldEvent {
	
	[XmlAttribute]
	public int GroupId;
	
	[XmlIgnore]
	public CellGroup Group;
	
	public CellGroupEvent () {
		
	}
	
	public CellGroupEvent (CellGroup group, int triggerDate) : base (group.World, triggerDate) {
		
		Group = group;
		GroupId = Group.Id;
		
//		Group.AddAssociatedEvent (this);
	}

	public override bool CanTrigger () {

		if (Group == null)
			return false;
		
		return Group.StillPresent;
	}
	
	public override void FinalizeLoad () {
		
		Group = World.FindCellGroup (GroupId);
		
//		Group.AddAssociatedEvent (this);
	}
	
	protected override void DestroyInternal ()
	{
		if (Group == null)
			return;
		
//		Group.RemoveAssociatedEvent (Id);
	}
}

public class UpdateCellGroupEvent : CellGroupEvent {

	public UpdateCellGroupEvent () {

	}

	public UpdateCellGroupEvent (CellGroup group, int triggerDate) : base (group, triggerDate) {

	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		if (Group.NextUpdateDate != TriggerDate)
			return false;

		return true;
	}

	public override void Trigger () {

		World.AddGroupToUpdate (Group);
	}
}

public class MigrateGroupEvent : WorldEvent {
	
	public static int MigrationEventCount = 0;
	
//	public static float TotalTravelTime = 0;
	
	[XmlAttribute]
	public int TravelTime;

	public MigratingGroup Group;
	
	public MigrateGroupEvent () {
		
		MigrationEventCount++;
		
//		TotalTravelTime += TravelTime;
//
//		if (TotalTravelTime <= 0) {
//		
//			throw new System.Exception ("Total travel time can't be equal or less than 0");
//		}
	}
	
	public MigrateGroupEvent (World world, int triggerDate, int travelTime, MigratingGroup group) : base (world, triggerDate) {

		TravelTime = travelTime;
		
		Group = group;
		
		MigrationEventCount++;
		
//		TotalTravelTime += TravelTime;
//		
//		if (TotalTravelTime <= 0) {
//			
//			throw new System.Exception ("Total travel time can't be equal or less than 0");
//		}
	}
	
	public override bool CanTrigger () {
		
		return true;
	}
	
	public override void Trigger () {

		World.AddMigratingGroup (Group);
	}

	public override void FinalizeLoad () {

		Group.World = World;

		Group.FinalizeLoad ();
	}

	protected override void DestroyInternal ()
	{
		MigrationEventCount--;
		
//		TotalTravelTime -= TravelTime;
//		
//		TotalTravelTime = Mathf.Max (0, TotalTravelTime);

		if (Group.SourceGroup != null) {
			
			Group.SourceGroup.HasMigrationEvent = false;
		}
	}
}

public class SailingDiscoveryEvent : CellGroupEvent {
	
	public const int MaxDateSpanToTrigger = CellGroup.GenerationTime * 10000;
	public const float MinShipBuildingKnowledgeSpawnEventValue = 5;
	public const float MinShipBuildingKnowledgeValue = SailingDiscovery.MinShipBuildingKnowledgeValue;
	public const float OptimalShipBuildingKnowledgeValue = SailingDiscovery.OptimalShipBuildingKnowledgeValue;

	public const string EventSetFlag = "SailingDiscoveryEvent_Set";
	
	public SailingDiscoveryEvent () {
		
	}
	
	public SailingDiscoveryEvent (CellGroup group, int triggerDate) : base (group, triggerDate) {
		
		Group.SetFlag (EventSetFlag);
	}
	
	public static int CalculateTriggerDate (CellGroup group) {
		
		float shipBuildingValue = 0;
		
		CulturalKnowledge shipbuildingKnowledge = group.Culture.GetKnowledge (ShipbuildingKnowledge.ShipbuildingKnowledgeId);
		
		if (shipbuildingKnowledge != null)
			shipBuildingValue = shipbuildingKnowledge.Value;

		float randomFactor = group.Cell.GetNextLocalRandomFloat ();
		randomFactor = randomFactor * randomFactor;

		float shipBuildingFactor = (shipBuildingValue - MinShipBuildingKnowledgeValue) / (OptimalShipBuildingKnowledgeValue - MinShipBuildingKnowledgeValue);
		shipBuildingFactor = Mathf.Clamp01 (shipBuildingFactor);
		
		float mixFactor = (1 - shipBuildingFactor) * (1 - randomFactor);
		
		int dateSpan = (int)Mathf.Ceil(Mathf.Max (1, MaxDateSpanToTrigger * mixFactor));
		
		return group.World.CurrentDate + dateSpan;
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
		Group.UnsetFlag (EventSetFlag);

		base.DestroyInternal ();
	}
}

public class BoatMakingDiscoveryEvent : CellGroupEvent {
	
	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 10000;
	
	public BoatMakingDiscoveryEvent () {
		
	}
	
	public BoatMakingDiscoveryEvent (CellGroup group, int triggerDate) : base (group, triggerDate) {

	}
	
	public static int CalculateTriggerDate (CellGroup group) {
		
		float oceanPresence = ShipbuildingKnowledge.CalculateNeighborhoodOceanPresenceIn (group);

		float randomFactor = group.Cell.GetNextLocalRandomFloat ();
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

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 50000;

	public PlantCultivationDiscoveryEvent () {

	}

	public PlantCultivationDiscoveryEvent (CellGroup group, int triggerDate) : base (group, triggerDate) {

	}

	public static int CalculateTriggerDate (CellGroup group) {

		float terrainFactor = group.Cell.Arability * group.Cell.Accessibility * group.Cell.Accessibility;

		float randomFactor = group.Cell.GetNextLocalRandomFloat ();
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

		float arability = group.Cell.Arability;

		return (arability > 0);
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		if (Group.Culture.GetKnowledge (AgricultureKnowledge.AgricultureKnowledgeId) != null)
			return false;

		return true;
	}

	public override void Trigger () {

		Group.Culture.AddDiscoveryToFind (new PlantCultivationDiscovery ());
		Group.Culture.AddKnowledgeToLearn (new AgricultureKnowledge (Group));
		World.AddGroupToUpdate (Group);
	}
}

public class KnowledgeTransferEvent : CellGroupEvent {
	
	public static int KnowledgeTransferEventCount = 0;

	public const int MaxDateSpanToTrigger = CellGroup.GenerationTime * 50;

	[XmlAttribute]
	public int TargetGroupId;

	[XmlIgnore]
	public CellGroup TargetGroup;
	
	public KnowledgeTransferEvent () {
		
		KnowledgeTransferEventCount++;
	}
	
	public KnowledgeTransferEvent (CellGroup sourceGroup, CellGroup targetGroup, int triggerDate) : base (sourceGroup, triggerDate) {
		
		KnowledgeTransferEventCount++;

		sourceGroup.HasKnowledgeTransferEvent = true;
		
		TargetGroup = targetGroup;
		TargetGroupId = TargetGroup.Id;

//		TargetGroup.AddAssociatedEvent (this);
	}
	
	public static CellGroup DiscoverTargetGroup (CellGroup sourceGroup, out float targetTransferValue) {

		TerrainCell sourceCell = sourceGroup.Cell;

		Dictionary<CellGroup, float> groupValuePairs = new Dictionary<CellGroup, float> ();

		float totalTransferValue = 0;
		sourceGroup.Neighbors.ForEach (g => {
			
			float transferValue = CellCulture.CalculateKnowledgeTransferValue (sourceGroup, g);
			totalTransferValue += transferValue;

			groupValuePairs.Add (g, transferValue);
		});

		CellGroup targetGroup = CollectionUtility.WeightedSelection (groupValuePairs, totalTransferValue, sourceCell.GetNextLocalRandomFloat);

		targetTransferValue = 0;

		if (targetGroup == null)
			return null;
		
		if (!targetGroup.StillPresent)
			return null;

		if (!groupValuePairs.TryGetValue (targetGroup, out targetTransferValue))
			return null;

		if (targetTransferValue < CellGroup.MinKnowledgeTransferValue)
			return null;
		
		return targetGroup;
	}
	
	public static int CalculateTriggerDate (CellGroup group, float targetTransferValue) {
		
		float randomFactor = group.Cell.GetNextLocalRandomFloat ();
		
		float mixFactor = (1 - targetTransferValue * randomFactor);
		
		int dateSpan = (int)Mathf.Ceil(Mathf.Max (1, MaxDateSpanToTrigger * mixFactor));
		
		return group.World.CurrentDate + dateSpan;
	}
	
	public override bool CanTrigger () {
		
		return (0 < CellCulture.CalculateKnowledgeTransferValue(Group, TargetGroup));
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		TargetGroup = World.FindCellGroup (TargetGroupId);

//		TargetGroup.AddAssociatedEvent (this);
	}
	
	public override void Trigger () {

		World.AddGroupActionToPerform (new KnowledgeTransferAction (Group, TargetGroup));
		World.AddGroupToUpdate (TargetGroup);
	}

	protected override void DestroyInternal ()
	{
		KnowledgeTransferEventCount--;
		
		if (Group != null) {

//			Group.RemoveAssociatedEvent (Id);
			Group.HasKnowledgeTransferEvent = false;
		}

		if (TargetGroup != null) {

//			TargetGroup.RemoveAssociatedEvent (Id);
		}
	}
}
