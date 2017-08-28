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

	[XmlAttribute("CGrpId")]
	public long CoreGroupId;

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

	[XmlIgnore]
	public CellGroup CoreGroup;

	[XmlIgnore]
	public bool CoreGroupUpdated = false;

	private HashSet<string> _flags = new HashSet<string> ();

	public Faction () {

	}

	public Faction (string type, Polity polity, CellGroup coreGroup, float prominence, Faction parentFaction = null) {

		Type = type;

		World = polity.World;

		CoreGroup = coreGroup;
		CoreGroupId = coreGroup.Id;

		CoreGroup.AddFactionCore (this);

		int idOffset = 0;

		if (parentFaction != null) {
		
			idOffset = (int)parentFaction.Id;
		}

		PolityId = polity.Id;
		Polity = polity;

		Id = GenerateUniqueIdentifier (offset: idOffset);

		Prominence = prominence;

		GenerateName (parentFaction);
	}

	public void Destroy () {
		
		Polity.RemoveFaction (this, true);

		StillPresent = false;
	}

	protected abstract void GenerateName (Faction parentFaction);

	public void Update () {

		if (!Polity.StillPresent) {

			return;
		}

		UpdateInternal ();

		World.AddPolityToUpdate (Polity);
	}

	public void SetCoreGroup (CellGroup coreGroup) {
		
		if (coreGroup == null) {
			Debug.LogError ("New CoreGroup is null");
		}

		if (CoreGroup == null) {
			Debug.LogError ("Old CoreGroup is null");
		}

		CoreGroup.RemoveFactionCore (this);

		CoreGroup = coreGroup;
		CoreGroupId = coreGroup.Id;

		CoreGroup.AddFactionCore (this);

		if (IsDominant) {
			Polity.SetCoreGroup (CoreGroup);
		}

		CoreGroupUpdated = true;
	}

	protected abstract void UpdateInternal ();

	public virtual void Synchronize () {

		Flags = new List<string> (_flags);

		Name.Synchronize ();
	}

	public virtual void FinalizeLoad () {

		Name.World = World;
		Name.FinalizeLoad ();

		CoreGroup = World.GetGroup (CoreGroupId);

		Polity = World.GetPolity (PolityId);

		if (Polity == null) {
			throw new System.Exception ("Missing Polity with Id " + PolityId);
		}

		Flags.ForEach (f => _flags.Add (f));
	}

	public long GenerateUniqueIdentifier (long oom = 1, long offset = 0) {

		return CoreGroup.GenerateUniqueIdentifier (oom, offset);
	}

	public float GetNextLocalRandomFloat (int iterationOffset) {

		return CoreGroup.GetNextLocalRandomFloat (iterationOffset + (int)Id);
	}

	public int GetNextLocalRandomInt (int iterationOffset, int maxValue) {

		return CoreGroup.GetNextLocalRandomInt (iterationOffset + (int)Id, maxValue);
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
		PolityId = Polity.Id;
		Prominence = targetProminence;

		targetPolity.AddFaction (this);
	}

	public virtual bool ShouldMigrateFactionCore (CellGroup sourceGroup, CellGroup targetGroup) {

		return false;
	}

	public virtual bool ShouldMigrateFactionCore (CellGroup sourceGroup, TerrainCell targetCell, float targetInfluence, int targetPopulation) {

		return false;
	}
}

public abstract class FactionEventMessage : PolityEventMessage {

	[XmlAttribute]
	public long FactionId;

	[XmlIgnore]
	public Faction Faction {
		get { return Polity.GetFaction (FactionId); }
	}

	public FactionEventMessage () {

	}

	public FactionEventMessage (Faction faction, long id, long date) : base (faction.Polity, id, date) {

		FactionId = faction.Id;
	}
}

public abstract class FactionEvent : WorldEvent {

	[XmlAttribute]
	public long FactionId;

	[XmlAttribute]
	public long PolityId;

	[XmlAttribute]
	public long EventTypeId;

	[XmlIgnore]
	public Faction Faction;

	public FactionEvent () {

	}

	public FactionEvent (Faction faction, int triggerDate, long eventTypeId) : base (faction.World, triggerDate, GenerateUniqueIdentifier (faction, triggerDate, eventTypeId)) {

		Faction = faction;
		FactionId = Faction.Id;

		EventTypeId = eventTypeId;

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			string factionId = "Id: " + faction.Id;
//
//			SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("FactionEvent - Faction: " + factionId, "TriggerDate: " + TriggerDate);
//
//			Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//		}
//		#endif
	}

	public static long GenerateUniqueIdentifier (Faction faction, int triggerDate, long eventTypeId) {

		CellGroup coreGroup = faction.CoreGroup;

		return ((long)triggerDate * 100000000000) + ((long)coreGroup.Longitude * 100000000) + ((long)coreGroup.Latitude * 100000) + (eventTypeId * 1000) + faction.Id;
	}

	public override bool IsStillValid () {
	
		if (!base.IsStillValid ())
			return false;
		
		if (Faction == null)
			return false;

		if (!Faction.StillPresent)
			return false;

		Polity polity = World.GetPolity (Faction.PolityId);

		if (polity == null) {

			Debug.LogError ("FactionEvent: Polity with Id:" + PolityId + " not found");
		}

		return true;
	}

	public override void Synchronize ()
	{
		PolityId = Faction.PolityId;

		base.Synchronize ();
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Polity polity = World.GetPolity (PolityId);

		if (polity == null) {

			Debug.LogError ("FactionEvent: Polity with Id:" + PolityId + " not found");
		}

		Faction = polity.GetFaction (FactionId);

		if (Faction == null) {

			Debug.LogError ("FactionEvent: Faction with Id:" + FactionId + " not found");
		}
	}

	public virtual void Reset (int newTriggerDate) {

		Reset (newTriggerDate, GenerateUniqueIdentifier (Faction, newTriggerDate, EventTypeId));
	}
}
