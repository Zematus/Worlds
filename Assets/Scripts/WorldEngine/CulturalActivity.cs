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

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;

	public const string ForagingActivityId = "ForagingActivity";
	public const string FarmingActivityId = "FarmingActivity";

	public const string ForagingActivityName = "Foraging";
	public const string FarmingActivityName = "Farming";
	
	[XmlAttribute]
	public float Value;

	[XmlAttribute]
	public float Contribution = 0;

	[XmlIgnore]
	public CellGroup Group;

	public CulturalActivity () {
	}

	private CulturalActivity (CellGroup group, string id, string name, float value) : base (id, name) {

		Group = group;
		Value = value;
	}

	public static CulturalActivity CreateForagingActivity (CellGroup group, float value = 0) {
	
		return new CulturalActivity (group, ForagingActivityId, ForagingActivityName, value);
	}

	public static CulturalActivity CreateFarmingActivity (CellGroup group, float value = 0) {

		return new CulturalActivity (group, FarmingActivityId, FarmingActivityName, value);
	}

	public CulturalActivity GenerateCopy (CellGroup targetGroup) {

		return new CulturalActivity (targetGroup, Id, Name, Value);
	}

	public void Merge (CulturalActivity activity, float percentage) {
	
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
}
