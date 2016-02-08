using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalSkillInfo {

	[XmlAttribute]
	public string Id;
	
	[XmlAttribute]
	public string Name;
	
	public CulturalSkillInfo () {
	}
	
	public CulturalSkillInfo (string id, string name) {
		
		Id = id;
		
		Name = name;
	}
	
	public CulturalSkillInfo (CulturalSkillInfo baseInfo) {
		
		Id = baseInfo.Id;
		
		Name = baseInfo.Name;
	}
}

public abstract class CulturalSkill : CulturalSkillInfo {
	
	[XmlAttribute]
	public float Value;
	
	[XmlAttribute]
	public float AdaptationLevel;

	[XmlIgnore]
	public CellGroup Group;
	
	public CulturalSkill () {
	}

	public CulturalSkill (CellGroup group, string id, string name, float value) : base (id, name) {

		Group = group;
		Value = value;
	}
	
	public CulturalSkill GenerateCopy (CellGroup targetGroup) {
		
		System.Type skillType = this.GetType ();
		
		System.Reflection.ConstructorInfo cInfo = skillType.GetConstructor (new System.Type[] {typeof(CellGroup), skillType});
		
		return cInfo.Invoke (new object[] {targetGroup, this}) as CulturalSkill;
	}

	public void Merge (CulturalSkill skill, float percentage) {
	
		Value = Value * (1f - percentage) + skill.Value * percentage;
	}
	
	public void ModifyValue (float percentage) {
		
		Value *= percentage;
	}

	public virtual void FinalizeLoad () {

	}

	public abstract void Update (int timeSpan);
	public abstract void UpdateAdaptationLevel ();
}

public class BiomeSurvivalSkill : CulturalSkill {

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;
	
	[XmlAttribute]
	public string BiomeName;
	
	private float _neighborhoodBiomePresence;

	public static string GenerateId (Biome biome) {
	
		return "BiomeSurvivalSkill_" + biome.Id;
	}
	
	public static string GenerateName (Biome biome) {
		
		return biome.Name + " Survival";
	}
	
	public BiomeSurvivalSkill () {

	}

	public BiomeSurvivalSkill (CellGroup group, Biome biome, float value = 0f) : base (group, GenerateId (biome), GenerateName (biome), value) {
	
		BiomeName = biome.Name;
		
		CalculateNeighborhoodBiomePresence ();
	}

	public BiomeSurvivalSkill (CellGroup group, BiomeSurvivalSkill baseSkill) : base (group, baseSkill.Id, baseSkill.Name, baseSkill.Value) {

		BiomeName = baseSkill.BiomeName;
		
		CalculateNeighborhoodBiomePresence ();
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		CalculateNeighborhoodBiomePresence ();
	}
	
	public void CalculateNeighborhoodBiomePresence () {

		int groupCellBonus = 4;
		int cellCount = groupCellBonus;
		
		TerrainCell groupCell = Group.Cell;
		
		float totalPresence = groupCell.GetBiomePresence (BiomeName) * groupCellBonus;

		foreach (TerrainCell c in groupCell.Neighbors.Values) {
			
			totalPresence += c.GetBiomePresence (BiomeName);
			cellCount++;
		}
		
		_neighborhoodBiomePresence = totalPresence / cellCount;

		if ((_neighborhoodBiomePresence < 0) || (_neighborhoodBiomePresence > 1)) {
		
			throw new System.Exception ("Neighborhood Biome Presence outside range: " + _neighborhoodBiomePresence);
		}
		
		UpdateAdaptationLevel ();
	}

	public override void Update (int timeSpan) {

		TerrainCell groupCell = Group.Cell;

		float randomModifierFactor = 1f;
		float randomModifier = randomModifierFactor * _neighborhoodBiomePresence - groupCell.GetNextLocalRandomFloat ();

		float targetValue = 0;

		if (randomModifier > 0) {
			targetValue = Value + (1 - Value) * randomModifier;
		} else {
			targetValue = Value * (1 + randomModifier);
		}

		targetValue = Mathf.Clamp01 (targetValue);

		float presenceEffect = Mathf.Abs (Value - _neighborhoodBiomePresence);
		
		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		float factor = timeEffect * presenceEffect;
		
		Value = (Value * (1 - factor)) + (targetValue * factor);

		UpdateAdaptationLevel ();
	}

	public override void UpdateAdaptationLevel ()
	{
		AdaptationLevel = 1 - Mathf.Abs (Value - _neighborhoodBiomePresence);
	}
}

public class SeafaringSkill : CulturalSkill {

	public const float TimeEffectConstant = CellGroup.GenerationTime * 500;

	public const string SeafaringSkillId = "SeafaringSkill";

	public const string SeafaringSkillName = "Seafaring";

	private float _neighborhoodOceanPresence;

	public SeafaringSkill () {

	}

	public SeafaringSkill (CellGroup group, float value = 0f) : base (group, SeafaringSkillId, SeafaringSkillName, value) {

		CalculateNeighborhoodOceanPresence ();
	}

	public SeafaringSkill (CellGroup group, SeafaringSkill baseSkill) : base (group, baseSkill.Id, baseSkill.Name, baseSkill.Value) {

		CalculateNeighborhoodOceanPresence ();
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		CalculateNeighborhoodOceanPresence ();
	}

	public void CalculateNeighborhoodOceanPresence () {

		int groupCellBonus = 1;
		int cellCount = groupCellBonus;

		TerrainCell groupCell = Group.Cell;

		float totalPresence = groupCell.GetBiomePresence (Biome.Ocean.Name) * groupCellBonus;

		foreach (TerrainCell c in groupCell.Neighbors.Values) {

			totalPresence += c.GetBiomePresence (Biome.Ocean.Name);
			cellCount++;
		}

		_neighborhoodOceanPresence = totalPresence / cellCount;

		if ((_neighborhoodOceanPresence < 0) || (_neighborhoodOceanPresence > 1)) {

			throw new System.Exception ("Neighborhood Ocean Presence outside range: " + _neighborhoodOceanPresence);
		}

		UpdateAdaptationLevel ();
	}

	public override void Update (int timeSpan) {

		TerrainCell groupCell = Group.Cell;

		float randomModifierFactor = 1f;
		float randomModifier = randomModifierFactor * _neighborhoodOceanPresence - groupCell.GetNextLocalRandomFloat ();

		float targetValue = 0;

		if (randomModifier > 0) {
			targetValue = Value + (1 - Value) * randomModifier;
		} else {
			targetValue = Value * (1 + randomModifier);
		}

		targetValue = Mathf.Clamp01 (targetValue);

		float presenceEffect = Mathf.Abs (Value - _neighborhoodOceanPresence);

		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		float factor = timeEffect * presenceEffect;

		Value = (Value * (1 - factor)) + (targetValue * factor);

		UpdateAdaptationLevel ();
	}

	public override void UpdateAdaptationLevel ()
	{
		AdaptationLevel = 1 - Mathf.Abs (Value - _neighborhoodOceanPresence);
	}
}
