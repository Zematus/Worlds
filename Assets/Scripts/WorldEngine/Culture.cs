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
	
	[XmlArrayItem(Type = typeof(BoatMakingDiscovery))]
	[XmlArrayItem(Type = typeof(SailingDiscovery))]
	public List<CulturalDiscovery> Discoveries = new List<CulturalDiscovery> ();
	
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
	
	protected void AddDiscovery (World world, CulturalDiscovery discovery) {
		
		world.AddExistingCulturalDiscoveryInfo (discovery);
		
		Discoveries.Add (discovery);
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
	
	public CulturalDiscovery GetDiscovery (string id) {
		
		foreach (CulturalDiscovery discovery in Discoveries) {
			
			if (discovery.Id == id) return discovery;
		}
		
		return null;
	}
}

public class CellCulture : Culture {
	
	public const float MinKnowledgeValue = 1f;
	public const float BaseKnowledgeTransferFactor = 0.1f;

	[XmlIgnore]
	public CellGroup Group;
	
	[XmlIgnore]
	public Dictionary<string, CulturalSkill> SkillsToLearn = new Dictionary<string, CulturalSkill> ();
	[XmlIgnore]
	public Dictionary<string, CulturalKnowledge> KnowledgesToLearn = new Dictionary<string, CulturalKnowledge> ();
	[XmlIgnore]
	public Dictionary<string, CulturalDiscovery> DiscoveriesToFind = new Dictionary<string, CulturalDiscovery> ();

	public CellCulture () {
	}

	public CellCulture (CellGroup group) : base () {

		Group = group;
	}

	public CellCulture (CellGroup group, CellCulture baseCulture) {

		Group = group;
		
		baseCulture.Skills.ForEach (s => Skills.Add (s.GenerateCopy (group)));
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
	
	public void AddDiscoveryToFind (CulturalDiscovery discovery) {
		
		if (DiscoveriesToFind.ContainsKey (discovery.Id))
			return;
		
		DiscoveriesToFind.Add (discovery.Id, discovery);
	}
	
	public void MergeCulture (CellCulture sourceCulture, float percentage) {

		sourceCulture.Skills.ForEach (s => {
			
			CulturalSkill skill = GetSkill (s.Id);
			
			if (skill == null) {
				skill = s.GenerateCopy (Group);
				skill.ModifyValue (percentage);
				
				Skills.Add (skill);
			} else {
				skill.Merge (s, percentage);
			}
		});
		
		sourceCulture.Knowledges.ForEach (k => {
			
			CulturalKnowledge knowledge = GetKnowledge (k.Id);
			
			if (knowledge == null) {
				knowledge = k.GenerateCopy (Group);
				knowledge.ModifyValue (percentage);
				
				Knowledges.Add (knowledge);
			} else {
				knowledge.Merge (k, percentage);
			}
		});

		sourceCulture.Discoveries.ForEach (d => {

			CulturalDiscovery discovery = GetDiscovery (d.Id);
			
			if (discovery == null) {

				if (!d.CanBeHold (sourceCulture.Group)) return;

				discovery = d.GenerateCopy ();
				
				Discoveries.Add (discovery);

				Knowledges.ForEach (k => k.CalculateAndSetAsymptote (discovery));
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

		bool discoveriesLost = false;

		CulturalDiscovery[] discoveries = Discoveries.ToArray ();
		
		foreach (CulturalDiscovery discovery in discoveries) {
			
			if (discovery.CanBeHold (Group)) continue;

			Discoveries.Remove (discovery);

			discoveriesLost = true;
		}

		if (discoveriesLost) {

			foreach (CulturalKnowledge knowledge in Knowledges) {
				
				knowledge.RecalculateAsymptote ();
			}
		}
	}

	public void PostUpdate () {
		
		foreach (CulturalSkill skill in SkillsToLearn.Values) {
			
			AddSkill (Group.World, skill);
		}
		
		foreach (CulturalKnowledge knowledge in KnowledgesToLearn.Values) {
			
			AddKnowledge (Group.World, knowledge);
		}
		
		foreach (CulturalDiscovery discovery in DiscoveriesToFind.Values) {
			
			if (!discovery.CanBeHold (Group)) continue;
			
			AddDiscovery (Group.World, discovery);

			Knowledges.ForEach (k => k.CalculateAndSetAsymptote (discovery));
		}
		
		SkillsToLearn.Clear ();
		KnowledgesToLearn.Clear ();
		DiscoveriesToFind.Clear ();
	}
	
	public static float CalculateKnowledgeTransferValue (CellGroup sourceGroup, CellGroup targetGroup) {
		
		float maxTransferValue = 0;
		
		if (sourceGroup == null)
			return maxTransferValue;
		
		if (targetGroup == null)
			return maxTransferValue;
		
		if (!sourceGroup.StillPresent)
			return maxTransferValue;
		
		if (!targetGroup.StillPresent)
			return maxTransferValue;
		
		foreach (CulturalKnowledge sourceKnowledge in sourceGroup.Culture.Knowledges) {
			
			if (sourceKnowledge.Value <= MinKnowledgeValue) continue;
			
			CulturalKnowledge targetKnowledge = targetGroup.Culture.GetKnowledge (sourceKnowledge.Id);
			
			if (targetKnowledge == null) {
				maxTransferValue = 1;
			} else {
				maxTransferValue = Mathf.Max (maxTransferValue, 1 - (targetKnowledge.Value / sourceKnowledge.Value));
			}
		}
		
		return maxTransferValue;
	}
	
	public void TransferKnowledge (CulturalKnowledge sourceKnowledge, float sourceFactor) {
		
		if (sourceKnowledge.Value < MinKnowledgeValue) return;
		
		CulturalKnowledge localKnowledge = GetKnowledge (sourceKnowledge.Id);
		
		if (localKnowledge == null) {
			
			localKnowledge = sourceKnowledge.GenerateCopy (Group, 0);
			
			AddKnowledgeToLearn (localKnowledge);
		}
		
		if (localKnowledge.Value >= sourceKnowledge.Value) return;

		float specificKnowledgeFactor = localKnowledge.CalculateTransferFactor ();
		
		localKnowledge.IncreaseValue (sourceKnowledge.Value, BaseKnowledgeTransferFactor * specificKnowledgeFactor * sourceFactor);
	}
	
	public void TransferDiscovery (CulturalDiscovery sourceDiscovery) {
		
		CulturalDiscovery localDiscovery = GetDiscovery (sourceDiscovery.Id);
		
		if (localDiscovery == null) {
			
			localDiscovery = sourceDiscovery.GenerateCopy ();
			
			AddDiscoveryToFind (localDiscovery);
		}
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
			
			float level = knowledge.CalculateModifiedProgressLevel ();
			
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
