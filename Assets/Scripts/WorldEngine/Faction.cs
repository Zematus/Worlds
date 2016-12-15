using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class Faction : ISynchronizable {

	[XmlAttribute("Type")]
	public string Type;

	[XmlAttribute]
	public long Id;

	[XmlAttribute("GrpId")]
	public long GroupId;

	[XmlAttribute("PolId")]
	public long PolityId;

	[XmlAttribute("Prom")]
	public float Prominence;

	[XmlAttribute("StilPres")]
	public bool StillPresent = true;

	public Name Name = null;

	public List<string> Flags;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public CellGroup Group;

	[XmlIgnore]
	public Polity Polity;

	private HashSet<string> _flags = new HashSet<string> ();

	public Faction () {

	}

	public Faction (string type, CellGroup group, Polity polity, float prominence) {

		Type = type;

		World = group.World;

		Group = group;
		GroupId = group.Id;

		Id = group.GenerateUniqueIdentifier ();

		PolityId = polity.Id;
		Polity = polity;

		Prominence = prominence;

		GenerateName ();
	}

	public void Destroy () {
		
		Polity.RemoveFaction (this);

		StillPresent = false;
	}

	public void SetGroup (CellGroup group) {

		Group = group;
		GroupId = group.Id;
	}

	public abstract void GenerateName ();

	public void Update () {

		if (!Group.StillPresent) {
		
			Polity.RemoveFaction (this);

			return;
		}

		UpdateInternal ();
	}

	public abstract void UpdateInternal ();

	public virtual void Synchronize () {

		Flags = new List<string> (_flags);

		Name.Synchronize ();
	}

	public virtual void FinalizeLoad () {

		Name.World = World;
		Name.FinalizeLoad ();

		Group = World.GetGroup (GroupId);
		Polity = World.GetPolity (PolityId);

		if (Group == null) {
			throw new System.Exception ("Missing Group with Id " + GroupId);
		}

		if (Polity == null) {
			throw new System.Exception ("Missing Polity with Id " + PolityId);
		}

		Flags.ForEach (f => _flags.Add (f));
	}

	public long GenerateUniqueIdentifier (long oom = 1, long offset = 0) {

		return Group.GenerateUniqueIdentifier (oom, offset);
	}

	public float GetNextLocalRandomFloat (int iterationOffset) {

		return Group.GetNextLocalRandomFloat (iterationOffset);
	}

	public void SetFlag (string flag) {

		if (_flags.Contains (flag))
			return;

		_flags.Add (flag);
	}

	public bool IsFlagSet (string flag) {

		return _flags.Contains (flag);
	}

	public void UnsetFlag (string flag) {

		if (!_flags.Contains (flag))
			return;

		_flags.Remove (flag);
	}
}

public abstract class FactionEvent : WorldEvent {

	[XmlAttribute]
	public long FactionId;

	[XmlAttribute]
	public long PolityId;

	[XmlIgnore]
	public Faction Faction;

	[XmlIgnore]
	public Polity Polity;

	public FactionEvent () {

	}

	public FactionEvent (Faction faction, int triggerDate, long eventTypeId) : base (faction.World, triggerDate, faction.GenerateUniqueIdentifier (1000, eventTypeId)) {

		Faction = faction;
		FactionId = Faction.Id;

		Polity = faction.Polity;
		PolityId = Polity.Id;
	}

	public override bool CanTrigger () {

		if (Faction == null)
			return false;

		return Faction.StillPresent;
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Polity = World.GetPolity (PolityId);
		Faction = Polity.GetFaction (FactionId);
	}
}
