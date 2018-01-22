using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

// Cultural Preferences
// -- Authority
// -- Cohesion

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

	public CulturalPreference () {
	}

	public CulturalPreference (string id, string name, int rngOffset, float value) : base (id, name, rngOffset) {

		Value = value;
	}

	public CulturalPreference (CulturalPreference basePreference) : base (basePreference) {

		Value = basePreference.Value;
	}
}

public class CellCulturalPreference : CulturalPreference {

	public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;

	public const string AuthorityPreferenceId = "AuthorityPreference";
	public const string CohesivenessPreferenceId = "CohesivenessPreference";

	public const string AuthorityPreferenceName = "Authority";
	public const string CohesivenessPreferenceName = "Cohesiveness";

	public const int AuthorityPreferenceRandomOffset = 0;
	public const int CohesivenessPreferenceRandomOffset = 100;

	[XmlIgnore]
	public CellGroup Group;

	private const float MaxChangeDelta = 0.2f;

	private float _newValue;

	public CellCulturalPreference () {
	}

	private CellCulturalPreference (CellGroup group, string id, string name, int rngOffset, float value = 0) : base (id, name, rngOffset, value) {

		Group = group;

		_newValue = value;
	}

	public static CellCulturalPreference CreateCellInstance (CellGroup group, CulturalPreference basePreference) {

		return CreateCellInstance (group, basePreference, basePreference.Value);
	}

	public static CellCulturalPreference CreateCellInstance (CellGroup group, CulturalPreference basePreference, float initialValue) {
	
		return new CellCulturalPreference (group, basePreference.Id, basePreference.Name, basePreference.RngOffset, initialValue);
	}

	public static CellCulturalPreference CreateAuthorityPreference (CellGroup group, float value = 0) {
	
		return new CellCulturalPreference (group, AuthorityPreferenceId, AuthorityPreferenceName, AuthorityPreferenceRandomOffset, value);
	}

	public static CellCulturalPreference CreateCohesivenessPreference (CellGroup group, float value = 0) {

		return new CellCulturalPreference (group, CohesivenessPreferenceId, CohesivenessPreferenceName, CohesivenessPreferenceRandomOffset, value);
	}

	public void Merge (CulturalPreference preference, float percentage) {

		// _newvalue should have been set correctly either by the constructor or by the Update function
		_newValue = _newValue * (1f - percentage) + preference.Value * percentage;
	}

	// This method should be called only once after a Cultural Value is copied from another source group
	public void ModifyValue (float percentage) {

		_newValue = Value * percentage;
	}

	public void Update (int timeSpan) {

		TerrainCell groupCell = Group.Cell;

		float randomModifier = groupCell.GetNextLocalRandomFloat (RngOffsets.PREFERENCE_UPDATE + RngOffset);
		randomModifier = 1f - (randomModifier * 2f);
		float randomFactor = MaxChangeDelta * randomModifier;

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

		float randomEffect = groupCell.GetNextLocalRandomFloat (RngOffsets.PREFERENCE_POLITY_INFLUENCE + RngOffset + (int)polityInfluence.PolityId);

		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		// _newvalue should have been set correctly either by the constructor or by the Update function
		float change = (targetValue - _newValue) * influenceEffect * timeEffect * randomEffect;

		_newValue = _newValue + change;
	}

	public void PostUpdate () {

		Value = Mathf.Clamp01 (_newValue);
	}
}