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

public abstract class CulturalActivity : CulturalActivityInfo {
	
	[XmlAttribute]
	public float Value;

	[XmlIgnore]
	public CellGroup Group;
	
	public CulturalActivity () {
	}

	public CulturalActivity (CellGroup group, string id, string name, float value) : base (id, name) {

		Group = group;
		Value = value;
	}
	
	public CulturalActivity GenerateCopy (CellGroup targetGroup) {
		
		System.Type activityType = this.GetType ();
		
		System.Reflection.ConstructorInfo cInfo = activityType.GetConstructor (new System.Type[] {typeof(CellGroup), activityType});
		
		return cInfo.Invoke (new object[] {targetGroup, this}) as CulturalActivity;
	}

	public void Merge (CulturalActivity activity, float percentage) {
	
		Value = Value * (1f - percentage) + activity.Value * percentage;
	}
	
	public void ModifyValue (float percentage) {
		
		Value *= percentage;
	}

	public virtual void FinalizeLoad () {

	}

	public abstract void Update (int timeSpan);

	protected void UpdateValue (int timeSpan, float timeEffectFactor) {

		TerrainCell groupCell = Group.Cell;

		float randomModifier = groupCell.GetNextLocalRandomFloat ();
		randomModifier = 1f - (randomModifier * 2f);
		float randomFactor = 0.2f * randomModifier;

		float maxTargetValue = 1f;
		float minTargetValue = 0f;
		float targetValue = 0;

		if (randomFactor > 0) {
			targetValue = Value + (maxTargetValue - Value) * randomFactor;
		} else {
			targetValue = Value - (minTargetValue - Value) * randomFactor;
		}

		float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

		Value = (Value * (1 - timeEffect)) + (targetValue * timeEffect);

		Value = Mathf.Clamp01 (Value);
	}
}

public class ForagingActivity : CulturalActivity {

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;

	public const string ForagingActivityId = "ForagingActivity";

	public const string ForagingActivityName = "Foraging";

	public ForagingActivity () {

	}

	public ForagingActivity (CellGroup group, float value = 0f) : base (group, ForagingActivityId, ForagingActivityName, value) {

	}

	public ForagingActivity (CellGroup group, SeafaringSkill baseSkill) : base (group, baseSkill.Id, baseSkill.Name, baseSkill.Value) {

	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();
	}

	public override void Update (int timeSpan) {

		UpdateValue (timeSpan, TimeEffectConstant);
	}
}

public class FarmingActivity : CulturalActivity {

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;

	public const string FarmingActivityId = "FarmingActivity";

	public const string FarmingActivityName = "Farming";

	public FarmingActivity () {

	}

	public FarmingActivity (CellGroup group, float value = 0f) : base (group, FarmingActivityId, FarmingActivityName, value) {

	}

	public FarmingActivity (CellGroup group, SeafaringSkill baseSkill) : base (group, baseSkill.Id, baseSkill.Name, baseSkill.Value) {

	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();
	}

	public override void Update (int timeSpan) {

		UpdateValue (timeSpan, TimeEffectConstant);
	}
}
