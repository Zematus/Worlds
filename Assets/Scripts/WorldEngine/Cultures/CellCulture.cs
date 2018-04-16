using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class CellCulture : Culture {
	
	public const float MinKnowledgeValue = 1f;
//	public const float BaseKnowledgeTransferFactor = 0.1f;

	[XmlIgnore]
	public CellGroup Group;

	[XmlIgnore]
	public Dictionary<string, CellCulturalPreference> PreferencesToAcquire = new Dictionary<string, CellCulturalPreference> ();
	[XmlIgnore]
	public Dictionary<string, CellCulturalActivity> ActivitiesToPerform = new Dictionary<string, CellCulturalActivity> ();
	[XmlIgnore]
	public Dictionary<string, CellCulturalSkill> SkillsToLearn = new Dictionary<string, CellCulturalSkill> ();
	[XmlIgnore]
	public Dictionary<string, CellCulturalKnowledge> KnowledgesToLearn = new Dictionary<string, CellCulturalKnowledge> ();
	[XmlIgnore]
	public Dictionary<string, CellCulturalDiscovery> DiscoveriesToFind = new Dictionary<string, CellCulturalDiscovery> ();

	private HashSet<CellCulturalPreference> _preferencesToLose = new HashSet<CellCulturalPreference> ();
	private HashSet<CellCulturalActivity> _activitiesToLose = new HashSet<CellCulturalActivity> ();
	private HashSet<CellCulturalSkill> _skillsToLose = new HashSet<CellCulturalSkill> ();
	private HashSet<CellCulturalKnowledge> _knowledgesToLose = new HashSet<CellCulturalKnowledge> ();
	private HashSet<CellCulturalDiscovery> _discoveriesToLose = new HashSet<CellCulturalDiscovery> ();

	public CellCulture () {
	}

	public CellCulture (CellGroup group) : base (group.World) {

		Group = group;
	}

	public CellCulture (CellGroup group, Culture sourceCulture) : base (group.World, sourceCulture.Language) {

		Group = group;

		foreach (CulturalPreference p in sourceCulture.Preferences) {
			AddPreference (CellCulturalPreference.CreateCellInstance (group, p));
		}

		foreach (CulturalActivity a in sourceCulture.Activities) {
			AddActivity (CellCulturalActivity.CreateCellInstance (group, a));
		}

		foreach (CulturalSkill s in sourceCulture.Skills) {
			AddSkill (CellCulturalSkill.CreateCellInstance (group, s));
		}

		foreach (CulturalKnowledge k in sourceCulture.Knowledges) {
			CellCulturalKnowledge knowledge = CellCulturalKnowledge.CreateCellInstance (group, k);

			AddKnowledge (knowledge);

			knowledge.CalculateAsymptote ();
		}

		foreach (CulturalDiscovery d in sourceCulture.Discoveries) {
			AddDiscovery (CellCulturalDiscovery.CreateCellInstance (d));

			foreach (CellCulturalKnowledge knowledge in Knowledges) {
				knowledge.CalculateAsymptote (d);
			}
		}
	}

	public void AddPreferenceToAcquire (CellCulturalPreference preference) {

		if (PreferencesToAcquire.ContainsKey (preference.Id))
			return;

		PreferencesToAcquire.Add (preference.Id, preference);
	}

	public void AddActivityToPerform (CellCulturalActivity activity) {

		if (ActivitiesToPerform.ContainsKey (activity.Id))
			return;

		ActivitiesToPerform.Add (activity.Id, activity);
	}
	
	public void AddSkillToLearn (CellCulturalSkill skill) {

		if (SkillsToLearn.ContainsKey (skill.Id))
			return;

		SkillsToLearn.Add (skill.Id, skill);
	}
	
	public void AddKnowledgeToLearn (CellCulturalKnowledge knowledge) {
		
		if (KnowledgesToLearn.ContainsKey (knowledge.Id))
			return;

		KnowledgesToLearn.Add (knowledge.Id, knowledge);
	}
	
	public void AddDiscoveryToFind (CellCulturalDiscovery discovery) {
		
		if (DiscoveriesToFind.ContainsKey (discovery.Id))
			return;
		
		DiscoveriesToFind.Add (discovery.Id, discovery);
	}

	public CellCulturalPreference GetAcquiredPerferenceOrToAcquire (string id) {

		CellCulturalPreference preference = GetPreference (id) as CellCulturalPreference;

		if (preference != null)
			return preference;

		if (PreferencesToAcquire.TryGetValue (id, out preference))
			return preference;

		return null;
	}

	public CellCulturalActivity GetPerformedActivityOrToPerform (string id) {

		CellCulturalActivity activity = GetActivity (id) as CellCulturalActivity;

		if (activity != null)
			return activity;

		if (ActivitiesToPerform.TryGetValue (id, out activity))
			return activity;

		return null;
	}

	public CellCulturalSkill GetLearnedSkillOrToLearn (string id) {

		CellCulturalSkill skill = GetSkill (id) as CellCulturalSkill;

		if (skill != null)
			return skill;

		if (SkillsToLearn.TryGetValue (id, out skill))
			return skill;

		return null;
	}

	public CellCulturalKnowledge GetLearnedKnowledgeOrToLearn (string id) {

		CellCulturalKnowledge knowledge = GetKnowledge (id) as CellCulturalKnowledge;

		if (knowledge != null)
			return knowledge;

		if (KnowledgesToLearn.TryGetValue (id, out knowledge))
			return knowledge;

		return null;
	}

	public CellCulturalDiscovery GetFoundDiscoveryOrToFind (string id) {

		CellCulturalDiscovery discovery = GetDiscovery (id) as CellCulturalDiscovery;

		if (discovery != null)
			return discovery;

		if (DiscoveriesToFind.TryGetValue (id, out discovery))
			return discovery;

		return null;
	}

	public void MergeCulture (Culture sourceCulture, float percentage) {

		#if DEBUG
		if ((percentage < 0) || (percentage > 1)) {

			Debug.LogWarning ("percentage value outside the [0,1] range");
		}
		#endif

		foreach (CulturalPreference p in sourceCulture.Preferences) {

			CellCulturalPreference preference = GetAcquiredPerferenceOrToAcquire (p.Id);

			if (preference == null) {
				preference = CellCulturalPreference.CreateCellInstance (Group, p);
				preference.DecreaseValue (percentage);

				AddPreferenceToAcquire (preference);
			} else {
				preference.Merge (p, percentage);
			}
		}

		foreach (CulturalActivity a in sourceCulture.Activities) {

			CellCulturalActivity activity = GetPerformedActivityOrToPerform (a.Id);

			if (activity == null) {
				activity = CellCulturalActivity.CreateCellInstance (Group, a);
				activity.DecreaseValue (percentage);

				AddActivityToPerform (activity);
			} else {
				activity.Merge (a, percentage);
			}
		}

		foreach (CulturalSkill s in sourceCulture.Skills) {

			CellCulturalSkill skill = GetLearnedSkillOrToLearn (s.Id);

			if (skill == null) {
				skill = CellCulturalSkill.CreateCellInstance (Group, s);
				skill.DecreaseValue (percentage);

				AddSkillToLearn (skill);
			} else {
				skill.Merge (s, percentage);
			}
		}

		foreach (CulturalKnowledge k in sourceCulture.Knowledges) {

			CellCulturalKnowledge knowledge = GetLearnedKnowledgeOrToLearn (k.Id);

			if (knowledge == null) {
				knowledge = CellCulturalKnowledge.CreateCellInstance (Group, k);
				knowledge.DecreaseValue (percentage);

				AddKnowledgeToLearn (knowledge);
			} else {
				knowledge.Merge (k, percentage);
			}
		}

		foreach (CulturalDiscovery d in sourceCulture.Discoveries) {

			CellCulturalDiscovery discovery = GetFoundDiscoveryOrToFind (d.Id);

			if (discovery == null) {
				
				discovery = CellCulturalDiscovery.CreateCellInstance (d);
				AddDiscoveryToFind (discovery);
			}
		}
	}

	public void Update (long timeSpan) {

		foreach (CellCulturalPreference preference in Preferences) {

			preference.Update (timeSpan);
		}

		foreach (CellCulturalActivity activity in Activities) {

			activity.Update (timeSpan);
		}

		foreach (CellCulturalSkill skill in Skills) {
		
			skill.Update (timeSpan);
		}

		foreach (CellCulturalKnowledge knowledge in Knowledges) {
			
			knowledge.Update (timeSpan);
		}
	}

	public void UpdatePolityCulturalProminence (PolityProminence polityProminence, long timeSpan) {

		PolityCulture polityCulture = polityProminence.Polity.Culture;

		foreach (CulturalPreference polityPreference in polityCulture.Preferences) {

			CellCulturalPreference cellPreference = GetAcquiredPerferenceOrToAcquire (polityPreference.Id);

			if (cellPreference == null) {

				cellPreference = CellCulturalPreference.CreateCellInstance (Group, polityPreference, 0);
				AddPreferenceToAcquire (cellPreference);
			}

			cellPreference.PolityCulturalProminence (polityPreference, polityProminence, timeSpan);
		}

		foreach (CulturalActivity polityActivity in polityCulture.Activities) {

			CellCulturalActivity cellActivity = GetPerformedActivityOrToPerform (polityActivity.Id);

			if (cellActivity == null) {
			
				cellActivity = CellCulturalActivity.CreateCellInstance (Group, polityActivity, 0);
				AddActivityToPerform (cellActivity);
			}

			cellActivity.PolityCulturalProminence (polityActivity, polityProminence, timeSpan);
		}

		foreach (CulturalSkill politySkill in polityCulture.Skills) {

			CellCulturalSkill cellSkill = GetLearnedSkillOrToLearn (politySkill.Id);

			if (cellSkill == null) {

				cellSkill = CellCulturalSkill.CreateCellInstance (Group, politySkill, 0);
				AddSkillToLearn (cellSkill);
			}

			cellSkill.PolityCulturalProminence (politySkill, polityProminence, timeSpan);
		}

		foreach (CulturalKnowledge polityKnowledge in polityCulture.Knowledges) {

			CellCulturalKnowledge cellKnowledge = GetLearnedKnowledgeOrToLearn (polityKnowledge.Id);

			if (cellKnowledge == null) {

				cellKnowledge = CellCulturalKnowledge.CreateCellInstance (Group, polityKnowledge, 0);
				AddKnowledgeToLearn (cellKnowledge);
			}

			cellKnowledge.PolityCulturalProminence (polityKnowledge, polityProminence, timeSpan);
		}

		foreach (CulturalDiscovery polityDiscovery in polityCulture.Discoveries) {

			CellCulturalDiscovery cellDiscovery = GetFoundDiscoveryOrToFind (polityDiscovery.Id);

			if (cellDiscovery == null) {

				cellDiscovery = CellCulturalDiscovery.CreateCellInstance (polityDiscovery);
				AddDiscoveryToFind (cellDiscovery);
			}
		}
	}

	public void PostUpdatePolityCulturalProminence (PolityProminence polityProminence) {

		PolityCulture polityCulture = polityProminence.Polity.Culture;

		if ((Language == null) || (polityProminence.Value >= Group.HighestPolityProminence.Value)) {

			Language = polityCulture.Language;
		}
	}

	public void PostUpdateRemoveAttributes () {

		bool discoveriesLost = false;

		foreach (CellCulturalPreference p in _preferencesToLose) {
			RemovePreference (p);
		}

		foreach (CellCulturalActivity a in _activitiesToLose) {
			RemoveActivity (a);
		}

		foreach (CellCulturalSkill s in _skillsToLose) { 
			RemoveSkill (s);
		}

		foreach (CellCulturalKnowledge k in _knowledgesToLose) {

			RemoveKnowledge (k);
			k.LossConsequences ();
		}

		foreach (CellCulturalDiscovery d in _discoveriesToLose) {

			RemoveDiscovery (d);
			d.LossConsequences (Group);
			discoveriesLost = true;
		}

		if (discoveriesLost) {
			foreach (CellCulturalKnowledge knowledge in Knowledges) {
				(knowledge as CellCulturalKnowledge).RecalculateAsymptote ();
			}
		}

		_preferencesToLose.Clear ();
		_activitiesToLose.Clear ();
		_skillsToLose.Clear ();
		_knowledgesToLose.Clear ();
		_discoveriesToLose.Clear ();
	}

	public void PostUpdateAddAttributes () {

		foreach (CellCulturalPreference preference in PreferencesToAcquire.Values) {

			AddPreference (preference);
		}

		foreach (CellCulturalActivity activity in ActivitiesToPerform.Values) {

			AddActivity (activity);
		}

		foreach (CellCulturalSkill skill in SkillsToLearn.Values) {

			AddSkill (skill);
		}

		foreach (CellCulturalKnowledge knowledge in KnowledgesToLearn.Values) {

			AddKnowledge (knowledge);

			knowledge.RecalculateAsymptote ();
		}

		foreach (CellCulturalDiscovery discovery in DiscoveriesToFind.Values) {

			if (!discovery.CanBeHeld (Group)) continue;

			AddDiscovery (discovery);

			discovery.GainConsequences (Group);

			foreach (CellCulturalKnowledge knowledge in Knowledges) {
				knowledge.CalculateAsymptote (discovery);
			}
		}

		PreferencesToAcquire.Clear ();
		ActivitiesToPerform.Clear ();
		SkillsToLearn.Clear ();
		KnowledgesToLearn.Clear ();
		DiscoveriesToFind.Clear ();
	}

	public void PostUpdateAttributeValues () {

		foreach (CellCulturalPreference preference in Preferences) {

			preference.PostUpdate ();
		}

		float totalActivityValue = 0;

		foreach (CellCulturalActivity activity in Activities) {

			activity.PostUpdate ();
			totalActivityValue += activity.Value;
		}

		foreach (CellCulturalActivity activity in Activities) {

			if (totalActivityValue > 0) {
				activity.Contribution = activity.Value / totalActivityValue;
			} else {
				activity.Contribution = 1f / Activities.Count;
			}
		}

		foreach (CellCulturalSkill skill in Skills) {

			skill.PostUpdate ();
		}

		foreach (CellCulturalKnowledge knowledge in Knowledges) {

			knowledge.PostUpdate ();

			if (!knowledge.WillBeLost ())
				continue;

			_knowledgesToLose.Add (knowledge);
		}

		foreach (CellCulturalDiscovery discovery in Discoveries) {

			if (discovery.CanBeHeld (Group))
				continue;

			_discoveriesToLose.Add (discovery);
		}
	}

	public void PostUpdate () {

		PostUpdateAddAttributes ();

		PostUpdateAttributeValues ();

		PostUpdateRemoveAttributes ();
	}
	
	public float MinimumSkillAdaptationLevel () {
		
		float minAdaptationLevel = 1f;
		
		foreach (CellCulturalSkill skill in Skills) {
			
			float level = skill.AdaptationLevel;

			if (level < minAdaptationLevel) {
				minAdaptationLevel = level;
			}
		}
		
		return minAdaptationLevel;
	}
	
	public float MinimumKnowledgeProgressLevel () {
		
		float minProgressLevel = 1f;
		
		foreach (CellCulturalKnowledge knowledge in Knowledges) {
			
			float level = knowledge.CalculateExpectedProgressLevel ();
			
			if (level < minProgressLevel) {
				minProgressLevel = level;
			}
		}
		
		return minProgressLevel;
	}

	public override void Synchronize () {

		foreach (CellCulturalSkill s in Skills) {

			s.Synchronize ();
		}
		foreach (CellCulturalKnowledge k in Knowledges) {

			k.Synchronize ();
		}

		base.Synchronize ();
	}
	
	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		foreach (CellCulturalPreference p in Preferences) {
			p.Group = Group;
		}

		foreach (CellCulturalActivity a in Activities) {
			a.Group = Group;
		}

		foreach (CellCulturalSkill s in Skills) {
			
			s.Group = Group;
			s.FinalizeLoad ();
		}
		foreach (CellCulturalKnowledge k in Knowledges) {
			
			k.Group = Group;
			k.FinalizeLoad ();
		}
	}
}
