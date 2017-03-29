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

	[XmlAttribute("PolId")]
	public long PolityId;

	[XmlAttribute("Prom")]
	public float Prominence;

	[XmlAttribute("StilPres")]
	public bool StillPresent = true;

	[XmlAttribute("IsDom")]
	public bool IsDominant = false;

	public Name Name = null;

	public List<string> Flags;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public Polity Polity;

	private HashSet<string> _flags = new HashSet<string> ();

	public Faction () {

	}

	public Faction (string type, Polity polity, float prominence, Faction parentFaction = null) {

		Type = type;

		World = polity.World;

		int idOffset = 0;

		if (parentFaction != null) {
		
			idOffset = (int)parentFaction.Id;
		}

		Id = polity.GenerateUniqueIdentifier (offset: idOffset);

		PolityId = polity.Id;
		Polity = polity;

		Prominence = prominence;

		GenerateName ();
	}

	public void Destroy () {
		
		Polity.RemoveFaction (this);

		StillPresent = false;
	}

	public abstract void GenerateName ();

	public void Update () {

		if (!Polity.StillPresent) {

			return;
		}

		UpdateInternal ();
	}

	protected abstract void UpdateInternal ();

	public virtual void Synchronize () {

		Flags = new List<string> (_flags);

		Name.Synchronize ();
	}

	public virtual void FinalizeLoad () {

		Name.World = World;
		Name.FinalizeLoad ();

		Polity = World.GetPolity (PolityId);

		if (Polity == null) {
			throw new System.Exception ("Missing Polity with Id " + PolityId);
		}

		Flags.ForEach (f => _flags.Add (f));
	}

	public long GenerateUniqueIdentifier (long oom = 1, long offset = 0) {

		return Polity.GenerateUniqueIdentifier (oom, offset);
	}

	public float GetNextLocalRandomFloat (int iterationOffset) {

		return Polity.GetNextLocalRandomFloat (iterationOffset);
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

	public virtual void SetDominant (bool state) {
	
		IsDominant = state;
	}

	public void ChangePolity (Polity targetPolity, float targetProminence) {
	
		if ((targetPolity == null) || (!targetPolity.StillPresent)) 
			throw new System.Exception ("target Polity is null or not Present");

		Polity.RemoveFaction (this);

		Polity = targetPolity;
		Prominence = targetProminence;

		targetPolity.AddFaction (this);
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
