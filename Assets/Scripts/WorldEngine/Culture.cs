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
}

public class BiomeSurvivalSkill : CulturalSkill {

	public const float TimeEffectConstant = CellGroup.GenerationSpan * 100;
	
	[XmlAttribute]
	public string Biome;

	public static string GenerateId (string biome) {
	
		return "BiomeSurvivalSkill : " + biome;
	}

	public BiomeSurvivalSkill (string biome, float value) : base (GenerateId (biome), value) {
	
		Biome = biome;
	}

	public BiomeSurvivalSkill (BiomeSurvivalSkill baseSkill) : base (baseSkill.Id, baseSkill.Value) {

		Biome = baseSkill.Biome;
	}
	
	public override void Update (CellGroup group, int timeSpan) {

		TerrainCell cell = group.Cell;

		float timeEffect = timeSpan / (float)(timeSpan + TimeEffectConstant);
		
		float presence = cell.GetBiomePresence (Biome);
		
		float targetValue = cell.GetNextLocalRandomFloat () * (Value - presence) + presence;
		
		Value = (Value * (1 - timeEffect)) + (targetValue * timeEffect);
	}
}

public class Culture {

	public List<CulturalSkill> Skills = new List<CulturalSkill> ();

	public Culture () {
	}

	public Culture (Culture baseCulture) {

		foreach (CulturalSkill skill in baseCulture.Skills) {

			Skills.Add (CulturalSkill.Clone (skill));
		}
	}

	public CulturalSkill GetSkill (string id) {

		foreach (CulturalSkill skill in Skills) {
		
			if (skill.Id == id) return skill;
		}

		return null;
	}

	public void MergeCulture (Culture sourceCulture, float percentage) {

		foreach (CulturalSkill sourceSkill in sourceCulture.Skills) {
		
			CulturalSkill skill = GetSkill (sourceSkill.Id);

			if (skill == null) {
				skill = CulturalSkill.Clone (sourceSkill);
				skill.ModifyValue (percentage);

				Skills.Add (skill);
			} else {
				skill.MergeSkill (sourceSkill, percentage);
			}
		}
	}

	public void Update (CellGroup group, int timeSpan) {

		foreach (CulturalSkill skill in Skills) {
		
			skill.Update (group, timeSpan);
		}
	}
}
