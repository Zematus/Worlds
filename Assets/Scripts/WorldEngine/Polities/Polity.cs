using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;
using System.Linq;
using System.Xml.Schema;

public delegate float GroupValueCalculationDelegate (CellGroup group);
public delegate float FactionValueCalculationDelegate (Faction faction);
public delegate float PolityContactValueCalculationDelegate (PolityContact contact);

[XmlInclude(typeof(Tribe))]
public abstract class Polity : ISynchronizable {

	public const float TimeEffectConstant = CellGroup.GenerationSpan * 2500;

	public const float CoreDistanceEffectConstant = 10000;

	public const float MinPolityProminence = 0.001f;

	[XmlAttribute("CGrpId")]
	public long CoreGroupId;

	[XmlAttribute("TotalAdmCost")]
	public float TotalAdministrativeCost = 0;

	[XmlAttribute("TotalPop")]
	public float TotalPopulation = 0;

	[XmlAttribute("PromArea")]
	public float ProminenceArea = 0;

	[XmlAttribute("FctnCount")]
	public int FactionCount { get; private set; }

	[XmlAttribute("StilPres")]
	public bool StillPresent = true;

	[XmlAttribute("DomFactId")]
	public long DominantFactionId;

	[XmlAttribute("IsFoc")]
	public bool IsUnderPlayerFocus = false;

	public List<string> Flags;
    
    public DelayedLoadXmlSerializableDictionary<long, CellGroup> ProminencedGroups = new DelayedLoadXmlSerializableDictionary<long, CellGroup>();

    public Territory Territory;

	public PolityCulture Culture;

    public DelayedLoadXmlSerializableDictionary<long, Faction> Factions = new DelayedLoadXmlSerializableDictionary<long, Faction>();

    public List<long> EventMessageIds;

    public XmlSerializableDictionary<long, PolityContact> Contacts = new XmlSerializableDictionary<long, PolityContact>();

    public List<PolityEventData> EventDataList = new List<PolityEventData>();

    [XmlIgnore]
    public PolityInfo Info;

    [XmlIgnore]
	public World World;

	[XmlIgnore]
	public CellGroup CoreGroup;

	[XmlIgnore]
	public Faction DominantFaction;

	[XmlIgnore]
	public bool WillBeUpdated;

    public string Type
    {
        get { return Info.Type; }
    }

    public long Id
    {
        get { return Info.Id; }
    }

    public Name Name
    {
        get { return Info.Name; }
    }

    public Agent CurrentLeader
    {
        get
        {
            return DominantFaction.CurrentLeader;
        }
    }

    protected class WeightedGroup : CollectionUtility.ElementWeightPair<CellGroup> {

		public WeightedGroup (CellGroup group, float weight) : base (group, weight) {

		}
	}

	protected class WeightedFaction : CollectionUtility.ElementWeightPair<Faction> {

		public WeightedFaction (Faction faction, float weight) : base (faction, weight) {

		}
	}

	protected class WeightedPolityContact : CollectionUtility.ElementWeightPair<PolityContact> {

		public WeightedPolityContact (PolityContact contact, float weight) : base (contact, weight) {

		}
	}

	protected Dictionary<long, PolityEvent> _events = new Dictionary<long, PolityEvent> ();

	//private Dictionary<long, WeightedGroup> _polityPopPerGroup = new Dictionary<long, WeightedGroup> ();

	private HashSet<string> _flags = new HashSet<string> ();

	private bool _willBeRemoved = false;

	#if DEBUG
	private bool _populationCensusUpdated = false;
	#endif

	private HashSet<long> _eventMessageIds = new HashSet<long> ();

	public Polity () {
	
	}

	protected Polity (string type, CellGroup coreGroup, Polity parentPolity = null) {
        
		World = coreGroup.World;

		Territory = new Territory (this);

		CoreGroup = coreGroup;
		CoreGroupId = coreGroup.Id;

		long idOffset = 0;

		if (parentPolity != null) {
		
			idOffset = parentPolity.Id + 1;
		}

		long id = GenerateUniqueIdentifier (World.CurrentDate, 100L, idOffset);

        Info = new PolityInfo(type, id, this);

		Culture = new PolityCulture (this);

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (CoreGroupId == Manager.TracingData.GroupId) {
//				string groupId = "Id:" + CoreGroupId + "|Long:" + CoreGroup.Longitude + "|Lat:" + CoreGroup.Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"new Polity - Group:" + groupId + 
//					", Polity.Id: " + Id,
//					"CurrentDate: " + World.CurrentDate  +
//					", CoreGroup:" + groupId + 
//					", Polity.TotalGroupProminenceValue: " + TotalGroupProminenceValue + 
//					", coreGroupProminenceValue: " + coreGroupProminenceValue + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif
	}

	public void Initialize () {
	
		Culture.Initialize ();

		foreach (Faction faction in Factions.Values) {

			if (!faction.IsInitialized) {

				faction.Initialize ();
			}
		}

		InitializeInternal ();
	}

	public abstract void InitializeInternal ();

	public void Destroy()
    {
        if (Territory.IsSelected)
        {
            Manager.SetSelectedTerritory(null);
        }

        if (IsUnderPlayerFocus)
        {
            Manager.UnsetFocusOnPolity(this);
        }

        List<PolityContact> contacts = new List<PolityContact>(Contacts.Values);

        foreach (PolityContact contact in contacts)
        {
            Polity.RemoveContact(this, contact.Polity);
        }

        List<Faction> factions = new List<Faction>(Factions.Values);

        foreach (Faction faction in factions)
        {
            faction.Destroy(true);
        }

        foreach (CellGroup group in ProminencedGroups.Values)
        {
            group.RemovePolityProminence(this);

            World.AddGroupToPostUpdate_AfterPolityUpdate(group);
        }

        Info.Polity = null;

        StillPresent = false;
    }

    public string GetNameAndTypeString()
    {
        return Info.GetNameAndTypeString();
    }

    public string GetNameAndTypeStringBold()
    {
        return Info.GetNameAndTypeStringBold();
    }

    public void SetUnderPlayerFocus (bool state, bool setDominantFactionFocused = true) {
	
		IsUnderPlayerFocus = state;
	}

	public void AddEventMessage (WorldEventMessage eventMessage) {

		if (IsUnderPlayerFocus)
			World.AddEventMessageToShow (eventMessage);

		_eventMessageIds.Add (eventMessage.Id);
	}

	public bool HasEventMessage (long id) {
	
		return _eventMessageIds.Contains (id);
	}

	public void SetCoreGroup (CellGroup coreGroup) {
	
		CoreGroup = coreGroup;
		CoreGroupId = coreGroup.Id;
	}

	public long GenerateUniqueIdentifier (long date, long oom = 1L, long offset = 0L) {

		return CoreGroup.GenerateUniqueIdentifier (date, oom, offset);
	}

	public float GetNextLocalRandomFloat (int iterationOffset) {

		return CoreGroup.GetNextLocalRandomFloat (iterationOffset + (int)Id);
	}

	public int GetNextLocalRandomInt (int iterationOffset, int maxValue) {

		return CoreGroup.GetNextLocalRandomInt (iterationOffset + (int)Id, maxValue);
	}

    public void AddFaction(Faction faction)
    {
        //		foreach (Faction existingFaction in _factions.Values) {
        //
        //			if (!existingFaction.HasRelationship (faction)) {
        //			
        //				Faction.SetRelationship (existingFaction, faction, 0.5f);
        //			}
        //		}

        Factions.Add(faction.Id, faction);

        if (!World.ContainsFactionInfo(faction.Id))
        {
            World.AddFactionInfo(faction.Info);
        }

        World.AddFactionToUpdate(faction);

        FactionCount++;
    }

    public void RemoveFaction (Faction faction) {

		Factions.Remove (faction.Id);

		if (Factions.Count <= 0) {
			
			//#if DEBUG
			//Debug.Log ("Polity will be removed due to losing all factions. faction id: " + faction.Id + ", polity id:" + Id);
			//#endif

			PrepareToRemoveFromWorld ();
			return;
		}

		if (DominantFaction == faction) {
			UpdateDominantFaction ();
		}

		World.AddFactionToUpdate (faction);

		FactionCount--;
	}

	public Faction GetFaction (long id) {

		Faction faction;

		Factions.TryGetValue (id, out faction);

		return faction;
	}

	public ICollection<Faction> GetFactions (long id) {

		return Factions.Values;
	}

	public void UpdateDominantFaction () {
	
		Faction mostProminentFaction = null;
		float greatestInfluence = float.MinValue;

		foreach (Faction faction in Factions.Values) {
		
			if (faction.Influence > greatestInfluence) {
			
				mostProminentFaction = faction;
				greatestInfluence = faction.Influence;
			}
		}

		if ((mostProminentFaction == null) || (!mostProminentFaction.StillPresent))
			throw new System.Exception ("Faction is null or not present");

		SetDominantFaction (mostProminentFaction);
	}

	public void SetDominantFaction (Faction faction) {

		if (DominantFaction == faction)
			return;

		if (DominantFaction != null) {
		
			DominantFaction.SetDominant (false);

			World.AddFactionToUpdate (DominantFaction);
		}

		if ((faction == null) || (!faction.StillPresent))
			throw new System.Exception ("Faction is null or not present");

		if (faction.Polity != this)
			throw new System.Exception ("Faction is not part of polity");
	
		DominantFaction = faction;

		if (faction != null) {
			DominantFactionId = faction.Id;

			faction.SetDominant (true);

			SetCoreGroup (faction.CoreGroup);

            foreach (PolityContact contact in Contacts.Values)
            {
				if (!faction.HasRelationship (contact.Polity.DominantFaction)) {

					Faction.SetRelationship (faction, contact.Polity.DominantFaction, 0.5f);
				}
			}

			World.AddFactionToUpdate (faction);
		}

		World.AddPolityToUpdate (this);
	}

	public static void AddContact (Polity polityA, Polity polityB, int initialGroupCount) {

		polityA.AddContact (polityB, initialGroupCount);
		polityB.AddContact (polityA, initialGroupCount);
	}

	public void AddContact (Polity polity, int initialGroupCount) {

        if (!Contacts.ContainsKey(polity.Id))
        {
			PolityContact contact = new PolityContact (polity, initialGroupCount);
            
			Contacts.Add (polity.Id, contact);

			if (!DominantFaction.HasRelationship (polity.DominantFaction)) {
			
				DominantFaction.SetRelationship (polity.DominantFaction, 0.5f);
			}
		}
        else
        {
			throw new System.Exception ("Unable to modify existing polity contact. polityA: " + Id + ", polityB: " + polity.Id);
		}
    }

    public static void RemoveContact(Polity polityA, Polity polityB)
    {
        polityA.RemoveContact(polityB);
        polityB.RemoveContact(polityA);
    }

    public void RemoveContact (Polity polity) {

        if (!Contacts.ContainsKey(polity.Id))
			return;

		PolityContact contact = Contacts [polity.Id];

        Contacts.Remove (polity.Id);
	}

	public int GetContactGroupCount (Polity polity) {

        if (!Contacts.ContainsKey(polity.Id))
			return 0;

        return Contacts[polity.Id].GroupCount;
	}

	public static void IncreaseContactGroupCount (Polity polityA, Polity polityB) {

		polityA.IncreaseContactGroupCount (polityB);
		polityB.IncreaseContactGroupCount (polityA);
	}

	public void IncreaseContactGroupCount (Polity polity) {

        if (!Contacts.ContainsKey(polity.Id))
        {
			PolityContact contact = new PolityContact (polity);
            
			Contacts.Add (polity.Id, contact);

			if (!DominantFaction.HasRelationship (polity.DominantFaction)) {

				DominantFaction.SetRelationship (polity.DominantFaction, 0.5f);
			}
		}

        Contacts[polity.Id].GroupCount++;
	}

	public static void DecreaseContactGroupCount (Polity polityA, Polity polityB) {

		polityA.DecreaseContactGroupCount (polityB);
		polityB.DecreaseContactGroupCount (polityA);
	}

	public void DecreaseContactGroupCount (Polity polity) {

        if (!Contacts.ContainsKey(polity.Id))
			throw new System.Exception ("(id: " + Id + ") contact not present: " + polity.Id + " - Date: " + World.CurrentDate);

        PolityContact contact = Contacts[polity.Id];

		contact.GroupCount--;

		if (contact.GroupCount <= 0)
        {
            Contacts.Remove (polity.Id);
		}

	}

	public float GetRelationshipValue (Polity polity) {

        if (!Contacts.ContainsKey(polity.Id))
			throw new System.Exception ("(id: " + Id + ") contact not present: " + polity.Id);

		return DominantFaction.GetRelationshipValue (polity.DominantFaction);
	}

	public IEnumerable<Faction> GetFactions (bool ordered = false) {

		if (ordered) {
			List<Faction> sortedFactions = new List<Faction> (Factions.Values);
			sortedFactions.Sort (Faction.CompareId);

			return sortedFactions;
		}

		return Factions.Values;
	}

	public IEnumerable<Faction> GetFactions (string type) {

		foreach (Faction faction in Factions.Values) {

			if (faction.Type == type)
				yield return faction;
		}
	}

	public IEnumerable<T> GetFactions<T> () where T : Faction {

		foreach (T faction in Factions.Values) {

				yield return faction;
		}
	}

	public void NormalizeFactionInfluences () {
	
		float totalInfluence = 0;

		foreach (Faction f in Factions.Values) {
		
			totalInfluence += f.Influence;
		}

		if (totalInfluence <= 0) {
			throw new System.Exception ("Total influence equal or less than zero: " + totalInfluence + ", polity id:" + Id);
		}

		foreach (Faction f in Factions.Values) {

			f.Influence = f.Influence / totalInfluence;
		}
	}

	public static void TransferInfluence (Faction sourceFaction, Faction targetFaction, float percentage) {

		// Can only tranfer influence between factions belonging to the same polity

		if (sourceFaction.PolityId != targetFaction.PolityId)
			throw new System.Exception ("Source faction and target faction do not belong to same polity");

		// Always reduce influence of source faction and increase promience of target faction

		if ((percentage < 0f) || (percentage > 1f))
			throw new System.Exception ("Invalid percentage: " + percentage);

		float oldSourceInfluenceValue = sourceFaction.Influence;

		sourceFaction.Influence = oldSourceInfluenceValue * (1f - percentage);

		float influenceDelta = oldSourceInfluenceValue - sourceFaction.Influence;

		targetFaction.Influence += influenceDelta;

		sourceFaction.Polity.UpdateDominantFaction ();
	}

	public void PrepareToRemoveFromWorld () {

		World.AddPolityToRemove (this);

		_willBeRemoved = true;
	}

	public void Update () {

		if (_willBeRemoved) {
			return;
		}

		if (!StillPresent) {
			Debug.LogWarning ("Polity is no longer present. Id: " + Id);

			return;
		}

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			Manager.RegisterDebugEvent ("DebugMessage", 
//				"Update - Polity:" + Id + 
//				", CurrentDate: " + World.CurrentDate + 
//				", ProminencedGroups.Count: " + ProminencedGroups.Count + 
//				", TotalGroupProminenceValue: " + TotalGroupProminenceValue + 
//				"");
//		}
//		#endif

		WillBeUpdated = false;

		if (ProminencedGroups.Count <= 0) {

			#if DEBUG
			Debug.Log ("Polity will be removed due to losing all prominenced groups. polity id:" + Id);
			#endif

			PrepareToRemoveFromWorld ();
			return;
		}

		Profiler.BeginSample ("Normalize Faction Influences");

		NormalizeFactionInfluences ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Run Census");

		RunCensus ();

		Profiler.EndSample ();

		#if DEBUG
		_populationCensusUpdated = true;
		#endif

		Profiler.BeginSample ("Update Culture");
	
		Culture.Update ();

		Profiler.EndSample ();

		Profiler.BeginSample ("Update Internal");

		UpdateInternal ();

		Profiler.EndSample ();

		#if DEBUG
		_populationCensusUpdated = false;
		#endif

		Manager.AddUpdatedCells (Territory.GetCells (), CellUpdateType.Territory);
	}

	protected abstract void UpdateInternal ();

	public void RunCensus () {

		TotalAdministrativeCost = 0;
		TotalPopulation = 0;
		ProminenceArea = 0;

        //_polityPopPerGroup.Clear ();

        Profiler.BeginSample("foreach");

        foreach (CellGroup group in ProminencedGroups.Values) {
            
            Profiler.BeginSample("group - GetPolityProminence");

            PolityProminence pi = group.GetPolityProminence (this);

            Profiler.EndSample();

            Profiler.BeginSample("add administrative cost");

            if (pi.AdministrativeCost < float.MaxValue)
				TotalAdministrativeCost += pi.AdministrativeCost;
			else
				TotalAdministrativeCost = float.MaxValue;

            Profiler.EndSample();

            Profiler.BeginSample("add pop");

            float polityPop = group.Population * pi.Value;

			TotalPopulation += polityPop;

            Profiler.EndSample();

            //Profiler.BeginSample("add WeightedGroup to _polityPopPerGroup");

            //_polityPopPerGroup.Add (group.Id, new WeightedGroup (group, polityPop));

            //Profiler.EndSample();

            Profiler.BeginSample("add area");

            ProminenceArea += group.Cell.Area;

            Profiler.EndSample();
        }

        Profiler.EndSample();
    }

	//protected CellGroup GetGroupWithMostProminencedPop () {

	//	#if DEBUG
	//	if (!_populationCensusUpdated)
	//		Debug.LogWarning ("This function should only be called within polity updates after executing RunPopulationCensus");
	//	#endif

	//	CellGroup groupWithMostProminencedPop = null;
	//	float maxProminencedGroupPopulation = 0;
	
	//	foreach (KeyValuePair<long, WeightedGroup> pair in _polityPopPerGroup) {
		
	//		if (maxProminencedGroupPopulation < pair.Value.Weight) {

	//			maxProminencedGroupPopulation = pair.Value.Weight;

	//			groupWithMostProminencedPop = pair.Value.Value;
	//		}
	//	}

	//	return groupWithMostProminencedPop;
	//}

	public void AddProminencedGroup (CellGroup group) {
	
		ProminencedGroups.Add (group.Id, group);
	}

	public void RemoveProminencedGroup (CellGroup group) {

		ProminencedGroups.Remove (group.Id);
	}

	public virtual void Synchronize () {

		Flags = new List<string> (_flags);

		EventDataList.Clear ();

		foreach (PolityEvent e in _events.Values) {

			EventDataList.Add (e.GetData () as PolityEventData);
		}

		Culture.Synchronize ();

		Territory.Synchronize ();

		Name.Synchronize ();

		EventMessageIds = new List<long> (_eventMessageIds);
	}

    private CellGroup GetGroupOrThrow(long id)
    {
        CellGroup group = World.GetGroup(id);

        if (group == null)
        {
            string message = "Missing Group with Id " + id + " in polity with Id " + Id;
            throw new System.Exception(message);
        }

        return group;
    }

    private Faction GetFactionOrThrow(long id)
    {
        Faction faction = World.GetFaction(id);

        if (faction == null)
        {
            string message = "Missing Faction with Id " + faction + " in polity with Id " + Id;
            throw new System.Exception(message);
        }

        return faction;
    }

    public virtual void FinalizeLoad () {

		foreach (long messageId in EventMessageIds) {

			_eventMessageIds.Add (messageId);
		}

		Name.World = World;
		Name.FinalizeLoad ();

		CoreGroup = World.GetGroup (CoreGroupId);

		if (CoreGroup == null) {
			string message = "Missing Group with Id " + CoreGroupId + " in polity with Id " + Id;
			throw new System.Exception (message);
		}

        ProminencedGroups.FinalizeLoad(GetGroupOrThrow);
        Factions.FinalizeLoad(GetFactionOrThrow);

		DominantFaction = GetFaction (DominantFactionId);

		Territory.World = World;
		Territory.Polity = this;
		Territory.FinalizeLoad ();

		Culture.World = World;
		Culture.Polity = this;
		Culture.FinalizeLoad ();

        foreach (PolityContact contact in Contacts.Values)
        {
			contact.Polity = World.GetPolity(contact.Id);

			if (contact.Polity == null) {
				throw new System.Exception ("Polity is null, Id: " + contact.Id);
			}
		}

        GenerateEventsFromData();

		Flags.ForEach (f => _flags.Add (f));
	}

	protected abstract void GenerateEventsFromData ();

	public void AddEvent (PolityEvent polityEvent) {

		if (_events.ContainsKey (polityEvent.TypeId))
			throw new System.Exception ("Event of type " + polityEvent.TypeId + " already present");

		_events.Add (polityEvent.TypeId, polityEvent);
		World.InsertEventToHappen (polityEvent);
	}

	public PolityEvent GetEvent (long typeId) {

		if (!_events.ContainsKey (typeId))
			return null;

		return _events[typeId];
	}

	public void ResetEvent (long typeId, long newTriggerDate) {

		if (!_events.ContainsKey (typeId))
			throw new System.Exception ("Unable to find event of type: " + typeId);

		PolityEvent polityEvent = _events [typeId];

		polityEvent.Reset (newTriggerDate);
		World.InsertEventToHappen (polityEvent);
	}

	public abstract float CalculateGroupProminenceExpansionValue (CellGroup sourceGroup, CellGroup targetGroup, float sourceValue);

	public virtual void GroupUpdateEffects (CellGroup group, float prominenceValue, float totalPolityProminenceValue, long timeSpan) {

		if (group.Culture.GetFoundDiscoveryOrToFind (TribalismDiscovery.TribalismDiscoveryId) == null) {

			group.SetPolityProminence (this, 0);

			return;
		}

		float coreFactionDistance = group.GetFactionCoreDistance (this);

		float coreDistancePlusConstant = coreFactionDistance + CoreDistanceEffectConstant;

		float distanceFactor = 0;

		if (coreDistancePlusConstant > 0)
			distanceFactor = CoreDistanceEffectConstant / coreDistancePlusConstant;

		TerrainCell groupCell = group.Cell;

		float maxTargetValue = 1f;
		float minTargetValue = 0.8f * totalPolityProminenceValue;

		float randomModifier = groupCell.GetNextLocalRandomFloat (RngOffsets.POLITY_UPDATE_EFFECTS + (int)Id);
		randomModifier *= distanceFactor;
		float targetValue = ((maxTargetValue - minTargetValue) * randomModifier) + minTargetValue;

		float scaledValue = (targetValue - totalPolityProminenceValue) * prominenceValue / totalPolityProminenceValue;
		targetValue = prominenceValue + scaledValue;

		float timeFactor = timeSpan / (float)(timeSpan + TimeEffectConstant);

		prominenceValue = (prominenceValue * (1 - timeFactor)) + (targetValue * timeFactor);

		prominenceValue = Mathf.Clamp01 (prominenceValue);

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			if (group.Id == Manager.TracingData.GroupId) {
//				string groupId = "Id:" + group.Id + "|Long:" + group.Longitude + "|Lat:" + group.Latitude;
//
//				SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//					"UpdateEffects - Group:" + groupId + 
//					", Polity.Id: " + Id,
//					"CurrentDate: " + World.CurrentDate  +
//					", randomFactor: " + randomFactor + 
//					", groupTotalPolityProminenceValue: " + groupTotalPolityProminenceValue + 
//					", Polity.TotalGroupProminenceValue: " + TotalGroupProminenceValue + 
//					", unmodInflueceValue: " + unmodInflueceValue + 
//					", prominenceValue: " + prominenceValue + 
//					", group.LastUpdateDate: " + group.LastUpdateDate + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		group.SetPolityProminence (this, prominenceValue);
	}

	public void CalculateAdaptionToCell (TerrainCell cell, out float foragingCapacity, out float survivability) {

		float modifiedForagingCapacity = 0;
		float modifiedSurvivability = 0;

//		Profiler.BeginSample ("Get Polity Skill Values");

		foreach (string biomeName in cell.PresentBiomeNames) {

//			Profiler.BeginSample ("Try Get Polity Biome Survival Skill");

			float biomePresence = cell.GetBiomePresence(biomeName);

			Biome biome = Biome.Biomes [biomeName];

			string skillId = BiomeSurvivalSkill.GenerateId (biome);

			CulturalSkill skill = Culture.GetSkill (skillId);

			if (skill != null) {

//				Profiler.BeginSample ("Evaluate Polity Biome Survival Skill");

				modifiedForagingCapacity += biomePresence * biome.ForagingCapacity * skill.Value;
				modifiedSurvivability += biomePresence * (biome.Survivability + skill.Value * (1 - biome.Survivability));

//				Profiler.EndSample ();

			} else {
				
				modifiedSurvivability += biomePresence * biome.Survivability;
			}

//			Profiler.EndSample ();
		}

//		Profiler.EndSample ();

		float altitudeSurvivabilityFactor = 1 - Mathf.Clamp01 (cell.Altitude / World.MaxPossibleAltitude);

		modifiedSurvivability = (modifiedSurvivability * (1 - cell.FarmlandPercentage)) + cell.FarmlandPercentage;

		foragingCapacity = modifiedForagingCapacity * (1 - cell.FarmlandPercentage);
		survivability = modifiedSurvivability * altitudeSurvivabilityFactor;

		if (foragingCapacity > 1) {
			throw new System.Exception ("ForagingCapacity greater than 1: " + foragingCapacity);
		}

		if (survivability > 1) {
			throw new System.Exception ("Survivability greater than 1: " + survivability);
		}
	}

	public CellGroup GetRandomGroup (int rngOffset, GroupValueCalculationDelegate calculateGroupValue, bool nullIfNoValidGroup = false) {

        // Instead of this cumbersome sampling mechanism, create a sample list for each polity that adds/removes groups that update 
        // or have been selected by this method

        int maxSampleSize = 20;

        int sampleGroupLength = 1 + (ProminencedGroups.Count / maxSampleSize);

        int sampleSize = ProminencedGroups.Count / sampleGroupLength;

        if ((sampleGroupLength > 1) && ((ProminencedGroups.Count % sampleGroupLength) > 0)) {
            sampleSize++;
        }

        WeightedGroup[] weightedGroups = new WeightedGroup[sampleSize];

		float totalWeight = 0;

		int index = 0;
        int sampleIndex = 0;
        int nextGroupToPick = GetNextLocalRandomInt(rngOffset++, sampleGroupLength);

		foreach (CellGroup group in ProminencedGroups.Values)
        {
            bool skipGroup = false;

            if ((sampleGroupLength > 1) && (index != nextGroupToPick))
                skipGroup = true;

            index++;

            if (sampleGroupLength > 1)
            {
                if ((index % sampleGroupLength) == 0)
                {
                    int groupsRemaining = ProminencedGroups.Count - index;

                    if (groupsRemaining > sampleGroupLength)
                    {
                        nextGroupToPick = index + GetNextLocalRandomInt(rngOffset++, sampleGroupLength);
                    }
                    else if (groupsRemaining > 0)
                    {
                        nextGroupToPick = index + (GetNextLocalRandomInt(rngOffset++, sampleGroupLength) % groupsRemaining);
                    }
                }
            }

            if (skipGroup) continue;
            
            Profiler.BeginSample("GetRandomGroup - calculateGroupValue - " + calculateGroupValue.Method.Module.Name + ":" + calculateGroupValue.Method.Name);

            float weight = calculateGroupValue (group);

            Profiler.EndSample();

			if (weight < 0)
				throw new System.Exception ("calculateGroupValue method returned weight value less than zero: " + weight);

			totalWeight += weight;

			weightedGroups [sampleIndex] = new WeightedGroup (group, weight);

            sampleIndex++;
        }

		if (totalWeight < 0) {
		
			throw new System.Exception ("Total weight can't be less than zero: " + totalWeight);
		}

		if ((totalWeight == 0) && nullIfNoValidGroup) {
		
			return null;
		}

		return CollectionUtility.WeightedSelection (weightedGroups, totalWeight, GetNextLocalRandomFloat (rngOffset));
	}

	public Faction GetRandomFaction (int rngOffset, FactionValueCalculationDelegate calculateFactionValue, bool nullIfNoValidFaction = false) {

		WeightedFaction[] weightedFactions = new WeightedFaction[Factions.Count];

		float totalWeight = 0;

		int index = 0;
		foreach (Faction faction in Factions.Values) {

			float weight = calculateFactionValue (faction);

			if (weight < 0)
				throw new System.Exception ("calculateFactionValue method returned weight value less than zero: " + weight);

			totalWeight += weight;

			weightedFactions [index] = new WeightedFaction (faction, weight);
			index++;
		}

		if (totalWeight < 0) {

			throw new System.Exception ("Total weight can't be less than zero: " + totalWeight);
		}

		if ((totalWeight == 0) && nullIfNoValidFaction) {

			return null;
		}

		return CollectionUtility.WeightedSelection (weightedFactions, totalWeight, GetNextLocalRandomFloat (rngOffset));
	}

	public PolityContact GetRandomPolityContact (int rngOffset, PolityContactValueCalculationDelegate calculateContactValue, bool nullIfNoValidContact = false)
    {
        WeightedPolityContact[] weightedContacts = new WeightedPolityContact[Contacts.Count];

		float totalWeight = 0;

		int index = 0;
        foreach (PolityContact contact in Contacts.Values)
        {
			float weight = calculateContactValue (contact);

			if (weight < 0)
				throw new System.Exception ("calculateContactValue method returned weight value less than zero: " + weight);

			totalWeight += weight;

			weightedContacts [index] = new WeightedPolityContact (contact, weight);
			index++;
        }

        float selectionValue = GetNextLocalRandomFloat(rngOffset);

#if DEBUG
        if (Manager.RegisterDebugEvent != null)
        {
            if (Id == Manager.TracingData.PolityId)
            {
                string contactWeights = "";

                foreach (WeightedPolityContact wc in weightedContacts)
                {
                    contactWeights += "\n\tPolity.Id: " + wc.Value.Id + ", weight: " + wc.Weight;
                }

                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                "Polity:GetRandomPolityContact - Polity.Id:" + Id,
                "selectionValue: " + selectionValue +
                ", Contact Weights: " + contactWeights +
                "");

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        if (totalWeight < 0) {

			throw new System.Exception ("Total weight can't be less than zero: " + totalWeight);
		}

		if ((totalWeight == 0) && nullIfNoValidContact) {

			return null;
		}

		return CollectionUtility.WeightedSelection (weightedContacts, totalWeight, selectionValue);
	}

	protected abstract void GenerateName ();

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

	public float GetPreferenceValue (string id) {

		CulturalPreference preference = Culture.GetPreference (id);

		if (preference != null)
			return preference.Value; 

		return 0;
	}

	public float CalculateContactStrength (Polity polity)
    {
        if (!Contacts.ContainsKey(polity.Id))
        {
			return 0;
		}

        return CalculateContactStrength(Contacts[polity.Id]);
	}

	public float CalculateContactStrength (PolityContact contact) {

		int contacGroupCount = contact.Polity.ProminencedGroups.Count;

		float minGroupCount = Mathf.Min(contacGroupCount, ProminencedGroups.Count);

		float countFactor = contact.GroupCount / minGroupCount;

		return countFactor;
	}

	public void MergePolity (Polity polity)
    {
		World.AddPolityToRemove (polity);
		World.AddPolityToUpdate (this);

		float polPopulation = Mathf.Floor (polity.TotalPopulation);
        
#if DEBUG
        World.PolityMergeCount++;
#endif

		if (polPopulation <= 0)
        {
            Debug.LogWarning("Merged polity with 0 or less population. this.Id:" + Id + ", polity.Id:" + polity.Id);

			return;
		}

		float localPopulation = Mathf.Floor (TotalPopulation);

		float populationFactor = polPopulation / localPopulation;

		List<Faction> factionsToMove = new List<Faction> (polity.GetFactions ());
	
		foreach (Faction faction in factionsToMove) {
		
			faction.ChangePolity (this, faction.Influence * populationFactor);

			faction.SetToUpdate ();
		}

		foreach (CellGroup group in polity.ProminencedGroups.Values) {
		
			float ppValue = group.GetPolityProminenceValue (polity);
			float localPpValue = group.GetPolityProminenceValue (this);

			group.SetPolityProminence (polity, 0);
			group.SetPolityProminence (this, localPpValue + ppValue);

			World.AddGroupToUpdate (group);
		}
	}

	public float CalculateAdministrativeLoad () {

		float socialOrganizationValue = 0;

		CulturalKnowledge socialOrganizationKnowledge = Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge != null)
			socialOrganizationValue = socialOrganizationKnowledge.Value;

		if (socialOrganizationValue < 0) {

			return Mathf.Infinity;
		}

		float administrativeLoad = TotalAdministrativeCost / socialOrganizationValue;

		administrativeLoad = Mathf.Pow (administrativeLoad, 2);

		if (administrativeLoad < 0) {

			Debug.LogWarning ("administrativeLoad less than 0: " + administrativeLoad);

			return Mathf.Infinity;
		}

		return administrativeLoad;
	}
}
