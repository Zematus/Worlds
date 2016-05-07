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

public class CulturalSkill : CulturalSkillInfo {

	[XmlAttribute]
	public float Value;

	public CulturalSkill () {
	}

	public CulturalSkill (string id, string name, float value) : base (id, name) {

		Value = value;
	}

	public CulturalSkill (CulturalSkill baseSkill) : base (baseSkill) {

		Value = baseSkill.Value;
	}
}

public abstract class CellCulturalSkill : CulturalSkill, Synchronizable {
	
	[XmlAttribute]
	public float AdaptationLevel;

	[XmlIgnore]
	public CellGroup Group;
	
	public CellCulturalSkill () {
	}

	protected CellCulturalSkill (CellGroup group, string id, string name, float value = 0) : base (id, name, value) {

		Group = group;
	}

	public static CellCulturalSkill CreateCellInstance (CellGroup group, CulturalSkill baseSkill, float initialValue) {

		if (BiomeSurvivalSkill.IsBiomeSurvivalSkill (baseSkill)) {
		
			return new BiomeSurvivalSkill (group, baseSkill, initialValue);
		}

		if (SeafaringSkill.IsSeafaringSkill (baseSkill)) {

			return new SeafaringSkill (group, baseSkill, initialValue);
		}

		throw new System.Exception ("Unexpected CulturalSkill type: " + baseSkill.Id);
	}
	
	public CellCulturalSkill GenerateCopy (CellGroup targetGroup) {
		
		System.Type skillType = this.GetType ();
		
		System.Reflection.ConstructorInfo cInfo = skillType.GetConstructor (new System.Type[] {typeof(CellGroup), skillType});
		
		return cInfo.Invoke (new object[] {targetGroup, this}) as CellCulturalSkill;
	}

	public void Merge (CellCulturalSkill skill, float percentage) {
	
		Value = Value * (1f - percentage) + skill.Value * percentage;
	}
	
	public void ModifyValue (float percentage) {
		
		Value *= percentage;
	}

	public virtual void Synchronize () {

	}

	public virtual void FinalizeLoad () {

	}

	public abstract void Update (int timeSpan);

	protected void UpdateInternal (int timeSpan, float timeEffectFactor, float specificModifier) {

		TerrainCell groupCell = Group.Cell;

		float randomModifier = groupCell.GetNextLocalRandomFloat ();
		randomModifier *= randomModifier;
		float randomFactor = specificModifier - randomModifier;

		float maxTargetValue = 1.0f;
		float minTargetValue = -0.2f;
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

	public abstract void PolityCulturalInfluence (CulturalSkill politySkill, PolityInfluence polityInfluence, int timeSpan);

	protected void PolityCulturalInfluenceInternal (CulturalSkill politySkill, PolityInfluence polityInfluence, int timeSpan, float timeEffectFactor) {

		float targetValue = politySkill.Value;
		float influenceEffect = polityInfluence.Value;

		TerrainCell groupCell = Group.Cell;

		float randomEffect = groupCell.GetNextLocalRandomFloat ();

		float timeEffect = timeSpan / (float)(timeSpan + timeEffectFactor);

		float change = (targetValue - Value) * influenceEffect * timeEffect * randomEffect;

		Value += change;

		Value = Mathf.Clamp01 (Value);
	}

	protected void RecalculateAdaptation (float targetValue)
	{
		AdaptationLevel = 1 - Mathf.Abs (Value - targetValue);
	}
}

public class BiomeSurvivalSkill : CellCulturalSkill {

	public const float TimeEffectConstant = CellGroup.GenerationTime * 1500;

	public const string BiomeSurvivalSkillIdPrefix = "BiomeSurvivalSkill_";
	
	[XmlAttribute]
	public string BiomeName;
	
	private float _neighborhoodBiomePresence;

	public static string GenerateId (Biome biome) {
	
		return BiomeSurvivalSkillIdPrefix + biome.Id;
	}
	
	public static string GenerateName (Biome biome) {
		
		return biome.Name + " Survival";
	}
	
	public BiomeSurvivalSkill () {

	}

	public BiomeSurvivalSkill (CellGroup group, Biome biome, float value) : base (group, GenerateId (biome), GenerateName (biome), value) {
	
		BiomeName = biome.Name;
		
		CalculateNeighborhoodBiomePresence ();
	}

	public BiomeSurvivalSkill (CellGroup group, BiomeSurvivalSkill baseSkill) : base (group, baseSkill.Id, baseSkill.Name, baseSkill.Value) {

		BiomeName = baseSkill.BiomeName;
		
		CalculateNeighborhoodBiomePresence ();
	}

	public BiomeSurvivalSkill (CellGroup group, CulturalSkill baseSkill, float initialValue) : base (group, baseSkill.Id, baseSkill.Name, initialValue) {

		int suffixIndex = baseSkill.Name.IndexOf (" Survival");

		BiomeName = baseSkill.Name.Substring (0, suffixIndex);

		CalculateNeighborhoodBiomePresence ();
	}

	public static bool IsBiomeSurvivalSkill (CulturalSkill skill) {

		return skill.Id.Contains (BiomeSurvivalSkillIdPrefix);
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		CalculateNeighborhoodBiomePresence ();
	}
	
	public void CalculateNeighborhoodBiomePresence () {

		int groupCellBonus = 2;
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

		RecalculateAdaptation (_neighborhoodBiomePresence);
	}

	public override void Update (int timeSpan) {

		UpdateInternal (timeSpan, TimeEffectConstant, _neighborhoodBiomePresence);

		RecalculateAdaptation (_neighborhoodBiomePresence);
	}

	public override void PolityCulturalInfluence (CulturalSkill politySkill, PolityInfluence polityInfluence, int timeSpan) {

		PolityCulturalInfluenceInternal (politySkill, polityInfluence, timeSpan, TimeEffectConstant);

		RecalculateAdaptation (_neighborhoodBiomePresence);
	}
}

public class SeafaringSkill : CellCulturalSkill {

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

	public SeafaringSkill (CellGroup group, CulturalSkill baseSkill, float initialValue) : base (group, baseSkill.Id, baseSkill.Name, initialValue) {

		CalculateNeighborhoodOceanPresence ();
	}

	public static bool IsSeafaringSkill (CulturalSkill skill) {

		return skill.Id.Contains (SeafaringSkillId);
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

		RecalculateAdaptation (_neighborhoodOceanPresence);
	}

	public override void Update (int timeSpan) {

		UpdateInternal (timeSpan, TimeEffectConstant, _neighborhoodOceanPresence);

		RecalculateAdaptation (_neighborhoodOceanPresence);
	}

	public override void PolityCulturalInfluence (CulturalSkill politySkill, PolityInfluence polityInfluence, int timeSpan) {

		PolityCulturalInfluenceInternal (politySkill, polityInfluence, timeSpan, TimeEffectConstant);

		RecalculateAdaptation (_neighborhoodOceanPresence);
	}
}
