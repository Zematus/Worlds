using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class FactionRelationship {

	[XmlAttribute("Id")]
	public long Id;

	[XmlAttribute("Val")]
	public float Value;

	[XmlIgnore]
	public Faction Faction;

	public FactionRelationship () {
	}

	public FactionRelationship (Faction faction, float value) {

		Faction = faction;

		Id = faction.Id;

		Value = value;
	}
}

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
	public long LastUpdateDate;

	[XmlAttribute("LeadStDate")]
	public long LeaderStartDate;

	[XmlAttribute("IsCon")]
	public bool IsUnderPlayerGuidance = false;

	public FactionCulture Culture;

	public List<FactionRelationship> Relationships = new List<FactionRelationship> ();

	protected CellGroup _splitFactionCoreGroup;
	protected float _splitFactionMinProminence;
	protected float _splitFactionMaxProminence;

	protected Dictionary<long, FactionRelationship> relationships = new Dictionary<long, FactionRelationship> ();

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
	public bool IsInitialized = true;

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

		IsInitialized = false;
	}

	public void Initialize () {

		InitializeInternal ();
	
		IsInitialized = true;
	}

	protected virtual void InitializeInternal () {
	
	}

	public void Destroy (bool polityBeingDestroyed = false) {

		if (IsUnderPlayerGuidance) {
		
			Manager.SetGuidedFaction (null);
		}
		
		CoreGroup.RemoveFactionCore (this);

		if (!polityBeingDestroyed) {
			Polity.RemoveFaction (this);
		}

		foreach (FactionRelationship relationship in relationships.Values) {

			relationship.Faction.RemoveRelationship (this);
		}

		World.RemoveFaction (this);

		StillPresent = false;
	}

	public static int CompareId (Faction a, Faction b) {

		if (a.Id > b.Id)
			return 1;

		if (a.Id < b.Id)
			return -1;

		return 0;
	}

	public void SetToUpdate () {

		World.AddGroupToUpdate (CoreGroup);
		World.AddFactionToUpdate (this);
		World.AddPolityToUpdate (Polity);
	}

	public static void SetRelationship (Faction factionA, Faction factionB, float value) {
	
		factionA.SetRelationship (factionB, value);
		factionB.SetRelationship (factionA, value);
	}

	public void SetRelationship (Faction faction, float value) {

		value = Mathf.Clamp01 (value);

		if (!relationships.ContainsKey (faction.Id)) {
		
			FactionRelationship relationship = new FactionRelationship (faction, value);

			relationships.Add (faction.Id, relationship);
			Relationships.Add (relationship);

		} else {

			relationships[faction.Id].Value = value;
		}
	}

	public void RemoveRelationship (Faction faction) {

		if (!relationships.ContainsKey (faction.Id))
			throw new System.Exception ("relationship not present: " + faction.Id);

		FactionRelationship relationship = relationships [faction.Id];

		Relationships.Remove (relationship);
		relationships.Remove (faction.Id);
	}

	public float GetRelationshipValue (Faction faction) {

		if (!relationships.ContainsKey (faction.Id))
			throw new System.Exception ("relationship not present: " + faction.Id);

		return relationships[faction.Id].Value;
	}

	public void SetToSplit (CellGroup splitFactionCoreGroup, float splitFactionMinProminence, float splitFactionMaxProminence) {
	
		_splitFactionCoreGroup = splitFactionCoreGroup;
		_splitFactionMinProminence = splitFactionMinProminence;
		_splitFactionMaxProminence = splitFactionMaxProminence;

		World.AddFactionToSplit (this);
	}

	protected abstract void GenerateName (Faction parentFaction);

	protected Agent RequestCurrentLeader (int leadershipSpan, int minStartAge, int maxStartAge, int offset)
	{
		long spawnDate = CoreGroup.GeneratePastSpawnDate (CoreGroup.LastUpdateDate, leadershipSpan, offset++);

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
		long spawnDate = CoreGroup.GeneratePastSpawnDate (CoreGroup.LastUpdateDate, leadershipSpan, offset++);

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
	}

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

		foreach (FactionRelationship relationship in Relationships) {
		
			relationships.Add (relationship.Id, relationship);
			relationship.Faction = World.GetFaction (relationship.Id);

			if (relationship.Faction == null) {
				throw new System.Exception ("Faction is null, Id: " + relationship.Id);
			}
		}

		Flags.ForEach (f => _flags.Add (f));
	}

	public long GenerateUniqueIdentifier (long date, long oom = 1L, long offset = 0L) {

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

	public void SetUnderPlayerGuidance (bool state) {

		IsUnderPlayerGuidance = state;
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

	public void IncreasePreferenceValue (string id, float percentage) {

		CulturalPreference preference = Culture.GetPreference (id);

		if (preference == null)
			throw new System.Exception ("preference is null: " + id);

		float value = preference.Value;

		preference.Value = MathUtility.IncreaseByPercent (value, percentage);
	}

	public void DecreasePreferenceValue (string id, float percentage) {

		CulturalPreference preference = Culture.GetPreference (id);

		if (preference == null)
			throw new System.Exception ("preference is null: " + id);

		float value = preference.Value;

		preference.Value = MathUtility.DecreaseByPercent (value, percentage);
	}

	public float GetPreferenceValue (string id) {

		CulturalPreference preference = Culture.GetPreference (id);

		if (preference != null)
			return preference.Value; 

		return 0;
	}
}
