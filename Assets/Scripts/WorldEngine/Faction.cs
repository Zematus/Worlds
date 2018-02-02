using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

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

	[XmlAttribute("LastUpDate")]
	public int LastUpdateDate;

	[XmlAttribute("NextUpDate")]
	public int NextUpdateDate;

	[XmlAttribute("LeadStDate")]
	public int LeaderStartDate;

	[XmlAttribute("IsFoc")]
	public bool IsFocused = false;

	[XmlAttribute("IsCon")]
	public bool IsControlled = false;

	public FactionCulture Culture;

	protected CellGroup _splitFactionCoreGroup;

	public Name Name = null;

	// Do not call this property directly, only for serialization
	public Agent LastLeader = null;

	// Use this instead to get the leader
	public Agent CurrentLeader {

		get { 
			return RequestCurrentLeader ();
		}
	}

	public List<string> Flags;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public Polity Polity;

	[XmlIgnore]
	public CellGroup CoreGroup;

	[XmlIgnore]
	public CellGroup NewCoreGroup = null;

	[XmlIgnore]
	public FactionUpdateEvent UpdateEvent;

	private HashSet<string> _flags = new HashSet<string> ();

	public Faction () {

	}

	public Faction (string type, Polity polity, CellGroup coreGroup, float prominence, Faction parentFaction = null) {

		Type = type;

		World = polity.World;

		LastUpdateDate = World.CurrentDate;

		long idOffset = 0;

		if (parentFaction != null) {
		
			idOffset = parentFaction.Id + 1;
		}

		PolityId = polity.Id;
		Polity = polity;

		CoreGroup = coreGroup;
		CoreGroupId = coreGroup.Id;

		Id = GenerateUniqueIdentifier (World.CurrentDate, 100L, idOffset);

		Culture = new FactionCulture (this);

		CoreGroup.AddFactionCore (this);

		World.AddGroupToUpdate (CoreGroup);

		Prominence = prominence;

		GenerateName (parentFaction);

		World.AddUpdatedFaction (this);
	}

	public void Destroy (bool polityBeingDestroyed = false) {
		
		CoreGroup.RemoveFactionCore (this);

		if (!polityBeingDestroyed) {
			Polity.RemoveFaction (this);
		}

		World.RemoveFaction (this);

		StillPresent = false;
	}

	public void SetToSplit (CellGroup splitFactionCoreGroup) {
	
		_splitFactionCoreGroup = splitFactionCoreGroup;

		World.AddFactionToSplit (this);
	}

	protected abstract void GenerateName (Faction parentFaction);

	protected Agent RequestCurrentLeader (int leadershipSpan, int minStartAge, int maxStartAge, int offset)
	{
		int spawnDate = CoreGroup.GeneratePastSpawnDate (CoreGroup.LastUpdateDate, leadershipSpan, offset++);

		if ((LastLeader != null) && (spawnDate < LeaderStartDate)) {

			return LastLeader;
		}

		// Generate a birthdate from the leader spawnDate (when the leader takes over)
		int startAge = minStartAge + CoreGroup.GetLocalRandomInt (spawnDate, offset++, maxStartAge - minStartAge);

		LastLeader = new Agent (CoreGroup, spawnDate - startAge);
		LeaderStartDate = spawnDate;

		return LastLeader;
	}

	protected Agent RequestNewLeader (int leadershipSpan, int minStartAge, int maxStartAge, int offset)
	{
		int spawnDate = CoreGroup.GeneratePastSpawnDate (CoreGroup.LastUpdateDate, leadershipSpan, offset++);

		// Generate a birthdate from the leader spawnDate (when the leader takes over)
		int startAge = minStartAge + CoreGroup.GetLocalRandomInt (spawnDate, offset++, maxStartAge - minStartAge);

		LastLeader = new Agent (CoreGroup, spawnDate - startAge);
		LeaderStartDate = spawnDate;

		return LastLeader;
	}

	protected abstract Agent RequestCurrentLeader ();
	protected abstract Agent RequestNewLeader ();

	public abstract void Split ();

	public virtual void HandleUpdateEvent () {
	}

	public void Update () {

		if (!StillPresent) {
			Debug.LogWarning ("Faction is no longer present. Id: " + Id);

			return;
		}

		if (!Polity.StillPresent) {
			Debug.LogWarning ("Faction's polity is no longer present. Id: " + Id + " Polity Id: " + Polity.Id);

			return;
		}

		RequestCurrentLeader ();

		Culture.Update ();

		UpdateInternal ();

		LastUpdateDate = World.CurrentDate;

		World.AddPolityToUpdate (Polity);

		World.AddUpdatedFaction (this);
	}

	public void SetupForNextUpdate () {

		if (!StillPresent)
			return;

		Profiler.BeginSample ("Calculate Next Update Date");

		NextUpdateDate = CalculateNextUpdateDate ();

		Profiler.EndSample ();

		LastUpdateDate = World.CurrentDate;

		if (UpdateEvent == null) {
			UpdateEvent = new FactionUpdateEvent (this, NextUpdateDate);
		} else {
			UpdateEvent.Reset (NextUpdateDate);
		}

		World.InsertEventToHappen (UpdateEvent);
	}

	public abstract int CalculateNextUpdateDate ();

	public void PrepareNewCoreGroup (CellGroup coreGroup) {

		NewCoreGroup = coreGroup;
	}

	public void MigrateToNewCoreGroup () {

		CoreGroup.RemoveFactionCore (this);

		CoreGroup = NewCoreGroup;
		CoreGroupId = NewCoreGroup.Id;

		CoreGroup.AddFactionCore (this);

		if (IsDominant) {
			Polity.SetCoreGroup (CoreGroup);
		}
	}

	protected abstract void UpdateInternal ();

	public virtual void Synchronize () {

		Flags = new List<string> (_flags);

		Culture.Synchronize ();

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

		Culture.World = World;
		Culture.Faction = this;
		Culture.FinalizeLoad ();

		Flags.ForEach (f => _flags.Add (f));

		// Generate Update Event

		UpdateEvent = new FactionUpdateEvent (this, NextUpdateDate);
		World.InsertEventToHappen (UpdateEvent);
	}

	public long GenerateUniqueIdentifier (int date, long oom = 1L, long offset = 0L) {

		return CoreGroup.GenerateUniqueIdentifier (date, oom, offset);
	}

	public float GetNextLocalRandomFloat (int iterationOffset) {

		return CoreGroup.GetNextLocalRandomFloat (iterationOffset + (int)Id);
	}

	public float GetLocalRandomFloat (int date, int iterationOffset) {

		return CoreGroup.GetLocalRandomFloat (date, iterationOffset + (int)Id);
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

public abstract class FactionEventMessage : WorldEventMessage {

	[XmlAttribute]
	public long FactionId;

	[XmlIgnore]
	public Faction Faction {
		get { return World.GetFaction (FactionId); }
	}

	public FactionEventMessage () {

	}

	public FactionEventMessage (Faction faction, long id, long date) : base (faction.World, id, date) {

		FactionId = faction.Id;
	}
}

public abstract class FactionDecision : Decision {
	
	public Faction Faction;

	public FactionDecision (Faction faction) : base () {

		Faction = faction;
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

		return ((long)triggerDate * 1000000000) + ((faction.Id % 1000000L) * 1000L) + eventTypeId;
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

public class FactionUpdateEvent : FactionEvent {

	public FactionUpdateEvent () {

		DoNotSerialize = true;
	}

	public FactionUpdateEvent (Faction faction, int triggerDate) : base (faction, triggerDate, FactionUpdateEventId) {

		DoNotSerialize = true;
	}

	public override bool IsStillValid ()
	{
		if (!base.IsStillValid ())
			return false;

		if (Faction.NextUpdateDate != TriggerDate)
			return false;

		return true;
	}

	public override void Trigger () {

		Faction.HandleUpdateEvent ();

		World.AddFactionToUpdate (Faction);
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Faction.UpdateEvent = this;
	}
}
