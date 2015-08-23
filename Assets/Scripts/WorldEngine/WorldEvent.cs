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

public class UpdateGroupEvent : WorldEvent {
	
	[XmlAttribute]
	public int GroupId;

	[XmlIgnore]
	public CellGroup Group;

	public UpdateGroupEvent () {

	}

	public UpdateGroupEvent (World world, int triggerDate, CellGroup group) : base (world, triggerDate) {

		Group = group;
		GroupId = Group.Id;
	}

	public override bool CanTrigger () {

		if (Group == null)
			return false;

		return Group.StillPresent;
	}

	public override void FinalizeLoad () {

		Group = World.FindGroup (GroupId);
	}

	public override void Trigger () {

		World.AddGroupToUpdate (Group);
	}
}
