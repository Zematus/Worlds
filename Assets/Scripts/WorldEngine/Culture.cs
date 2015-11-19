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

public static class CulturalSkillHelper {

	public static CulturalSkill CopyWithGroup (this CulturalSkill skill, CellGroup group) {
		
		System.Type skillType = skill.GetType ();
		
		System.Reflection.ConstructorInfo cInfo = skillType.GetConstructor (new System.Type[] {typeof(CellGroup), skillType});
		
		return cInfo.Invoke (new object[] {group, skill}) as CulturalSkill;
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

	public void MergeSkill (CulturalSkill skill, float percentage) {
	
		Value = Value * (1f - percentage) + skill.Value * percentage;
	}
	
	public void ModifyValue (float percentage) {
		
		Value *= percentage;
	}

	public virtual void FinalizeLoad () {

	}

	public abstract void Update (int timeSpan);
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

	public BiomeSurvivalSkill (CellGroup group, Biome biome, float value) : base (group, GenerateId (biome), GenerateName (biome), value) {
	
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
		
		groupCell.GetNeighborCells ().ForEach (c => {
			
			totalPresence += c.GetBiomePresence (BiomeName);
			cellCount++;
		});
		
		_neighborhoodBiomePresence = totalPresence / cellCount;

		if ((_neighborhoodBiomePresence < 0) || (_neighborhoodBiomePresence > 1)) {
		
			throw new System.Exception ("Neighborhood Biome Presence outside range: " + _neighborhoodBiomePresence);
		}
		
		AdaptationLevel = 1 - Mathf.Abs (Value - _neighborhoodBiomePresence);
	}

	public override void Update (int timeSpan) {

		TerrainCell groupCell = Group.Cell;

		float randomModifier = _neighborhoodBiomePresence - groupCell.GetNextLocalRandomFloat ();

		float targetValue = 0;

		if (randomModifier > 0) {
			targetValue = Value + (1 - Value) * randomModifier;
		} else {
			targetValue = Value * (1 + randomModifier);
		}

		float presenceEffect = Mathf.Abs (Value - _neighborhoodBiomePresence);
		
		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		float factor = timeEffect * presenceEffect;
		
		Value = (Value * (1 - factor)) + (targetValue * factor);

		AdaptationLevel = 1 - Mathf.Abs (Value - _neighborhoodBiomePresence);
	}
}

public abstract class Culture {

	[XmlArrayItem(Type = typeof(BiomeSurvivalSkill))]
	public List<CulturalSkill> Skills = new List<CulturalSkill> ();
	
	public Culture () {
	}
	
	protected void AddSkill (World world, CulturalSkill skill) {
		
		world.AddExistingCulturalSkillInfo (skill);
		
		Skills.Add (skill);
	}
	
	public CulturalSkill GetSkill (string id) {
		
		foreach (CulturalSkill skill in Skills) {
			
			if (skill.Id == id) return skill;
		}
		
		return null;
	}
}

public class CellCulture : Culture {

	[XmlIgnore]
	public CellGroup Group;

	public CellCulture () {
	}

	public CellCulture (CellGroup group) : base () {

		Group = group;
	}

	public CellCulture (CellGroup group, CellCulture baseCulture) {

		Group = group;
		
		baseCulture.Skills.ForEach (s => Skills.Add (s.CopyWithGroup (group)));
	}
	
	public void AddSkill (CulturalSkill skill) {
		
		AddSkill (Group.World, skill);
	}
	
	public void MergeCulture (CellCulture sourceCulture, float percentage) {

		sourceCulture.Skills.ForEach (s => {
			
			CulturalSkill skill = GetSkill (s.Id);
			
			if (skill == null) {
				skill = s.CopyWithGroup (s.Group);
				skill.ModifyValue (percentage);
				
				Skills.Add (skill);
			} else {
				skill.MergeSkill (s, percentage);
			}
		});
	}

	public void Update (int timeSpan) {

		foreach (CulturalSkill skill in Skills) {
		
			skill.Update (timeSpan);
		}
	}

	public float AverageSkillAdaptationLevel () {

		if (Skills.Count == 0)
			throw new System.Exception ("Group has no cultural skills");

		float totalAdaptationLevel = 0;

		foreach (CulturalSkill skill in Skills) {
			
			totalAdaptationLevel += skill.AdaptationLevel;
		}

		return totalAdaptationLevel / (float)Skills.Count;
	}
	
	public float MinimumSkillAdaptationLevel () {
		
		if (Skills.Count == 0)
			throw new System.Exception ("Group has no cultural skills");
		
		float minAdaptationLevel = 1f;
		
		foreach (CulturalSkill skill in Skills) {
			
			float level = skill.AdaptationLevel;

			if (level < minAdaptationLevel) {
				minAdaptationLevel = level;
			}
		}
		
		return minAdaptationLevel;
	}
	
	public void FinalizeLoad () {

		Skills.ForEach (s => {

			s.Group = Group;
			s.FinalizeLoad ();
		});
	}
}
