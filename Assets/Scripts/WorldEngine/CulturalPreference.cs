using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

// Cultural Preferences
// -- Elder Respect
// -- Authority Respect
// -- Family Cohesion

public class CulturalPreferenceInfo {

	[XmlAttribute]
	public string Id;
	
	[XmlAttribute]
	public string Name;

	[XmlAttribute("RO")]
	public int RngOffset;
	
	public CulturalPreferenceInfo () {
	}
	
	public CulturalPreferenceInfo (string id, string name, int rngOffset) {
		
		Id = id;
		Name = name;
		RngOffset = rngOffset;
	}
	
	public CulturalPreferenceInfo (CulturalPreferenceInfo basePreference) {
		
		Id = basePreference.Id;
		Name = basePreference.Name;
		RngOffset = basePreference.RngOffset;
	}
}

public class CulturalPreference : CulturalPreferenceInfo {

	[XmlAttribute]
	public float Value;

	[XmlAttribute]
	public float Contribution = 0;

	public CulturalPreference () {
	}

	public CulturalPreference (string id, string name, int rngOffset, float value, float contribution) : base (id, name, rngOffset) {

		Value = value;
		Contribution = contribution;
	}

	public CulturalPreference (CulturalPreference basePreference) : base (basePreference) {

		Value = basePreference.Value;
		Contribution = basePreference.Contribution;
	}
}

public class CellCulturalPreference : CulturalPreference {

	public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;

	public const string ForagingActivityId = "ForagingActivity";
	public const string FarmingActivityId = "FarmingActivity";

	public const string ForagingActivityName = "Foraging";
	public const string FarmingActivityName = "Farming";

	public const int ForagingActivityRandomOffset = 0;
	public const int FarmingActivityRandomOffset = 100;

	[XmlIgnore]
	public CellGroup Group;

	private float _newValue;

	public CellCulturalPreference () {
	}

	private CellCulturalPreference (CellGroup group, string id, string name, int rngOffset, float value = 0, float contribution = 0) : base (id, name, rngOffset, value, contribution) {

		Group = group;

		_newValue = value;
	}

	public static CellCulturalPreference CreateCellInstance (CellGroup group, CulturalPreference basePreference) {

		return CreateCellInstance (group, basePreference, basePreference.Value);
	}

	public static CellCulturalPreference CreateCellInstance (CellGroup group, CulturalPreference basePreference, float initialValue, float initialContribution = 0) {
	
		return new CellCulturalPreference (group, basePreference.Id, basePreference.Name, basePreference.RngOffset, initialValue, initialContribution);
	}

	public static CellCulturalPreference CreateForagingActivity (CellGroup group, float value = 0, float contribution = 0) {
	
		return new CellCulturalPreference (group, ForagingActivityId, ForagingActivityName, ForagingActivityRandomOffset, value, contribution);
	}

	public static CellCulturalPreference CreateFarmingActivity (CellGroup group, float value = 0, float contribution = 0) {

		return new CellCulturalPreference (group, FarmingActivityId, FarmingActivityName, FarmingActivityRandomOffset, value, contribution);
	}

	public void Merge (CulturalPreference preference, float percentage) {

		// _newvalue should have been set correctly either by the constructor or by the Update function
		_newValue = _newValue * (1f - percentage) + preference.Value * percentage;
	}

	// This method should be called only once after a Activity is copied from another source group
	public void ModifyValue (float percentage) {

		_newValue = Value * percentage;
	}

	public void Update (int timeSpan) {

		TerrainCell groupCell = Group.Cell;

		float changeSpeedFactor = 0.001f;

		float randomModifier = groupCell.GetNextLocalRandomFloat (RngOffsets.ACTIVITY_UPDATE + RngOffset);
		randomModifier = 1f - (randomModifier * 2f);
		float randomFactor = changeSpeedFactor * randomModifier;

		float maxTargetValue = 1f;
		float minTargetValue = 0f;
		float targetValue = 0;

		if (randomFactor > 0) {
			targetValue = Value + (maxTargetValue - Value) * randomFactor;
		} else {
			targetValue = Value - (minTargetValue - Value) * randomFactor;
		}

		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		_newValue = (Value * (1 - timeEffect)) + (targetValue * timeEffect);
	}

	public void PolityCulturalInfluence (CulturalPreference polityPreference, PolityInfluence polityInfluence, int timeSpan) {

		float targetValue = polityPreference.Value;
		float influenceEffect = polityInfluence.Value;

		TerrainCell groupCell = Group.Cell;

		float randomEffect = groupCell.GetNextLocalRandomFloat (RngOffsets.ACTIVITY_POLITY_INFLUENCE + RngOffset + (int)polityInfluence.PolityId);

		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		// _newvalue should have been set correctly either by the constructor or by the Update function
		float change = (targetValue - _newValue) * influenceEffect * timeEffect * randomEffect;

		_newValue = _newValue + change;
	}

	public void PostUpdate () {

		Value = Mathf.Clamp01 (_newValue);
	}
}
