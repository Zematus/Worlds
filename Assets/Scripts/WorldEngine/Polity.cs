using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PolityInfluence {

	[XmlAttribute]
	public long PolityId;
	[XmlAttribute]
	public float Value;

	[XmlIgnore]
	public Polity Polity;

	public PolityInfluence () {

	}

	public PolityInfluence (Polity polity, float value) {
	
		PolityId = polity.Id;
		Polity = polity;
		Value = MathUtility.RoundToSixDecimals (value);
	}
}

public abstract class Polity : ISynchronizable {

	public const float TimeEffectConstant = CellGroup.GenerationTime * 15000;

	public const float MinPolityInfluence = 0.001f;

	[XmlAttribute]
	public long Id;

	[XmlAttribute]
	public long CoreGroupId;

	[XmlAttribute]
	public float TotalGroupInfluenceValue = 0;

	[XmlAttribute]
	public float TotalPopulation = 0;

	public Name Name;

	public List<long> InfluencedGroupIds;

	public Territory Territory;

	public PolityCulture Culture;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public CellGroup CoreGroup;

	[XmlIgnore]
	public Dictionary<long, CellGroup> InfluencedGroups = new Dictionary<long, CellGroup> ();

	private Dictionary<long, float> _influencedPopPerGroup = new Dictionary<long, float> ();

	#if DEBUG
	private bool _populationCensusUpdated = false;
	#endif

	private bool _coreGroupIsValid = true;

	public Polity () {
	
	}

	public Polity (CellGroup coreGroup, float coreGroupInfluenceValue) {

		World = coreGroup.World;

		Territory = new Territory (this);

		Id = World.GeneratePolityId ();

		CoreGroup = coreGroup;
		CoreGroupId = coreGroup.Id;

		Culture = new PolityCulture (this);

		coreGroup.SetPolityInfluenceValue (this, coreGroupInfluenceValue);

		GenerateName ();
	}

	public void Destroy () {
		
		World.RemovePolity (this);

		foreach (CellGroup group in InfluencedGroups.Values) {

			group.RemovePolityInfluence (this);
		}
	}

	public void SetCoreGroup (CellGroup group) {

		if (!InfluencedGroups.ContainsKey (group.Id))
			throw new System.Exception ("Group is not part of polity's influenced groups");

		CoreGroup = group;
		CoreGroupId = group.Id;
	}

	public void Update () {

		if (InfluencedGroups.Count <= 0) {
		
			World.AddPolityToRemove (this);

			return;
		}

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

				return;
			}

			_coreGroupIsValid = true;
		}

		#if DEBUG
		_populationCensusUpdated = false;
		#endif
	}

	public float GetNextRandomFloat () {

		return CoreGroup.GetNextLocalRandomFloat ();
	}

	public abstract void UpdateInternal ();

	public void RunPopulationCensus () {

		TotalPopulation = 0;

		_influencedPopPerGroup.Clear ();
	
		foreach (CellGroup group in InfluencedGroups.Values) {

			float influencedPop = group.Population * group.GetPolityInfluenceValue (this);

			TotalPopulation += influencedPop;

			_influencedPopPerGroup.Add (group.Id, influencedPop);
		}
	}

	protected CellGroup GetGroupWithMostInfluencedPop () {

		#if DEBUG
		if (!_populationCensusUpdated)
			Debug.LogWarning ("This function should only be called within polity updates after executing RunPopulationCensus");
		#endif

		CellGroup groupWithMostInfluencedPop = null;
		float maxInfluencedGroupPopulation = 0;
	
		foreach (KeyValuePair<long, float> pair in _influencedPopPerGroup) {
		
			if (maxInfluencedGroupPopulation < pair.Value) {

				maxInfluencedGroupPopulation = pair.Value;

				groupWithMostInfluencedPop = InfluencedGroups [pair.Key];
			}
		}

		return groupWithMostInfluencedPop;
	}

	public void AddInfluencedGroup (CellGroup group) {

		#if DEBUG
		if (!group.RunningFunction_SetPolityInfluence)
			Debug.LogWarning ("AddInfluencedGroup should only be called withn SetPolityInfluence");
		#endif
	
		InfluencedGroups.Add (group.Id, group);

		//TODO: Remove line
//		Territory.AddCell (group.Cell);
	}

	public void RemoveInfluencedGroup (CellGroup group) {

		#if DEBUG
		if (!group.RunningFunction_SetPolityInfluence)
			Debug.LogWarning ("RemoveInfluencedGroup should only be called withn SetPolityInfluence");
		#endif

		InfluencedGroups.Remove (group.Id);

		//TODO: Remove line
//		Territory.RemoveCell (group.Cell);

		if (group == CoreGroup) {

			_coreGroupIsValid = false;
		}
	}

	public virtual void Synchronize () {

		Culture.Synchronize ();

		Territory.Synchronize ();

		InfluencedGroupIds = new List<long> (InfluencedGroups.Keys);

		Name.Synchronize ();
	}

	public virtual void FinalizeLoad () {

		Name.World = World;
		Name.FinalizeLoad ();

		CoreGroup = World.GetGroup (CoreGroupId);

		if (CoreGroup == null) {
			throw new System.Exception ("Missing Group with Id " + CoreGroupId);
		}

		foreach (int id in InfluencedGroupIds) {

			CellGroup group = World.GetGroup (id);

			if (group == null) {
				throw new System.Exception ("Missing Group with Id " + id);
			}

			InfluencedGroups.Add (group.Id, group);
		}

		Territory.World = World;
		Territory.Polity = this;
		Territory.FinalizeLoad ();

		Culture.World = World;
		Culture.Polity = this;
		Culture.FinalizeLoad ();
	}

	public virtual float MigrationValue (TerrainCell targetCell, float sourceValue)
	{
		if (sourceValue <= 0)
			return 0;

		CellGroup targetGroup = targetCell.Group;

		float groupTotalInfluenceValue = 0;

		float socialOrgFactor = 0;

		if (targetGroup != null) {

			CulturalKnowledge socialOrgKnowledge = targetGroup.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

			socialOrgFactor = Mathf.Clamp01 (socialOrgKnowledge.Value / SocialOrganizationKnowledge.MinKnowledgeValueForTribalism);
			socialOrgFactor = 1 - Mathf.Pow (1 - socialOrgFactor, 2);

			groupTotalInfluenceValue = targetGroup.TotalPolityInfluenceValue;
		}

		float sourceValueFactor = 0.05f + (sourceValue * 0.95f);

		float influenceFactor = socialOrgFactor * sourceValue / (groupTotalInfluenceValue + sourceValueFactor);

		return Mathf.Clamp01 (influenceFactor);
	}

	public virtual void MergingEffects (CellGroup targetGroup, float sourceValue, float percentOfTarget) {

		foreach (PolityInfluence pInfluence in targetGroup.GetPolityInfluences ()) {

			float influenceValue = pInfluence.Value;

			float newInfluenceValue = influenceValue * (1 - percentOfTarget);

			targetGroup.SetPolityInfluenceValue (pInfluence.Polity, newInfluenceValue);
		}

		float currentValue = targetGroup.GetPolityInfluenceValue (this);

		float newValue = currentValue + (sourceValue * percentOfTarget);

		#if DEBUG
		if (targetGroup.Cell.IsSelected) {

			bool debug = true;
		}
		#endif

		targetGroup.SetPolityInfluenceValue (this, newValue);
	}

	public virtual void UpdateEffects (CellGroup group, float influenceValue, int timeSpan) {

		if (group.Culture.GetDiscovery (TribalismDiscovery.TribalismDiscoveryId) == null) {

			group.SetPolityInfluenceValue (this, 0);

			return;
		}

		TerrainCell groupCell = group.Cell;

		float maxInfluenceValue = 1 - group.TotalPolityInfluenceValue + influenceValue;

		float maxTargetValue = maxInfluenceValue;
		float minTargetValue = -0.2f * maxInfluenceValue;

		float randomModifier = groupCell.GetNextLocalRandomFloat ();
		float randomFactor = 2 * randomModifier - 1f;
		float targetValue = 0;

		if (randomFactor > 0) {
			targetValue = influenceValue + (maxTargetValue - influenceValue) * randomFactor;
		} else {
			targetValue = influenceValue - (minTargetValue - influenceValue) * randomFactor;
		}

		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		influenceValue = (influenceValue * (1 - timeEffect)) + (targetValue * timeEffect);

		group.SetPolityInfluenceValue (this, influenceValue);
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
