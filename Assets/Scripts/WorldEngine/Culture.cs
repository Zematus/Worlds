using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class Culture {

	[XmlArrayItem(Type = typeof(BiomeSurvivalSkill))]
	public List<CulturalSkill> Skills = new List<CulturalSkill> ();
	
	[XmlArrayItem(Type = typeof(ShipbuildingKnowledge))]
	public List<CulturalKnowledge> Knowledges = new List<CulturalKnowledge> ();
	
	public Culture () {
	}
	
	protected void AddSkill (World world, CulturalSkill skill) {
		
		world.AddExistingCulturalSkillInfo (skill);
		
		Skills.Add (skill);
	}
	
	protected void AddKnowledge (World world, CulturalKnowledge knowledge) {
		
		world.AddExistingCulturalKnowledgeInfo (knowledge);
		
		Knowledges.Add (knowledge);
	}
	
	public CulturalSkill GetSkill (string id) {
		
		foreach (CulturalSkill skill in Skills) {
			
			if (skill.Id == id) return skill;
		}
		
		return null;
	}
	
	public CulturalKnowledge GetKnowledge (string id) {
		
		foreach (CulturalKnowledge knowledge in Knowledges) {
			
			if (knowledge.Id == id) return knowledge;
		}
		
		return null;
	}
}

public class CellCulture : Culture {
	
	public const float BaseKnowledgeTransferFactor = 0.25f;

	[XmlIgnore]
	public CellGroup Group;
	
	[XmlIgnore]
	public Dictionary<string, CulturalSkill> SkillsToLearn = new Dictionary<string, CulturalSkill> ();
	[XmlIgnore]
	public Dictionary<string, CulturalKnowledge> KnowledgesToLearn = new Dictionary<string, CulturalKnowledge> ();

	public CellCulture () {
	}

	public CellCulture (CellGroup group) : base () {

		Group = group;
	}

	public CellCulture (CellGroup group, CellCulture baseCulture) {

		Group = group;
		
		baseCulture.Skills.ForEach (s => Skills.Add (s.CopyWithGroup (group)));
	}
	
	public void AddSkillToLearn (CulturalSkill skill) {

		if (SkillsToLearn.ContainsKey (skill.Id))
			return;

		SkillsToLearn.Add (skill.Id, skill);
	}
	
	public void AddKnowledgeToLearn (CulturalKnowledge knowledge) {
		
		if (KnowledgesToLearn.ContainsKey (knowledge.Id))
			return;

		KnowledgesToLearn.Add (knowledge.Id, knowledge);
	}
	
	public void MergeCulture (CellCulture sourceCulture, float percentage) {

		sourceCulture.Skills.ForEach (s => {
			
			CulturalSkill skill = GetSkill (s.Id);
			
			if (skill == null) {
				skill = s.CopyWithGroup (Group);
				skill.ModifyValue (percentage);
				
				Skills.Add (skill);
			} else {
				skill.Merge (s, percentage);
			}
		});
		
		sourceCulture.Knowledges.ForEach (k => {
			
			CulturalKnowledge knowledge = GetKnowledge (k.Id);
			
			if (knowledge == null) {
				knowledge = k.CopyWithGroup (Group);
				knowledge.ModifyValue (percentage);
				
				Knowledges.Add (knowledge);
			} else {
				knowledge.Merge (k, percentage);
			}
		});
	}

	public void Update (int timeSpan) {

		foreach (CulturalSkill skill in Skills) {
		
			skill.Update (timeSpan);
		}

		foreach (CulturalKnowledge knowledge in Knowledges) {
			
			knowledge.Update (timeSpan);
		}
	}

	public void PostUpdate () {
		
		foreach (CulturalSkill skill in SkillsToLearn.Values) {
			
			AddSkill (Group.World, skill);
		}
		
		foreach (CulturalKnowledge knowledge in KnowledgesToLearn.Values) {
			
			AddKnowledge (Group.World, knowledge);
		}
		
		SkillsToLearn.Clear ();
		KnowledgesToLearn.Clear ();
	}
	
	public void TransferKnowledge (CulturalKnowledge sourceKnowledge, float factor) {
		
		CulturalKnowledge localKnowledge = GetKnowledge (sourceKnowledge.Id);
		
		if (localKnowledge == null) {
			
			localKnowledge = sourceKnowledge.CopyWithGroup (Group);
			localKnowledge.Value = 0;
			
			AddKnowledgeToLearn (localKnowledge);
		}
		
		float transferValueRate = localKnowledge.Value / sourceKnowledge.Value;
		
		if (transferValueRate >= 1) return;
		
		float transferFactor = BaseKnowledgeTransferFactor * (1 - transferValueRate);
		
		localKnowledge.IncreaseValue (localKnowledge.Value, transferFactor * factor);
	}
	
	public float MinimumSkillAdaptationLevel () {
		
		float minAdaptationLevel = 1f;
		
		foreach (CulturalSkill skill in Skills) {
			
			float level = skill.AdaptationLevel;

			if (level < minAdaptationLevel) {
				minAdaptationLevel = level;
			}
		}
		
		return minAdaptationLevel;
	}
	
	public float MinimumKnowledgeProgressLevel () {
		
		float minProgressLevel = 1f;
		
		foreach (CulturalKnowledge knowledge in Knowledges) {
			
			float level = knowledge.GetModifiedProgressLevel ();
			
			if (level < minProgressLevel) {
				minProgressLevel = level;
			}
		}
		
		return minProgressLevel;
	}
	
	public void FinalizeLoad () {

		Skills.ForEach (s => {

			s.Group = Group;
			s.FinalizeLoad ();
		});

		Knowledges.ForEach (k => {

			k.Group = Group;
			k.FinalizeLoad ();
		});
	}
}
