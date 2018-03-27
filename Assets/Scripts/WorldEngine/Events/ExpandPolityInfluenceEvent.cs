using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ExpandPolityInfluenceEvent : CellGroupEvent {

	[XmlAttribute ("TGrpId")]
	public long TargetGroupId;
	[XmlAttribute ("PolId")]
	public long PolityId;

	[XmlIgnore]
	public CellGroup TargetGroup;
	[XmlIgnore]
	public Polity Polity;

	public ExpandPolityInfluenceEvent () {

		DoNotSerialize = true;
	}

	public ExpandPolityInfluenceEvent (CellGroup group, Polity polity, CellGroup targetGroup, long triggerDate) : base (group, triggerDate, ExpandPolityInfluenceEventId) {

		Polity = polity;

		PolityId = polity.Id;

		TargetGroup = targetGroup;

		TargetGroupId = TargetGroup.Id;

		DoNotSerialize = true;
	}

	public override bool IsStillValid () {

		if (!base.IsStillValid ())
			return false;

		if (Polity == null)
			return false;

		if (!Polity.StillPresent)
			return false;

		if (TargetGroup == null)
			return false;

		if (!TargetGroup.StillPresent)
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		PolityInfluence sourcePi = Group.GetPolityInfluence (Polity);

		if (sourcePi == null)
			return false;

		return true;
	}

	public override void Trigger () {

		//		#if DEBUG
		//		if (Manager.RegisterDebugEvent != null) {
		//			if (Group.Id == Manager.TracingData.GroupId) {
		//				string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;
		//
		//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
		//					"ExpandPolityInfluence:Trigger - Group:" + groupId,
		//					"CurrentDate: " + World.CurrentDate + 
		//					", TriggerDate: " + TriggerDate + 
		//					", SpawnDate: " + SpawnDate +
		//					", PolityId: " + PolityId + 
		//					", TargetGroup Id: " + TargetGroupId + 
		//					"");
		//
		//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
		//			}
		//		}
		//		#endif

		float randomFactor = Group.Cell.GetNextLocalRandomFloat (RngOffsets.EVENT_TRIGGER + (int)Id);
		float percentToExpand = Mathf.Pow (randomFactor, 4);

		float populationFactor = Group.Population / (float)(Group.Population + TargetGroup.Population);
		percentToExpand *= populationFactor;

		PolityInfluence sourcePi = Group.GetPolityInfluence (Polity);

		TargetGroup.Culture.MergeCulture (Group.Culture, percentToExpand);
		TargetGroup.MergePolityInfluence (sourcePi, percentToExpand);

		TryMigrateFactionCores ();

		World.AddGroupToUpdate (Group);
		World.AddGroupToUpdate (TargetGroup);
	}

	private void TryMigrateFactionCores () {

		List<Faction> factionCoresToMigrate = new List<Faction> ();

		foreach (Faction faction in Group.GetFactionCores ()) {

			if (faction.ShouldMigrateFactionCore (Group, TargetGroup)) {
				factionCoresToMigrate.Add (faction);
			}
		}

		foreach (Faction faction in factionCoresToMigrate) {

			faction.SetToUpdate ();

			faction.PrepareNewCoreGroup (TargetGroup);
		}
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		TargetGroup = World.GetGroup (TargetGroupId);
		Polity = World.GetPolity (PolityId);

		Group.PolityExpansionEvent = this;
	}

	protected override void DestroyInternal () {

		if (Group != null) {
			Group.HasPolityExpansionEvent = false;
		}

		base.DestroyInternal ();
	}

	public void Reset (Polity polity, CellGroup targetGroup, long triggerDate) {

		TargetGroup = targetGroup;
		TargetGroupId = TargetGroup.Id;

		Polity = polity;
		PolityId = Polity.Id;

		Reset (triggerDate);
	}
}
