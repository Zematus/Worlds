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
		Value = value;
	}
}

public abstract class Polity : Synchronizable {

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

	public List<long> InfluencedGroupIds;

	public Territory Territory = new Territory ();

	public PolityCulture Culture;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public CellGroup CoreGroup;

	[XmlIgnore]
	public Dictionary<long, CellGroup> InfluencedGroups = new Dictionary<long, CellGroup> ();

	private Dictionary<long, float> _influencedPopPerGroup = new Dictionary<long, float> ();

	private bool _updating = false;

	public Polity () {
	
	}

	public Polity (CellGroup coreGroup, float coreGroupInfluenceValue) {

		World = coreGroup.World;

		Id = World.GeneratePolityId ();

		coreGroup.SetPolityInfluenceValue (this, coreGroupInfluenceValue);

		SetCoreGroup (coreGroup);

		Culture = new PolityCulture (this);
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

		_updating = true;

		if (InfluencedGroups.Count <= 0) {
		
			World.AddPolityToRemove (this);

			return;
		}

		RunPopulationCensus ();

		UpdateInternal ();
	
		Culture.Update ();

		_updating = false;
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

		if (!_updating)
			Debug.LogWarning ("This function should only be called within polity updates after executing RunPopulationCensus");

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
	
		InfluencedGroups.Add (group.Id, group);

		Territory.AddCell (group.Cell);
	}

	public void RemoveInfluencedGroup (CellGroup group) {

		InfluencedGroups.Remove (group.Id);

		Territory.RemoveCell (group.Cell);
	}

	public virtual void Synchronize () {

		Culture.Synchronize ();

		InfluencedGroupIds = new List<long> (InfluencedGroups.Keys);
	}

	public virtual void FinalizeLoad () {

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

		float maxTargetValue = 1.0f;
		float minTargetValue = -0.2f;

		float randomModifier = groupCell.GetNextLocalRandomFloat ();
		float randomFactor = randomModifier - 0.5f;
		float targetValue = 0;

		if (randomFactor > 0) {
			targetValue = influenceValue + (maxTargetValue - influenceValue) * randomFactor;
		} else {
			targetValue = influenceValue - (minTargetValue - influenceValue) * randomFactor;
		}

		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		influenceValue = (influenceValue * (1 - timeEffect)) + (targetValue * timeEffect);

		influenceValue = Mathf.Clamp01 (influenceValue);

		group.SetPolityInfluenceValue (this, influenceValue);
	}
}

public class Territory {

	public List<WorldPosition> CellPositions = new List<WorldPosition> ();

	[XmlIgnore]
	public World World;

	private HashSet<TerrainCell> _cells = new HashSet<TerrainCell> ();

	public Territory () {
	
	}

	public Territory (World world) {

		World = world;
	}

	public bool AddCell (TerrainCell cell) {

		if (!_cells.Add (cell))
			return false;

		CellPositions.Add (cell.Position);

		cell.AddEncompassingTerritory (this);

		return true;
	}

	public bool RemoveCell (TerrainCell cell) {

		if (!_cells.Remove (cell))
			return false;

		CellPositions.Remove (cell.Position);

		cell.RemoveEncompassingTerritory (this);

		return true;
	}

	public void FinalizeLoad () {

		foreach (WorldPosition position in CellPositions) {

			TerrainCell cell = World.GetCell (position);

			if (cell == null) {
				throw new System.Exception ("Cell missing at position " + position.Longitude + "," + position.Latitude);
			}
		
			_cells.Add (cell);

			cell.AddEncompassingTerritory (this);
		}
	}
}
