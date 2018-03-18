using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

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
	public const long PreventClanSplitEventId = 8;

	public const long ExpandPolityInfluenceEventId = 20;

	public const long TribeSplitEventId = 21;
	public const long SplitingClanPreventTribeSplitEventId = 25;

	public const long PolityFormationEventId = 22;
	public const long ClanCoreMigrationEventId = 23;
	public const long FactionUpdateEventId = 24;

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
