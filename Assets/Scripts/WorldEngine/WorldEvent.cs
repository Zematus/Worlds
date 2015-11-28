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

	public WorldEvent () {

		EventCount++;

		Manager.UpdateWorldLoadTrackEventCount ();
	}

	public WorldEvent (World world, int triggerDate) {
		
		EventCount++;

		World = world;
		TriggerDate = triggerDate;
	}

	public virtual bool CanTrigger () {

		EventCount--;

		return true;
	}
	
	public virtual void FinalizeLoad () {

	}

	public abstract void Trigger ();
}

public class UpdateCellGroupEvent : WorldEvent {
	
	[XmlAttribute]
	public int GroupId;

	[XmlIgnore]
	public CellGroup Group;

	public UpdateCellGroupEvent () {

	}

	public UpdateCellGroupEvent (World world, int triggerDate, CellGroup group) : base (world, triggerDate) {

		Group = group;
		GroupId = Group.Id;
	}

	public override bool CanTrigger () {
		
		if (!base.CanTrigger ())
			return false;

		if (Group == null)
			return false;

		if (Group.NextUpdateDate != TriggerDate)
			return false;

		return Group.StillPresent;
	}

	public override void FinalizeLoad () {

		Group = World.FindCellGroup (GroupId);
		Group.DebugTagged = true;
	}

	public override void Trigger () {

		World.AddGroupToUpdate (Group);
	}
}

public class MigrateGroupEvent : WorldEvent {
	
	public static int MigrationEventCount = 0;
	
	public static float TotalTravelTime = 0;
	
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
		
		TotalTravelTime += TravelTime;
		
		if (TotalTravelTime <= 0) {
			
			throw new System.Exception ("Total travel time can't be equal or less than 0");
		}
	}
	
	public override bool CanTrigger () {
		
		MigrationEventCount--;
		
		TotalTravelTime -= TravelTime;
		
		TotalTravelTime = Mathf.Max (0, TotalTravelTime);
		
		Group.SourceGroup.HasMigrationEvent = false;
		
		if (!base.CanTrigger ())
			return false;
		
		return true;
	}
	
	public override void Trigger () {

		World.AddMigratingGroup (Group);
	}

	public override void FinalizeLoad () {

		Group.World = World;

		Group.FinalizeLoad ();
	}
}

public class ShipbuildingDiscoveryEvent : WorldEvent {
	
	public const int MaxDateSpanToTrigger = CellGroup.GenerationTime * 10000;
	
	[XmlAttribute]
	public int GroupId;
	
	[XmlIgnore]
	public CellGroup Group;
	
	public ShipbuildingDiscoveryEvent () {
		
	}
	
	public ShipbuildingDiscoveryEvent (World world, int triggerDate, CellGroup group) : base (world, triggerDate) {
		
		Group = group;
		GroupId = Group.Id;
	}
	
	public static int CalculateTriggerDate (CellGroup group) {
		
		float oceanPresence = ShipbuildingKnowledge.CalculateNeighborhoodOceanPresenceIn (group);
		
		float randomFactor = group.Cell.GetNextLocalRandomFloat ();
		randomFactor = randomFactor * randomFactor;
		
		float mixFactor = (1 - oceanPresence) * (1 - randomFactor);
		
		int dateSpan = (int)Mathf.Ceil(Mathf.Max (1, MaxDateSpanToTrigger * mixFactor));
		
		return group.World.CurrentDate + dateSpan;
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
		
		if (Group == null)
			return false;
		
		if (!Group.StillPresent)
			return false;
		
		if (Group.Culture.GetKnowledge (ShipbuildingKnowledge.ShipbuildingKnowledgeId) != null)
			return false;
		
		return true;
	}
	
	public override void FinalizeLoad () {
		
		Group = World.FindCellGroup (GroupId);
	}
	
	public override void Trigger () {
		
		Group.Culture.AddKnowledgeToLearn (new ShipbuildingKnowledge (Group));
		World.AddGroupToUpdate (Group);
	}
}

public class KnowledgeTransferEvent : WorldEvent {
	
	public static int KnowledgeTransferEventCount = 0;

	public const int MaxDateSpanToTrigger = CellGroup.GenerationTime * 200;
	
	[XmlAttribute]
	public int SourceGroupId;
	[XmlAttribute]
	public int TargetGroupId;
	
	[XmlIgnore]
	public CellGroup SourceGroup;
	[XmlIgnore]
	public CellGroup TargetGroup;
	
	public KnowledgeTransferEvent () {
		
		KnowledgeTransferEventCount++;
	}
	
	public KnowledgeTransferEvent (World world, int triggerDate, CellGroup sourceGroup, CellGroup targetGroup) : base (world, triggerDate) {
		
		KnowledgeTransferEventCount++;
		
		SourceGroup = sourceGroup;
		SourceGroupId = SourceGroup.Id;
		
		TargetGroup = targetGroup;
		TargetGroupId = TargetGroup.Id;
	}
	
	public static CellGroup DiscoverTargetGroup (CellGroup sourceGroup, out float targetTransferValue) {

		TerrainCell sourceCell = sourceGroup.Cell;

		Dictionary<CellGroup, float> groupValuePairs = new Dictionary<CellGroup, float> ();

		sourceGroup.Neighbors.ForEach (g => {
			
			float transferValue = CellGroup.CalculateKnowledgeTransferValue (sourceGroup, g);

			groupValuePairs.Add (g, transferValue);
		});

		CellGroup targetGroup = CollectionUtility.WeightedSelection (groupValuePairs, sourceCell.GetNextLocalRandomFloat);

		targetTransferValue = 0;

		if (targetGroup == null)
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
		
		KnowledgeTransferEventCount--;
		
		SourceGroup.HasKnowledgeTransferEvent = false;
		
		if (!base.CanTrigger ())
			return false;
		
		return (0 < CellGroup.CalculateKnowledgeTransferValue(SourceGroup, TargetGroup));
	}
	
	public override void FinalizeLoad () {
		
		SourceGroup = World.FindCellGroup (SourceGroupId);
		TargetGroup = World.FindCellGroup (TargetGroupId);
	}
	
	public override void Trigger () {

		World.AddGroupActionToPerform (new KnowledgeTransferAction (SourceGroup, TargetGroup));
		World.AddGroupToUpdate (TargetGroup);
	}
}
