using UnityEngine;
using System.Collections;
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
		
		float TravelTimeSub = (MeanTravelTime * EventCount) - TravelTime;
		
		EventCount--;

		if (EventCount > 0) {
		
			MeanTravelTime = TravelTimeSub / (float)EventCount;

		} else {

			MeanTravelTime = 0;
		}

		if (Group == null)
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
