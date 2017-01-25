using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityInfluence {

	[XmlAttribute]
	public long PolityId;
	[XmlAttribute("Val")]
	public float Value;
	[XmlAttribute("Dist")]
	public float CoreDistance;
	[XmlAttribute("Cost")]
	public float AdiministrativeCost;

	[XmlIgnore]
	public float NewValue;
	[XmlIgnore]
	public float NewCoreDistance;

	[XmlIgnore]
	public Polity Polity;

	public PolityInfluence () {

	}

	public PolityInfluence (Polity polity, float value) {
	
		PolityId = polity.Id;
		Polity = polity;
		Value = MathUtility.RoundToSixDecimals (value);
		NewValue = Value;

		AdiministrativeCost = 0;
	}

	public void PostUpdate () {

		Value = NewValue;
		CoreDistance = NewCoreDistance;
	}
}

public abstract class Polity : ISynchronizable {

	public const float TimeEffectConstant = CellGroup.GenerationTime * 2500;

	public const float MinPolityInfluence = 0.001f;

	[XmlAttribute("Type")]
	public string Type;

	[XmlAttribute]
	public long Id;

	[XmlAttribute("CGrpId")]
	public long CoreGroupId;

	[XmlAttribute("TotalAdmCost")]
	public float TotalAdministrativeCost = 0;

	[XmlAttribute("TotalPop")]
	public float TotalPopulation = 0;

	[XmlAttribute("FctnCount")]
	public int FactionCount { get; private set; }

	[XmlAttribute("StilPres")]
	public bool StillPresent = true;

	public Name Name;

	public List<long> InfluencedGroupIds;

	public Territory Territory;

	public PolityCulture Culture;

	[XmlArrayItem (Type = typeof(Clan))]
	public List<Faction> Factions;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public CellGroup CoreGroup;

	[XmlIgnore]
	public bool WillBeUpdated;

	[XmlIgnore]
	public Dictionary<long, CellGroup> InfluencedGroups = new Dictionary<long, CellGroup> ();

	protected class WeightedGroup : CollectionUtility.ElementWeightPair<CellGroup> {

		public WeightedGroup (CellGroup group, float weight) : base (group, weight) {

		}
	}

	private Dictionary<long, WeightedGroup> _influencedPopPerGroup = new Dictionary<long, WeightedGroup> ();

	private Dictionary<long, Faction> _factions = new Dictionary<long, Faction> ();

	#if DEBUG
	private bool _populationCensusUpdated = false;
	#endif

	private bool _coreGroupIsValid = true;

	private bool _fullyInitialized = false;

	public Polity () {
	
	}

	public Polity (string type, CellGroup coreGroup, float coreGroupInfluenceValue) {

		Type = type;

		World = coreGroup.World;

		Territory = new Territory (this);

		CoreGroup = coreGroup;
		CoreGroupId = coreGroup.Id;

		Id = coreGroup.GenerateUniqueIdentifier ();

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
//					", Polity.TotalGroupInfluenceValue: " + TotalGroupInfluenceValue + 
//					", coreGroupInfluenceValue: " + coreGroupInfluenceValue + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		coreGroup.SetPolityInfluence (this, coreGroupInfluenceValue);

		World.AddGroupToUpdate (coreGroup);
	}

	public void FinishInitialization () {

		GenerateName ();

		FinishInitializationInternal ();

		_fullyInitialized = true;
	}

	protected abstract void FinishInitializationInternal ();

	public void Destroy () {

		List<Faction> factions = new List<Faction> (_factions.Values);

		foreach (Faction faction in factions) {
		
			faction.Destroy ();
		}

		foreach (CellGroup group in InfluencedGroups.Values) {

			group.RemovePolityInfluence (this);
		}
		
		World.RemovePolity (this);

		StillPresent = false;
	}

	public void AddFaction (Faction faction) {

		_factions.Add (faction.Id, faction);

		FactionCount++;
	}

	public void RemoveFaction (Faction faction) {

		_factions.Remove (faction.Id);

		FactionCount--;
	}

	public Faction GetFaction (long id) {

		Faction faction;

		_factions.TryGetValue (id, out faction);

		return faction;
	}

	public IEnumerable<Faction> GetFactions () {
	
		return _factions.Values;
	}

	public void SetCoreGroup (CellGroup group) {

		if (!InfluencedGroups.ContainsKey (group.Id))
			throw new System.Exception ("Group is not part of polity's influenced groups");

		CoreGroup = group;
		CoreGroupId = group.Id;
	}

	public void Update () {

		if (!_fullyInitialized) {
		
			FinishInitialization ();
		}

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			Manager.RegisterDebugEvent ("DebugMessage", 
//				"Update - Polity:" + Id + 
//				", CurrentDate: " + World.CurrentDate + 
//				", InfluencedGroups.Count: " + InfluencedGroups.Count + 
//				", TotalGroupInfluenceValue: " + TotalGroupInfluenceValue + 
//				"");
//		}
//		#endif

		WillBeUpdated = false;

		if (InfluencedGroups.Count <= 0) {
		
			World.AddPolityToRemove (this);

			return;
		}

//		Profiler.BeginSample ("Polity Update");

		RunPopulationCensus ();

		#if DEBUG
		_populationCensusUpdated = true;
		#endif

		UpdateInternal ();
	
		Culture.Update ();

		if (!_coreGroupIsValid) {

			if (!TryRelocateCore ()) {

				// We were unable to find a new core for the polity
				World.AddPolityToRemove (this);

				#if DEBUG
				_populationCensusUpdated = false;
				#endif

//				Profiler.EndSample ();

				return;
			}

			_coreGroupIsValid = true;
		}

		#if DEBUG
		_populationCensusUpdated = false;
		#endif

//		Profiler.EndSample ();
	}

	protected abstract void UpdateInternal ();

	public void RunPopulationCensus () {

		TotalPopulation = 0;

		_influencedPopPerGroup.Clear ();
	
		foreach (CellGroup group in InfluencedGroups.Values) {

			float influencedPop = group.Population * group.GetPolityInfluenceValue (this);

			TotalPopulation += influencedPop;

			_influencedPopPerGroup.Add (group.Id, new WeightedGroup (group, influencedPop));
		}
	}

	protected CellGroup GetGroupWithMostInfluencedPop () {

		#if DEBUG
		if (!_populationCensusUpdated)
			Debug.LogWarning ("This function should only be called within polity updates after executing RunPopulationCensus");
		#endif

		CellGroup groupWithMostInfluencedPop = null;
		float maxInfluencedGroupPopulation = 0;
	
		foreach (KeyValuePair<long, WeightedGroup> pair in _influencedPopPerGroup) {
		
			if (maxInfluencedGroupPopulation < pair.Value.Weight) {

				maxInfluencedGroupPopulation = pair.Value.Weight;

				groupWithMostInfluencedPop = pair.Value.Value;
			}
		}

		return groupWithMostInfluencedPop;
	}

	public void AddInfluencedGroup (CellGroup group) {
	
		InfluencedGroups.Add (group.Id, group);
	}

	public void RemoveInfluencedGroup (CellGroup group) {

		InfluencedGroups.Remove (group.Id);

		if (group == CoreGroup) {

			_coreGroupIsValid = false;
		}
	}

	public virtual void Synchronize () {

		Culture.Synchronize ();

		Territory.Synchronize ();

		InfluencedGroupIds = new List<long> (InfluencedGroups.Keys);

		Factions = new List<Faction> (_factions.Values);

		foreach (Faction f in Factions) {

			f.Synchronize ();
		}

		Name.Synchronize ();
	}

	public virtual void FinalizeLoad () {

		Name.World = World;
		Name.FinalizeLoad ();

		CoreGroup = World.GetGroup (CoreGroupId);

		if (CoreGroup == null) {
			throw new System.Exception ("Missing Group with Id " + CoreGroupId);
		}

		foreach (long id in InfluencedGroupIds) {

			CellGroup group = World.GetGroup (id);

			if (group == null) {
				throw new System.Exception ("Missing Group with Id " + id);
			}

			InfluencedGroups.Add (group.Id, group);
		}

		// all factions should be stored in the dictionary before finalizing load for each one
		Factions.ForEach (f => {

			f.World = World;

			_factions.Add (f.Id, f);
		});

		Factions.ForEach (f => {

			f.FinalizeLoad ();
		});

		Territory.World = World;
		Territory.Polity = this;
		Territory.FinalizeLoad ();

		Culture.World = World;
		Culture.Polity = this;
		Culture.FinalizeLoad ();
	}

//	public virtual float CalculateCellMigrationValue (CellGroup sourceGroup, TerrainCell targetCell, float sourceValue)
//	{
//		if (sourceValue <= 0)
//			return 0;
//
//		float sourceGroupTotalPolityInfluenceValue = sourceGroup.TotalPolityInfluenceValue;
//
//		CellGroup targetGroup = targetCell.Group;
//
//		if (targetGroup == null) {
//			return sourceValue / sourceGroupTotalPolityInfluenceValue;
//		}
//
//		float targetGroupTotalPolityInfluenceValue = targetGroup.TotalPolityInfluenceValue;
//
//		if (sourceGroupTotalPolityInfluenceValue <= 0) {
//		
//			throw new System.Exception ("sourceGroup.TotalPolityInfluenceValue equal or less than 0: " + sourceGroupTotalPolityInfluenceValue);
//		}
//
//		float influenceFactor = sourceValue / (targetGroupTotalPolityInfluenceValue + sourceGroupTotalPolityInfluenceValue);
//
//		influenceFactor = MathUtility.RoundToSixDecimals (influenceFactor);
//
////		#if DEBUG
////		if (Manager.RegisterDebugEvent != null) {
////			if (sourceGroup.Id == Manager.TracingData.GroupId) {
////				if (Id == Manager.TracingData.PolityId) {
////					if ((targetCell.Longitude == Manager.TracingData.Longitude) && (targetCell.Latitude == Manager.TracingData.Latitude)) {
////						string sourceGroupId = "Id:" + sourceGroup.Id + "|Long:" + sourceGroup.Longitude + "|Lat:" + sourceGroup.Latitude;
////						string targetLocation = "Long:" + targetCell.Longitude + "|Lat:" + targetCell.Latitude;
////
////						if (targetGroup != null) {
////							targetLocation = "Id:" + targetGroup.Id + "|Long:" + targetGroup.Longitude + "|Lat:" + targetGroup.Latitude;
////						}
////
////						SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
////							"MigrationValue - Group: " + sourceGroupId + 
////							"Polity Id: " + Id,
////							"CurrentDate: " + World.CurrentDate + 
////							", targetLocation: " + targetLocation + 
////							", sourceValue: " + sourceValue.ToString("F7") + 
////							", socialOrgFactor: " + socialOrgFactor.ToString("F7") + 
////							", groupTotalInfluenceValue: " + groupTotalInfluenceValue.ToString("F7") + 
////							", sourceValueFactor: " + sourceValueFactor.ToString("F7") + 
////							", influenceFactor: " + influenceFactor.ToString("F7") + 
////							"");
////
////						Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
////					}
////				}
////			}
////		}
////		#endif
//
//		return Mathf.Clamp01 (influenceFactor);
//	}

	public abstract float CalculateGroupInfluenceExpansionValue (CellGroup sourceGroup, CellGroup targetGroup, float sourceValue);

	public virtual void GroupUpdateEffects (CellGroup group, float influenceValue, float totalPolityInfluenceValue, int timeSpan) {

		if (group.Culture.GetFoundDiscoveryOrToFind (TribalismDiscovery.TribalismDiscoveryId) == null) {

			group.SetPolityInfluence (this, 0);

			return;
		}

		TerrainCell groupCell = group.Cell;

		float maxTargetValue = 1f;
		float minTargetValue = 0.8f * totalPolityInfluenceValue;

		float randomModifier = groupCell.GetNextLocalRandomFloat (RngOffsets.POLITY_UPDATE_EFFECTS + (int)Id);
		float targetValue = ((maxTargetValue - minTargetValue) * randomModifier) + minTargetValue;

		float scaledValue = (targetValue - influenceValue) * influenceValue / totalPolityInfluenceValue;
		targetValue = influenceValue + scaledValue;

		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		influenceValue = (influenceValue * (1 - timeEffect)) + (targetValue * timeEffect);

		influenceValue = Mathf.Clamp01 (influenceValue);

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
//					", groupTotalPolityInfluenceValue: " + groupTotalPolityInfluenceValue + 
//					", Polity.TotalGroupInfluenceValue: " + TotalGroupInfluenceValue + 
//					", unmodInflueceValue: " + unmodInflueceValue + 
//					", influenceValue: " + influenceValue + 
//					", group.LastUpdateDate: " + group.LastUpdateDate + 
//					"");
//
//				Manager.RegisterDebugEvent ("DebugMessage", debugMessage);
//			}
//		}
//		#endif

		group.SetPolityInfluence (this, influenceValue);
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

		float altitudeSurvivabilityFactor = 1 - (cell.Altitude / World.MaxPossibleAltitude);

		modifiedSurvivability = (modifiedSurvivability * (1 - cell.FarmlandPercentage)) + cell.FarmlandPercentage;

		foragingCapacity = modifiedForagingCapacity * (1 - cell.FarmlandPercentage);
		survivability = modifiedSurvivability * altitudeSurvivabilityFactor;

		if (survivability > 1) {
			throw new System.Exception ("Modified survivability greater than 1: " + survivability);
		}
	}

	// TODO: This function should be overriden in children
	public virtual bool TryRelocateCore () {

		CellGroup mostInfluencedPopGroup = GetGroupWithMostInfluencedPop ();

		if (mostInfluencedPopGroup == null) {

			return false;
		}

		SetCoreGroup (mostInfluencedPopGroup);

		return true;
	}

	protected abstract void GenerateName ();
}
