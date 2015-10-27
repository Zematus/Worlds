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

	public CulturalSkill (string id, string name, float value) : base (id, name) {

		Value = value;
	}

	public static CulturalSkill Clone (CulturalSkill skill) {

		System.Type skillType = skill.GetType ();
	
		System.Reflection.ConstructorInfo cInfo = skillType.GetConstructor (new System.Type[] {skillType});

		return cInfo.Invoke (new object[] {skill}) as CulturalSkill;
	}

	public void MergeSkill (CulturalSkill skill, float percentage) {
	
		Value = Value * (1f - percentage) + skill.Value * percentage;
	}
	
	public void ModifyValue (float percentage) {
		
		Value *= percentage;
	}
	
	public abstract void Update (CellGroup group, int timeSpan);
	public abstract float AdaptationLevel (CellGroup group);
}

public class BiomeSurvivalSkill : CulturalSkill {

	public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;
	
	[XmlAttribute]
	public string BiomeName;

	//private float _biomeSurvivalEffect;

	public static string GenerateId (Biome biome) {
	
		return "BiomeSurvivalSkill_" + biome.Id;
	}
	
	public static string GenerateName (Biome biome) {
		
		return biome.Name + " Survival";
	}

	public BiomeSurvivalSkill (Biome biome, float value) : base (GenerateId (biome), GenerateName (biome), value) {
	
		BiomeName = biome.Name;

		//_biomeSurvivalEffect = 0.2f + 0.8f * Biome.Biomes [BiomeName].Survivability;
	}

	public BiomeSurvivalSkill (BiomeSurvivalSkill baseSkill) : base (baseSkill.Id, baseSkill.Name, baseSkill.Value) {

		BiomeName = baseSkill.BiomeName;
		
		//_biomeSurvivalEffect = 0.2f + 0.8f * Biome.Biomes [BiomeName].Survivability;
	}

	public override void Update (CellGroup group, int timeSpan) {

		TerrainCell cell = group.Cell;
		
		float presence = cell.GetBiomePresence (BiomeName);

		float randomModifier = presence - cell.GetNextLocalRandomFloat ();

		float targetValue = 0;

		if (randomModifier > 0) {
			targetValue = Value + (1 - Value) * randomModifier;
		} else {
			targetValue = Value * (1 + randomModifier);
		}

		float presenceEffect = Mathf.Abs (Value - presence);
		
		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);

		float factor = timeEffect * presenceEffect;
		
		Value = (Value * (1 - factor)) + (targetValue * factor);
	}

	public override float AdaptationLevel (CellGroup group) {
		
		TerrainCell cell = group.Cell;
		
		float presence = cell.GetBiomePresence (BiomeName);

		if (presence == Value)
			return 1;

		return 1 - Mathf.Abs (Value - presence);
	}
}

public abstract class Culture {
	
	private List<CulturalSkill> _skills = new List<CulturalSkill> ();
	
	public Culture () {
	}
	
	public Culture (Culture baseCulture) {
		
		foreach (CulturalSkill skill in baseCulture._skills) {
			
			_skills.Add (CulturalSkill.Clone (skill));
		}
	}
	
	public ICollection<CulturalSkill> Skills {

		get {
			return _skills;
		}
	}
	
	protected void AddSkill (World world, CulturalSkill skill) {
		
		world.AddExistingCulturalSkillId (skill);
		
		_skills.Add (skill);
	}
	
	public CulturalSkill GetSkill (string id) {
		
		foreach (CulturalSkill skill in _skills) {
			
			if (skill.Id == id) return skill;
		}
		
		return null;
	}
	
	public void MergeCulture (Culture sourceCulture, float percentage) {
		
		foreach (CulturalSkill sourceSkill in sourceCulture._skills) {
			
			CulturalSkill skill = GetSkill (sourceSkill.Id);
			
			if (skill == null) {
				skill = CulturalSkill.Clone (sourceSkill);
				skill.ModifyValue (percentage);
				
				_skills.Add (skill);
			} else {
				skill.MergeSkill (sourceSkill, percentage);
			}
		}
	}
}

public class CellCulture : Culture {

	[XmlIgnore]
	public CellGroup Group;

	public CellCulture () : base () {
	}

	public CellCulture (CellGroup group) : base () {

		Group = group;
	}

	public CellCulture (CellGroup group, Culture baseCulture) : base (baseCulture) {

		Group = group;
	}
	
	public void AddSkill (CulturalSkill skill) {
		
		AddSkill (Group.World, skill);
	}

	public void Update (int timeSpan) {

		foreach (CulturalSkill skill in Skills) {
		
			skill.Update (Group, timeSpan);
		}
	}

	public float AverageSkillAdaptationLevel () {

		if (Skills.Count == 0)
			throw new System.Exception ("Group has no cultural skills");

		float totalAdaptationLevel = 0;

		foreach (CulturalSkill skill in Skills) {
			
			totalAdaptationLevel += skill.AdaptationLevel (Group);
		}

		return totalAdaptationLevel / (float)Skills.Count;
	}
	
	public float MinimumSkillAdaptationLevel () {
		
		if (Skills.Count == 0)
			throw new System.Exception ("Group has no cultural skills");
		
		float minAdaptationLevel = 1f;
		
		foreach (CulturalSkill skill in Skills) {
			
			float level = skill.AdaptationLevel (Group);

			if (level < minAdaptationLevel) {
				minAdaptationLevel = level;
			}
		}
		
		return minAdaptationLevel;
	}
}
