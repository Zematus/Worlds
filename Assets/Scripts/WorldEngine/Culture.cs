using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class CulturalSkill {

	[XmlAttribute]
	public string Id;
	
	[XmlAttribute]
	public float Value;

	public CulturalSkill (string id, float value) {
	
		Id = id;

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

	public const float TimeEffectConstant = CellGroup.GenerationSpan * 100;
	
	[XmlAttribute]
	public string Biome;

	public static string GenerateId (Biome biome) {
	
		return "BiomeSurvivalSkill_" + biome.Id;
	}

	public BiomeSurvivalSkill (Biome biome, float value) : base (GenerateId (biome), value) {
	
		Biome = biome.Name;
	}

	public BiomeSurvivalSkill (BiomeSurvivalSkill baseSkill) : base (baseSkill.Id, baseSkill.Value) {

		Biome = baseSkill.Biome;
	}

	public override void Update (CellGroup group, int timeSpan) {

		TerrainCell cell = group.Cell;
		
		float presence = cell.GetBiomePresence (Biome);

		float randomModifier = presence - cell.GetNextLocalRandomFloat ();

		float targetValue = 0;

		if (randomModifier > 0) {
			targetValue = Value + (1 - Value) * randomModifier;
		} else {
			targetValue = Value * (1 + randomModifier);
		}

		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);
		
		Value = (Value * (1 - timeEffect)) + (targetValue * timeEffect);
	}

	public override float AdaptationLevel (CellGroup group) {
		
		TerrainCell cell = group.Cell;
		
		float presence = cell.GetBiomePresence (Biome);

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
		
		world.AddExistingCulturalSkillId (skill.Id);
		
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

	public float SkillAdaptationLevel () {

		if (Skills.Count == 0)
			throw new System.Exception ("Group has no cultural skills");

		float totalAdaptationLevel = 0;

		foreach (CulturalSkill skill in Skills) {
			
			totalAdaptationLevel += skill.AdaptationLevel (Group);
		}

		return totalAdaptationLevel / (float)Skills.Count;
	}
}
