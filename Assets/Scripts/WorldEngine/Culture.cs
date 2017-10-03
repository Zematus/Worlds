using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class Culture : ISynchronizable {

	[XmlAttribute]
	public long LanguageId = -1;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public Language Language { get; protected set; }

	[XmlArrayItem(Type = typeof(CulturalActivity)),
		XmlArrayItem(Type = typeof(CellCulturalActivity))]
	public List<CulturalActivity> Activities = new List<CulturalActivity> ();

	[XmlArrayItem(Type = typeof(CulturalSkill)),
		XmlArrayItem(Type = typeof(BiomeSurvivalSkill)),
		XmlArrayItem(Type = typeof(SeafaringSkill))]
	public List<CulturalSkill> Skills = new List<CulturalSkill> ();
	
	[XmlArrayItem(Type = typeof(PolityCulturalKnowledge)),
		XmlArrayItem(Type = typeof(ShipbuildingKnowledge)),
		XmlArrayItem(Type = typeof(AgricultureKnowledge)),
		XmlArrayItem(Type = typeof(SocialOrganizationKnowledge))]
	public List<CulturalKnowledge> Knowledges = new List<CulturalKnowledge> ();
	
	[XmlArrayItem(Type = typeof(PolityCulturalDiscovery)),
		XmlArrayItem(Type = typeof(BoatMakingDiscovery)),
		XmlArrayItem(Type = typeof(SailingDiscovery)),
		XmlArrayItem(Type = typeof(TribalismDiscovery)),
		XmlArrayItem(Type = typeof(PlantCultivationDiscovery))]
	public List<CulturalDiscovery> Discoveries = new List<CulturalDiscovery> ();

	private Dictionary<string, CulturalActivity> _activities = new Dictionary<string, CulturalActivity> ();
	private Dictionary<string, CulturalSkill> _skills = new Dictionary<string, CulturalSkill> ();
	private Dictionary<string, CulturalKnowledge> _knowledges = new Dictionary<string, CulturalKnowledge> ();
	private Dictionary<string, CulturalDiscovery> _discoveries = new Dictionary<string, CulturalDiscovery> ();
	
	public Culture () {
	}

	public Culture (World world, Language language = null) {

		Language = language;

		World = world;
	}

	public Culture (Culture sourceCulture) : this (sourceCulture.World, sourceCulture.Language) {

		sourceCulture.Activities.ForEach (a => AddActivity (new CulturalActivity (a)));
		sourceCulture.Skills.ForEach (s => AddSkill (new CulturalSkill (s)));
		sourceCulture.Discoveries.ForEach (d => AddDiscovery (new CulturalDiscovery (d)));
		sourceCulture.Knowledges.ForEach (k => AddKnowledge (new CulturalKnowledge (k)));
	}

	protected void AddActivity (CulturalActivity activity) {

		if (_activities.ContainsKey (activity.Id))
			return;

		World.AddExistingCulturalActivityInfo (activity);

		Activities.Add (activity);
		_activities.Add (activity.Id, activity);
	}

	protected void RemoveActivity (CulturalActivity activity) {

		if (!_activities.ContainsKey (activity.Id))
			return;

		Activities.Remove (activity);
		_activities.Remove (activity.Id);
	}

	public void RemoveActivity (string activityId) {

		CulturalActivity activity = GetActivity (activityId);

		if (activity == null)
			return;

		RemoveActivity (activity);
	}
	
	protected void AddSkill (CulturalSkill skill) {

		if (_skills.ContainsKey (skill.Id))
			return;
		
		World.AddExistingCulturalSkillInfo (skill);

		Skills.Add (skill);
		_skills.Add (skill.Id, skill);
	}

	protected void RemoveSkill (CulturalSkill skill) {

		if (!_skills.ContainsKey (skill.Id))
			return;

		Skills.Remove (skill);
		_skills.Remove (skill.Id);
	}
	
	protected void AddKnowledge (CulturalKnowledge knowledge) {
		
		if (_knowledges.ContainsKey (knowledge.Id))
			return;
		
		World.AddExistingCulturalKnowledgeInfo (knowledge);

		Knowledges.Add (knowledge);
		_knowledges.Add (knowledge.Id, knowledge);
	}

	protected void RemoveKnowledge (CulturalKnowledge knowledge) {

		if (!_knowledges.ContainsKey (knowledge.Id))
			return;

		Knowledges.Remove (knowledge);
		_knowledges.Remove (knowledge.Id);
	}
	
	protected void AddDiscovery (CulturalDiscovery discovery) {
		
		if (_discoveries.ContainsKey (discovery.Id))
			return;
		
		World.AddExistingCulturalDiscoveryInfo (discovery);

		Discoveries.Add (discovery);
		_discoveries.Add (discovery.Id, discovery);
	}

	protected void RemoveDiscovery (CulturalDiscovery discovery) {

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

	public virtual void Synchronize () {

		if (Language != null)
			LanguageId = Language.Id;
	}

	public virtual void FinalizeLoad () {

		Activities.ForEach (a => _activities.Add (a.Id, a));
		Skills.ForEach (s => _skills.Add (s.Id, s));
		Knowledges.ForEach (k => _knowledges.Add (k.Id, k));
		Discoveries.ForEach (d => _discoveries.Add (d.Id, d));

		if (LanguageId != -1) {
			Language = World.GetLanguage (LanguageId);
		}
	}
}

public class PolityCulture : Culture {

	[XmlIgnore]
	public Polity Polity;

	[XmlIgnore]
	private float _totalGroupInfluenceValue;

	public PolityCulture () {
	
	}

	public PolityCulture (Polity polity) : base (polity.World) {

		Polity = polity;

		#if DEBUG
		if (World.SelectedCell != null && 
			World.SelectedCell.Group != null) {

			if (World.SelectedCell.Group.GetPolityInfluenceValue (Polity) > 0) {

				Debug.Log ("Debug Selected");
			}
		}
		#endif

		CellGroup coreGroup = Polity.CoreGroup;

		if (coreGroup == null)
			throw new System.Exception ("CoreGroup can't be null at this point");

		CellCulture coreCulture = coreGroup.Culture;

		coreCulture.Activities.ForEach (a => AddActivity (new CulturalActivity (a)));
		coreCulture.Skills.ForEach (s => AddSkill (new CulturalSkill (s)));

		coreCulture.Knowledges.ForEach (k => {
			PolityCulturalKnowledge knowledge = new PolityCulturalKnowledge (k);
			AddKnowledge (knowledge);

			#if DEBUG
			if (float.IsNaN(knowledge.Value)) {

				Debug.Break ();
				throw new System.Exception ("Debug.Break");
			}
			#endif
		});

		coreCulture.Discoveries.ForEach (d => {
			PolityCulturalDiscovery discovery = new PolityCulturalDiscovery (d);
			AddDiscovery (discovery);
			discovery.PresenceCount++;
		});

		Language = coreCulture.Language;

		if (Language == null) {
		
			GenerateNewLanguage ();
		}
	}

	public float GetNextRandomFloat (int rngOffset) {

		return Polity.GetNextLocalRandomFloat (rngOffset);
	}

	private void GenerateNewLanguage () {

		//Language = new Language (World.GenerateLanguageId ());
		Language = new Language (Polity.GenerateUniqueIdentifier (1000, Polity.Id));

		int rngOffset = 0;

		Language.GetRandomFloatDelegate getNextRandomFloat = () => GetNextRandomFloat (RngOffsets.POLITY_CULTURE_GENERATE_NEW_LANGUAGE + rngOffset++);

		// Generate Articles

		Language.GenerateArticleProperties (getNextRandomFloat);

		Language.GenerateArticleAdjunctionProperties (getNextRandomFloat);
		Language.GenerateArticleSyllables (getNextRandomFloat);
		Language.GenerateAllArticles (getNextRandomFloat);

		// Generate Indicatives

		Language.GenerateNounIndicativeProperties (getNextRandomFloat);

		Language.GenerateNounIndicativeAdjunctionProperties (getNextRandomFloat);
		Language.GenerateNounIndicativeSyllables (getNextRandomFloat);
		Language.GenerateAllNounIndicatives (getNextRandomFloat);

		// Generate Noun, Adjective and Adposition Properties and Syllables

		Language.GenerateNounAdjunctionProperties (getNextRandomFloat);
		Language.GenerateNounSyllables (getNextRandomFloat);

		Language.GenerateAdjectiveAdjunctionProperties (getNextRandomFloat);
		Language.GenerateAdjectiveSyllables (getNextRandomFloat);

		Language.GenerateAdpositionAdjunctionProperties (getNextRandomFloat);
		Language.GenerateAdpositionSyllables (getNextRandomFloat);

		World.AddLanguage (Language);
	}

	public void Update () {

		ResetAttributeValues ();

		AddGroupCultures ();

		NormalizeAttributeValues ();
	}

	private void ResetAttributeValues () {

		foreach (CulturalActivity activity in Activities) {

			activity.Value = 0;
			activity.Contribution = 0;
		}

		foreach (CulturalSkill skill in Skills) {
			skill.Value = 0;
		}

		foreach (PolityCulturalKnowledge knowledge in Knowledges) {

			knowledge.AggregateValue = 0;
			knowledge.Value = 0;
		}

		foreach (PolityCulturalDiscovery discovery in Discoveries) {

			discovery.PresenceCount = 0;
		}
	}

	private void NormalizeAttributeValues () {

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			Manager.RegisterDebugEvent ("DebugMessage", 
//				"NormalizeAttributeValues - Polity:" + Polity.Id + 
//				", CurrentDate: " + World.CurrentDate + 
//				", Activities.Count: " + Activities.Count + 
//				", Skills.Count: " + Skills.Count + 
//				", Knowledges.Count: " + Knowledges.Count + 
//				", Polity.TotalGroupInfluenceValue: " + Polity.TotalGroupInfluenceValue + 
//				"");
//		}
//		#endif

		if (Polity.InfluencedGroups.Count <= 0)
			return;

		foreach (CulturalActivity activity in Activities) {

			activity.Value = MathUtility.RoundToSixDecimals(Mathf.Clamp01 (activity.Value/_totalGroupInfluenceValue));
			activity.Contribution = MathUtility.RoundToSixDecimals(Mathf.Clamp01 (activity.Contribution/_totalGroupInfluenceValue));
		}

		foreach (CulturalSkill skill in Skills) {

			float realValue = skill.Value / _totalGroupInfluenceValue;

			#if DEBUG
			if ((realValue > 1.1f) || (realValue < -0.1f)) {
				throw new System.Exception ("Polity Skill value way out of bounds (-0.1f,1.1f): " + realValue);
			}
			#endif

			skill.Value = MathUtility.RoundToSixDecimals(Mathf.Clamp01(realValue));
		}

		foreach (PolityCulturalKnowledge knowledge in Knowledges) {


			float d;
			int newValue = (int)MathUtility.DivideAndGetDecimals (knowledge.AggregateValue, _totalGroupInfluenceValue, out d);

			if (d > GetNextRandomFloat (RngOffsets.POLITY_CULTURE_NORMALIZE_ATTRIBUTE_VALUES))
				newValue++;

			knowledge.Value = newValue;
		}
	}

	private void AddGroupCultures () {

		_totalGroupInfluenceValue = 0;

		foreach (CellGroup group in Polity.InfluencedGroups.Values) {
		
			AddGroupCulture (group);
		}
	}

	private void AddGroupCulture (CellGroup group) {

//		#if DEBUG
//		if (World.SelectedCell != null && 
//			World.SelectedCell.Group != null) {
//
//			if (World.SelectedCell.Group.GetPolityInfluenceValue (Polity) > 0) {
//
//				Debug.Log ("Debug Selected");
//			}
//		}
//		#endif

		float influenceValue = group.GetPolityInfluenceValue (Polity);

		_totalGroupInfluenceValue += influenceValue;

		if (influenceValue <= 0) {

			throw new System.Exception ("Polity [" + Polity.Id + "] has influence value of " + influenceValue + " in Group [" + group.Id + "]. Current Date: " + World.CurrentDate);
		}

		foreach (CulturalActivity groupActivity in group.Culture.Activities) {
		
			CulturalActivity activity = GetActivity (groupActivity.Id);

			if (activity == null) {
			
				activity = new CulturalActivity (groupActivity);
				activity.Value *= influenceValue;
				activity.Contribution *= influenceValue;

				AddActivity (activity);

			} else {
			
				activity.Value += groupActivity.Value * influenceValue;
				activity.Contribution += groupActivity.Contribution * influenceValue;
			}
		}

		foreach (CulturalSkill groupSkill in group.Culture.Skills) {

			CulturalSkill skill = GetSkill (groupSkill.Id);

			if (skill == null) {

				skill = new CulturalSkill (groupSkill);
				skill.Value *= influenceValue;

				AddSkill (skill);

			} else {

				skill.Value += groupSkill.Value * influenceValue;
			}
		}

		foreach (CulturalKnowledge groupKnowledge in group.Culture.Knowledges) {

			PolityCulturalKnowledge knowledge = GetKnowledge (groupKnowledge.Id) as PolityCulturalKnowledge;

			if (knowledge == null) {

				knowledge = new PolityCulturalKnowledge (groupKnowledge);
				knowledge.AggregateValue = groupKnowledge.Value * influenceValue;

				AddKnowledge (knowledge);

			} else {
				
				knowledge.AggregateValue += groupKnowledge.Value * influenceValue;
			}
		}

		foreach (CulturalDiscovery groupDiscovery in group.Culture.Discoveries) {

			PolityCulturalDiscovery discovery = GetDiscovery (groupDiscovery.Id) as PolityCulturalDiscovery;

			if (discovery == null) {

				discovery = new PolityCulturalDiscovery (groupDiscovery);

				AddDiscovery (discovery);
			}

			discovery.PresenceCount++;
		}
	}
}

public class CellCulture : Culture {
	
	public const float MinKnowledgeValue = 1f;
//	public const float BaseKnowledgeTransferFactor = 0.1f;

	[XmlIgnore]
	public CellGroup Group;

	[XmlIgnore]
	public Dictionary<string, CellCulturalActivity> ActivitiesToPerform = new Dictionary<string, CellCulturalActivity> ();
	[XmlIgnore]
	public Dictionary<string, CellCulturalSkill> SkillsToLearn = new Dictionary<string, CellCulturalSkill> ();
	[XmlIgnore]
	public Dictionary<string, CellCulturalKnowledge> KnowledgesToLearn = new Dictionary<string, CellCulturalKnowledge> ();
	[XmlIgnore]
	public Dictionary<string, CellCulturalDiscovery> DiscoveriesToFind = new Dictionary<string, CellCulturalDiscovery> ();

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

		sourceCulture.Activities.ForEach (a => AddActivity (CellCulturalActivity.CreateCellInstance (group, a)));
		sourceCulture.Skills.ForEach (s => AddSkill (CellCulturalSkill.CreateCellInstance (group, s)));

		sourceCulture.Knowledges.ForEach (k => {

			CellCulturalKnowledge knowledge = CellCulturalKnowledge.CreateCellInstance (group, k);

			AddKnowledge (knowledge);

			knowledge.CalculateAsymptote ();
		});

		sourceCulture.Discoveries.ForEach (d => {
			
			AddDiscovery (CellCulturalDiscovery.CreateCellInstance (d));

			foreach (CellCulturalKnowledge knowledge in Knowledges) {
				knowledge.CalculateAsymptote (d);
			}
		});
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

		foreach (CulturalActivity a in sourceCulture.Activities) {

			CellCulturalActivity activity = GetPerformedActivityOrToPerform (a.Id);

			if (activity == null) {
				activity = CellCulturalActivity.CreateCellInstance (Group, a);
				activity.ModifyValue (percentage);

				AddActivityToPerform (activity);
			} else {
				activity.Merge (a, percentage);
			}
		}

		foreach (CulturalSkill s in sourceCulture.Skills) {

			CellCulturalSkill skill = GetLearnedSkillOrToLearn (s.Id);

			if (skill == null) {
				skill = CellCulturalSkill.CreateCellInstance (Group, s);
				skill.ModifyValue (percentage);

				AddSkillToLearn (skill);
			} else {
				skill.Merge (s, percentage);
			}
		}

		foreach (CulturalKnowledge k in sourceCulture.Knowledges) {

			CellCulturalKnowledge knowledge = GetLearnedKnowledgeOrToLearn (k.Id);

			if (knowledge == null) {
				knowledge = CellCulturalKnowledge.CreateCellInstance (Group, k);
				knowledge.ModifyValue (percentage);

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

	public void Update (int timeSpan) {

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

	public void UpdatePolityCulturalInfluence (PolityInfluence polityInfluence, int timeSpan) {

		PolityCulture polityCulture = polityInfluence.Polity.Culture;

		foreach (CulturalActivity polityActivity in polityCulture.Activities) {

			CellCulturalActivity cellActivity = GetPerformedActivityOrToPerform (polityActivity.Id);

			if (cellActivity == null) {
			
				cellActivity = CellCulturalActivity.CreateCellInstance (Group, polityActivity, 0);
				AddActivityToPerform (cellActivity);
			}

			cellActivity.PolityCulturalInfluence (polityActivity, polityInfluence, timeSpan);
		}

		foreach (CulturalSkill politySkill in polityCulture.Skills) {

			CellCulturalSkill cellSkill = GetLearnedSkillOrToLearn (politySkill.Id);

			if (cellSkill == null) {

				cellSkill = CellCulturalSkill.CreateCellInstance (Group, politySkill, 0);
				AddSkillToLearn (cellSkill);
			}

			cellSkill.PolityCulturalInfluence (politySkill, polityInfluence, timeSpan);
		}

		foreach (CulturalKnowledge polityKnowledge in polityCulture.Knowledges) {

			CellCulturalKnowledge cellKnowledge = GetLearnedKnowledgeOrToLearn (polityKnowledge.Id);

			if (cellKnowledge == null) {

				cellKnowledge = CellCulturalKnowledge.CreateCellInstance (Group, polityKnowledge, 0);
				AddKnowledgeToLearn (cellKnowledge);
			}

			cellKnowledge.PolityCulturalInfluence (polityKnowledge, polityInfluence, timeSpan);
		}

		foreach (PolityCulturalDiscovery polityDiscovery in polityCulture.Discoveries) {

			CellCulturalDiscovery cellDiscovery = GetFoundDiscoveryOrToFind (polityDiscovery.Id);

			if (cellDiscovery == null) {

				cellDiscovery = CellCulturalDiscovery.CreateCellInstance (polityDiscovery);
				AddDiscoveryToFind (cellDiscovery);
			}
		}
	}

	public void PostUpdatePolityCulturalInfluence (PolityInfluence polityInfluence) {

		PolityCulture polityCulture = polityInfluence.Polity.Culture;

		if ((Language == null) || (polityInfluence.Value >= Group.HighestPolityInfluence.Value)) {

			Language = polityCulture.Language;
		}
	}

	public void PostUpdateRemoveAttributes () {

		bool discoveriesLost = false;

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

		_activitiesToLose.Clear ();
		_skillsToLose.Clear ();
		_knowledgesToLose.Clear ();
		_discoveriesToLose.Clear ();
	}

	public void PostUpdateAddAttributes () {

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

		ActivitiesToPerform.Clear ();
		SkillsToLearn.Clear ();
		KnowledgesToLearn.Clear ();
		DiscoveriesToFind.Clear ();
	}

	public void PostUpdateAttributeValues () {

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

public class BufferCulture : Culture {

	public BufferCulture () {
	}

	public BufferCulture (Culture sourceCulture) : base (sourceCulture) {
	}
}
