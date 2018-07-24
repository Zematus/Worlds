using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class WorldEvent : ISynchronizable {

	public const long UpdateCellGroupEventId = 0;
	public const long MigrateGroupEventId = 1;
	public const long SailingDiscoveryEventId = 2;
	public const long TribalismDiscoveryEventId = 3;
	public const long TribeFormationEventId = 4;
	public const long BoatMakingDiscoveryEventId = 5;
	public const long PlantCultivationDiscoveryEventId = 6;

	public const long ClanSplitDecisionEventId = 7;
	public const long PreventClanSplitEventId = 8;

	public const long ExpandPolityProminenceEventId = 9;

	public const long TribeSplitDecisionEventId = 10;
	public const long SplitClanPreventTribeSplitEventId = 11;
	public const long PreventTribeSplitEventId = 12;

	public const long PolityFormationEventId = 13;
	public const long ClanCoreMigrationEventId = 14;

	public const long ClanDemandsInfluenceDecisionEventId = 15;
	public const long ClanAvoidsInfluenceDemandDecisionEventId = 16;
	public const long RejectInfluenceDemandDecisionEventId = 17;
	public const long AcceptInfluenceDemandDecisionEventId = 18;

	public const long FosterTribeRelationDecisionEventId = 20;
	public const long AvoidFosterTribeRelationDecisionEventId = 21;
	public const long RejectFosterTribeRelationDecisionEventId = 22;
	public const long AcceptFosterTribeRelationDecisionEventId = 23;

	public const long MergeTribesDecisionEventId = 25;
	public const long AvoidMergeTribesAttemptDecisionEventId = 26;
	public const long RejectMergeTribesOfferDecisionEventId = 27;
	public const long AcceptMergeTribesOfferDecisionEventId = 28;

	public const long OpenTribeDecisionEventId = 30;
	public const long AvoidOpenTribeDecisionEventId = 31;

//	public static int EventCount = 0;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public bool DoNotSerialize = false;

	[XmlIgnore]
	public bool FailedToTrigger = false;

	[XmlIgnore]
	public BinaryTreeNode<long, WorldEvent> Node = null;

	[XmlAttribute("TId")]
	public long TypeId;

	[XmlAttribute("TDate")]
	public long TriggerDate;

	[XmlAttribute("SDate")]
	public long SpawnDate;
	
	[XmlAttribute]
	public long Id;

	public WorldEvent () {

//		EventCount++;

		Manager.UpdateWorldLoadTrackEventCount ();
    }

    public WorldEvent(World world, WorldEventData data, long id)
    {
        //		EventCount++;

        TypeId = data.TypeId;

        World = world;
        TriggerDate = data.TriggerDate;
        SpawnDate = data.SpawnDate;

        Id = id;
    }

    public WorldEvent(World world, long triggerDate, long id, long typeId)
    {
        //		EventCount++;

        TypeId = typeId;

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

	public WorldEvent (World world, long id, WorldEventData data) : this (world, data.TriggerDate, id, data.TypeId) {

	}

	public void AssociateNode (BinaryTreeNode<long, WorldEvent> node) {
	
		if (Node != null) {
			Node.Valid = false;
		}

		Node = node;
	}

	public virtual WorldEventData GetData () {
	
		return new WorldEventData (this);
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
        SpawnDate = World.CurrentDate;
        Id = newId;

		FailedToTrigger = false;
	}
}
