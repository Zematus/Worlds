using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class Culture {

	public List<CulturalActivity> Activities = new List<CulturalActivity> ();

	[XmlArrayItem(Type = typeof(BiomeSurvivalSkill)),
		XmlArrayItem(Type = typeof(SeafaringSkill))]
	public List<CulturalSkill> Skills = new List<CulturalSkill> ();
	
	[XmlArrayItem(Type = typeof(ShipbuildingKnowledge)),
		XmlArrayItem(Type = typeof(AgricultureKnowledge))]
	public List<CulturalKnowledge> Knowledges = new List<CulturalKnowledge> ();
	
	[XmlArrayItem(Type = typeof(BoatMakingDiscovery)),
		XmlArrayItem(Type = typeof(SailingDiscovery)),
		XmlArrayItem(Type = typeof(PlantCultivationDiscovery))]
	public List<CulturalDiscovery> Discoveries = new List<CulturalDiscovery> ();

	private Dictionary<string, CulturalActivity> _activities = new Dictionary<string, CulturalActivity> ();
	private Dictionary<string, CulturalSkill> _skills = new Dictionary<string, CulturalSkill> ();
	private Dictionary<string, CulturalKnowledge> _knowledges = new Dictionary<string, CulturalKnowledge> ();
	private Dictionary<string, CulturalDiscovery> _discoveries = new Dictionary<string, CulturalDiscovery> ();
	
	public Culture () {
	}

	protected void AddActivity (World world, CulturalActivity activity) {

		if (_activities.ContainsKey (activity.Id))
			return;

		world.AddExistingCulturalActivityInfo (activity);

		Activities.Add (activity);
		_activities.Add (activity.Id, activity);
	}

	protected void RemoveActivity (World world, CulturalActivity activity) {

		if (!_activities.ContainsKey (activity.Id))
			return;

		Activities.Remove (activity);
		_activities.Remove (activity.Id);
	}
	
	protected void AddSkill (World world, CulturalSkill skill) {

		if (_skills.ContainsKey (skill.Id))
			return;
		
		world.AddExistingCulturalSkillInfo (skill);

		Skills.Add (skill);
		_skills.Add (skill.Id, skill);
	}

	protected void RemoveSkill (World world, CulturalSkill skill) {

		if (!_skills.ContainsKey (skill.Id))
			return;

		Skills.Remove (skill);
		_skills.Remove (skill.Id);
	}
	
	protected void AddKnowledge (World world, CulturalKnowledge knowledge) {
		
		if (_knowledges.ContainsKey (knowledge.Id))
			return;
		
		world.AddExistingCulturalKnowledgeInfo (knowledge);

		Knowledges.Add (knowledge);
		_knowledges.Add (knowledge.Id, knowledge);
	}

	protected void RemoveKnowledge (World world, CulturalKnowledge knowledge) {

		if (!_knowledges.ContainsKey (knowledge.Id))
			return;

		Knowledges.Remove (knowledge);
		_knowledges.Remove (knowledge.Id);
	}
	
	protected void AddDiscovery (World world, CulturalDiscovery discovery) {
		
		if (_discoveries.ContainsKey (discovery.Id))
			return;
		
		world.AddExistingCulturalDiscoveryInfo (discovery);

		Discoveries.Add (discovery);
		_discoveries.Add (discovery.Id, discovery);
	}

	protected void RemoveDiscovery (World world, CulturalDiscovery discovery) {

		if (!_discoveries.ContainsKey (discovery.Id))
			return;

		Discoveries.Remove (discovery);
		_discoveries.Remove (discovery.Id);
	}

	public CulturalActivity GetActivity (string id) {

		CulturalActivity activity = null;

		if (!_activities.TryGetValue (id, out activity))
			return null;

		return activity;
	}
	
	public CulturalSkill GetSkill (string id) {

		CulturalSkill skill = null;

		if (!_skills.TryGetValue (id, out skill))
			return null;
		
		return skill;
	}
	
	public CulturalKnowledge GetKnowledge (string id) {
		
		CulturalKnowledge knowledge = null;
		
		if (!_knowledges.TryGetValue (id, out knowledge))
			return null;
		
		return knowledge;
	}
	
	public CulturalDiscovery GetDiscovery (string id) {
		
		CulturalDiscovery discovery = null;
		
		if (!_discoveries.TryGetValue (id, out discovery))
			return null;
		
		return discovery;
	}

	public virtual void FinalizeLoad () {

		Activities.ForEach (a => _activities.Add (a.Id, a));
		Skills.ForEach (s => _skills.Add (s.Id, s));
		Knowledges.ForEach (k => _knowledges.Add (k.Id, k));
		Discoveries.ForEach (d => _discoveries.Add (d.Id, d));
	}
}

public class CellCulture : Culture {
	
	public const float MinKnowledgeValue = 1f;
	public const float BaseKnowledgeTransferFactor = 0.1f;

	[XmlIgnore]
	public CellGroup Group;

	[XmlIgnore]
	public Dictionary<string, CulturalActivity> ActivitiesToPerform = new Dictionary<string, CulturalActivity> ();
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

		baseCulture.Activities.ForEach (a => AddActivity (a.GenerateCopy (group)));
		baseCulture.Skills.ForEach (s => AddSkill (s.GenerateCopy (group)));
		baseCulture.Discoveries.ForEach (d => AddDiscovery (d.GenerateCopy ()));
		baseCulture.Knowledges.ForEach (k => AddKnowledge (k.GenerateCopy (group)));
	}

	protected void AddActivity (CulturalActivity activity) {

		AddActivity (Group.World, activity);
	}

	protected void RemoveActivity (CulturalActivity activity) {

		RemoveActivity (Group.World, activity);
	}

	public void RemoveActivity (string activityId) {

		CulturalActivity activity = GetActivity (activityId);

		if (activity == null)
			return;

		RemoveActivity (Group.World, activity);
	}

	protected void AddSkill (CulturalSkill skill) {
	
		AddSkill (Group.World, skill);
	}

	protected void RemoveSkill (CulturalSkill skill) {

		RemoveSkill (Group.World, skill);
	}
	
	protected void AddKnowledge (CulturalKnowledge knowledge) {
		
		AddKnowledge (Group.World, knowledge);
	}

	protected void RemoveKnowledge (CulturalKnowledge knowledge) {

		RemoveKnowledge (Group.World, knowledge);
	}
	
	protected void AddDiscovery (CulturalDiscovery discovery) {
		
		AddDiscovery (Group.World, discovery);
	}

	protected void RemoveDiscovery (CulturalDiscovery discovery) {

		RemoveDiscovery (Group.World, discovery);
	}

	public void AddActivityToPerform (CulturalActivity activity) {

		if (ActivitiesToPerform.ContainsKey (activity.Id))
			return;

		ActivitiesToPerform.Add (activity.Id, activity);
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

		sourceCulture.Activities.ForEach (a => {

			CulturalActivity activity = GetActivity (a.Id);

			if (activity == null) {
				activity = a.GenerateCopy (Group);
				activity.ModifyValue (percentage);

				AddActivityToPerform (activity);
			} else {
				activity.Merge (a, percentage);
			}
		});

		sourceCulture.Skills.ForEach (s => {
			
			CulturalSkill skill = GetSkill (s.Id);
			
			if (skill == null) {
				skill = s.GenerateCopy (Group);
				skill.ModifyValue (percentage);
				
				AddSkillToLearn (skill);
			} else {
				skill.Merge (s, percentage);
			}
		});
		
		sourceCulture.Knowledges.ForEach (k => {
			
			CulturalKnowledge knowledge = GetKnowledge (k.Id);
			
			if (knowledge == null) {
				knowledge = k.GenerateCopy (Group);
				knowledge.ModifyValue (percentage);
				
				AddKnowledgeToLearn (knowledge);
			} else {
				knowledge.Merge (k, percentage);
			}
		});

		sourceCulture.Discoveries.ForEach (d => {

			CulturalDiscovery discovery = GetDiscovery (d.Id);
			
			if (discovery == null) {
				discovery = d.GenerateCopy ();
				
				AddDiscoveryToFind (discovery);
			}
		});
	}

	public void Update (int timeSpan) {

		float totalValue = 0;

		foreach (CulturalActivity activity in Activities) {

			activity.Update (timeSpan);
			totalValue += activity.Value;
		}

		foreach (CulturalActivity activity in Activities) {

			if (totalValue > 0) {
				activity.Contribution = activity.Value / totalValue;
			} else {
				activity.Contribution = 1f / Activities.Count;
			}
		}

		foreach (CulturalSkill skill in Skills) {
		
			skill.Update (timeSpan);
		}

		CulturalKnowledge[] knowledges = Knowledges.ToArray ();

		foreach (CulturalKnowledge knowledge in knowledges) {
			
			knowledge.Update (timeSpan);

			if (knowledge.WillBeLost ()) {

				RemoveKnowledge (knowledge);

				knowledge.LossConsequences ();
			}
		}

		bool discoveriesLost = false;

		CulturalDiscovery[] discoveries = Discoveries.ToArray ();
		
		foreach (CulturalDiscovery discovery in discoveries) {
			
			if (discovery.CanBeHeld (Group)) continue;

			RemoveDiscovery (discovery);

			discoveriesLost = true;
		}

		if (discoveriesLost) {

			foreach (CulturalKnowledge knowledge in Knowledges) {
				
				knowledge.RecalculateAsymptote ();
			}
		}
	}

	public void PostUpdate () {

		foreach (CulturalActivity activity in ActivitiesToPerform.Values) {

			AddActivity (activity);
		}
		
		foreach (CulturalSkill skill in SkillsToLearn.Values) {
			
			AddSkill (skill);
		}
		
		foreach (CulturalKnowledge knowledge in KnowledgesToLearn.Values) {
			
			AddKnowledge (knowledge);

			knowledge.RecalculateAsymptote ();
		}
		
		foreach (CulturalDiscovery discovery in DiscoveriesToFind.Values) {
			
			if (!discovery.CanBeHeld (Group)) continue;
			
			AddDiscovery (discovery);

			Knowledges.ForEach (k => {
				k.CalculateAsymptote (discovery);
			});
		}

		ActivitiesToPerform.Clear ();
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
	
	public void AbsorbKnowledgeFrom (CulturalKnowledge sourceKnowledge, float sourceFactor) {
		
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
	
	public void AbsorbDiscoveryFrom (CulturalDiscovery sourceDiscovery) {
		
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
			
			float level = knowledge.CalculateExpectedProgressLevel ();
			
			if (level < minProgressLevel) {
				minProgressLevel = level;
			}
		}
		
		return minProgressLevel;
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Activities.ForEach (a => {
		
			a.Group = Group;
		});

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
