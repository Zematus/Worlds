using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class WorldEvent {

	[XmlIgnore]
	public World World;
	
	[XmlAttribute]
	public int TriggerDate;

	public WorldEvent () {

		Manager.UpdateWorldLoadTrackEventCount ();
	}

	public WorldEvent (World world, int triggerDate) {

		World = world;
		TriggerDate = triggerDate;
	}

	public virtual bool CanTrigger () {

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
	
	public static int EventCount = 0;
	
	public static float MeanTravelTime = 0;
	
	[XmlAttribute]
	public int TravelTime;

	public MigratingGroup Group;
	
	public MigrateGroupEvent () {

		EventCount++;
	}
	
	public MigrateGroupEvent (World world, int triggerDate, int travelTime, MigratingGroup group) : base (world, triggerDate) {

		TravelTime = travelTime;
		
		Group = group;
		
		float TravelTimeSum = (MeanTravelTime * EventCount) + travelTime;

		EventCount++;

		MeanTravelTime = TravelTimeSum / (float)EventCount;
	}
	
	public override bool CanTrigger () {
		
		Group.SourceGroup.HasMigrationEvent = false;
		
		float TravelTimeSub = (MeanTravelTime * EventCount) - TravelTime;
		
		EventCount--;

		if (EventCount > 0) {
		
			MeanTravelTime = TravelTimeSub / (float)EventCount;

		} else {

			MeanTravelTime = 0;
		}
		
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
	
	public const float MinKnowledgeValue = 1f;
	public const float MinTransferValue = 0.1f;
	public const int MaxDateSpanToTrigger = CellGroup.GenerationTime * 10;
	
	[XmlAttribute]
	public int SourceGroupId;
	[XmlAttribute]
	public int TargetGroupId;
	
	[XmlIgnore]
	public CellGroup SourceGroup;
	[XmlIgnore]
	public CellGroup TargetGroup;
	
	public KnowledgeTransferEvent () {
		
	}
	
	public KnowledgeTransferEvent (World world, int triggerDate, CellGroup sourceGroup, CellGroup targetGroup) : base (world, triggerDate) {
		
		SourceGroup = sourceGroup;
		SourceGroupId = SourceGroup.Id;
		
		TargetGroup = targetGroup;
		TargetGroupId = TargetGroup.Id;
	}

	public static float CalculateTransferValue (CellGroup sourceGroup, CellGroup targetGroup) {

		float transferValue = float.MinValue;

		if (!sourceGroup.StillPresent)
			return transferValue;

		if (!targetGroup.StillPresent)
			return transferValue;

		transferValue = 0;
		
		foreach (CulturalKnowledge sourceKnowledge in sourceGroup.Culture.Knowledges) {
			
			if (sourceKnowledge.Value <= MinKnowledgeValue) continue;
			
			CulturalKnowledge targetKnowledge = targetGroup.Culture.GetKnowledge (sourceKnowledge.Id);
			
			if (targetKnowledge == null) {
				transferValue += 1;
			} else {
				transferValue += Mathf.Max (0, 1 - (2 * targetKnowledge.Value / sourceKnowledge.Value));
			}
		}

		return transferValue;
	}
	
	public static CellGroup DiscoverTargetGroup (CellGroup sourceGroup, out float targetTransferValue) {

		CellGroup targetGroup = null;

		targetTransferValue = MinTransferValue;

		foreach (TerrainCell neighborCell in sourceGroup.Cell.GetNeighborCells ()) {
		
			CellGroup neighborGroup = neighborCell.Group;

			if (neighborGroup == null) continue;

			float neighborTransferValue = CalculateTransferValue (sourceGroup, neighborGroup);
			
			if (neighborTransferValue > targetTransferValue) {
				targetTransferValue = neighborTransferValue;
				targetGroup = neighborGroup;
			}
		}
		
		return targetGroup;
	}
	
	public static int CalculateTriggerDate (CellGroup group, float targetTransferValue) {
		
		float randomFactor = group.Cell.GetNextLocalRandomFloat ();
		
		float mixFactor = (1 - targetTransferValue * randomFactor);
		
		int dateSpan = (int)Mathf.Ceil(Mathf.Max (1, MaxDateSpanToTrigger * mixFactor));
		
		return group.World.CurrentDate + dateSpan;
	}
	
	public override bool CanTrigger () {

		SourceGroup.HasKnowledgeTransferEvent = false;
		
		return (0 < CalculateTransferValue(SourceGroup, TargetGroup));
	}
	
	public override void FinalizeLoad () {
		
		SourceGroup = World.FindCellGroup (SourceGroupId);
		TargetGroup = World.FindCellGroup (TargetGroupId);
	}
	
	public override void Trigger () {

		World.AddGroupActionToPerform (new KnowledgeTransferAction (SourceGroup, TargetGroup));
		World.AddGroupToUpdate (SourceGroup);
		World.AddGroupToUpdate (TargetGroup);
	}
}
