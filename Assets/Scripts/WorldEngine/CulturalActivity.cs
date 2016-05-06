using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalActivityInfo {

	[XmlAttribute]
	public string Id;
	
	[XmlAttribute]
	public string Name;
	
	public CulturalActivityInfo () {
	}
	
	public CulturalActivityInfo (string id, string name) {
		
		Id = id;
		
		Name = name;
	}
	
	public CulturalActivityInfo (CulturalActivityInfo baseInfo) {
		
		Id = baseInfo.Id;
		
		Name = baseInfo.Name;
	}
}

public class CulturalActivity : CulturalActivityInfo {

	[XmlAttribute]
	public float Value;

	[XmlAttribute]
	public float Contribution = 0;

	public CulturalActivity () {
	}

	public CulturalActivity (string id, string name, float value, float contribution) : base (id, name) {

		Value = value;
		Contribution = contribution;
	}

	public CulturalActivity (CulturalActivity baseActivity) : base (baseActivity) {

		Value = baseActivity.Value;
		Contribution = baseActivity.Contribution;
	}
}

public class CellCulturalActivity : CulturalActivity {

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;

	public const string ForagingActivityId = "ForagingActivity";
	public const string FarmingActivityId = "FarmingActivity";

	public const string ForagingActivityName = "Foraging";
	public const string FarmingActivityName = "Farming";

	[XmlIgnore]
	public CellGroup Group;

	public CellCulturalActivity () {
	}

	private CellCulturalActivity (CellGroup group, string id, string name, float value = 0, float contribution = 0) : base (id, name, value, contribution) {

		Group = group;
	}

	public static CellCulturalActivity CreateCellInstance (CellGroup group, CulturalActivity baseActivity) {
	
		return new CellCulturalActivity (group, baseActivity.Id, baseActivity.Name);
	}

	public static CellCulturalActivity CreateForagingActivity (CellGroup group, float value = 0, float contribution = 0) {
	
		return new CellCulturalActivity (group, ForagingActivityId, ForagingActivityName, value, contribution);
	}

	public static CellCulturalActivity CreateFarmingActivity (CellGroup group, float value = 0, float contribution = 0) {

		return new CellCulturalActivity (group, FarmingActivityId, FarmingActivityName, value, contribution);
	}

	public CellCulturalActivity GenerateCopy (CellGroup targetGroup) {

		return new CellCulturalActivity (targetGroup, Id, Name, Value, 0);
	}

	public void Merge (CellCulturalActivity activity, float percentage) {
	
		Value = Value * (1f - percentage) + activity.Value * percentage;
	}
	
	public void ModifyValue (float percentage) {
		
		Value *= percentage;
	}

	public void Update (int timeSpan) {

		TerrainCell groupCell = Group.Cell;

		float changeSpeedFactor = 0.001f;

		float randomModifier = groupCell.GetNextLocalRandomFloat ();
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

		Value = (Value * (1 - timeEffect)) + (targetValue * timeEffect);

		Value = Mathf.Clamp01 (Value);
	}

	public void PolityCulturalInfluence (CulturalActivity polityActivity, PolityInfluence polityInfluence, int timeSpan) {

		float targetValue = polityActivity.Value;
		float influenceEffect = polityInfluence.Value;

		TerrainCell groupCell = Group.Cell;

		float randomEffect = groupCell.GetNextLocalRandomFloat ();

		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		float change = (targetValue - Value) * influenceEffect * timeEffect * randomEffect;

		Value += change;

		Value = Mathf.Clamp01 (Value);
	}
}
