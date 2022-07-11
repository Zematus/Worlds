using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public abstract class Culture : ISynchronizable
{
    #region LanguageId
    [XmlAttribute("LId")]
    public string LanguageIdStr
    {
        get { return LanguageId; }
        set { LanguageId = value; }
    }
    [XmlIgnore]
    public Identifier LanguageId;
    #endregion

    public List<CulturalPreference> Preferences = null;
    public List<CulturalActivity> Activities = null;
    public List<CulturalSkill> Skills = null;
    public List<CulturalKnowledge> Knowledges = null;

    public List<string> DiscoveryIds;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public Language Language { get; set; }

    [XmlIgnore]
    public Dictionary<string, IDiscovery> Discoveries = new Dictionary<string, IDiscovery>();

    protected Dictionary<string, CulturalPreference> _preferences = new Dictionary<string, CulturalPreference>();
    protected Dictionary<string, CulturalActivity> _activities = new Dictionary<string, CulturalActivity>();
    protected Dictionary<string, CulturalSkill> _skills = new Dictionary<string, CulturalSkill>();
    protected Dictionary<string, CulturalKnowledge> _knowledges = new Dictionary<string, CulturalKnowledge>();

    // preference references hardcoded for performance reasons
    private CulturalPreference _aggressionPreference;
    private CulturalPreference _isolationPreference;

    public Culture()
    {
    }

    public Culture(World world, Language language = null)
    {
        Language = language;

        World = world;
    }

    public Culture(Culture sourceCulture) : this(sourceCulture.World, sourceCulture.Language)
    {
        foreach (CulturalPreference p in sourceCulture._preferences.Values)
        {
            AddPreference(new CulturalPreference(p));
        }

        foreach (CulturalActivity a in sourceCulture._activities.Values)
        {
            AddActivity(new CulturalActivity(a));
        }

        foreach (CulturalSkill s in sourceCulture._skills.Values)
        {
            AddSkill(new CulturalSkill(s));
        }

        foreach (CulturalKnowledge k in sourceCulture._knowledges.Values)
        {
            AddKnowledge(new CulturalKnowledge(k));
        }

        foreach (var d in sourceCulture.Discoveries.Values)
        {
            AddDiscovery(d);
        }
    }

    public abstract void SetHolderToUpdate(bool warnIfUnexpected = true);

    /// <summary>
    /// Adds a new preference if not already present to the culture
    /// </summary>
    /// <param name="preference">The preference to try to add</param>
    public void AddPreference(CulturalPreference preference)
    {
        if (_preferences.ContainsKey(preference.Id))
            return;

        World.AddExistingCulturalPreferenceInfo(preference);

        _preferences.Add(preference.Id, preference);

        switch (preference.Id)
        {
            case CulturalPreference.AggressionPreferenceId:

                _aggressionPreference = preference;
                break;

            case CulturalPreference.IsolationPreferenceId:

                _isolationPreference = preference;
                break;
        }
    }

    /// <summary>
    /// Removes a preference from the culture.
    /// Note: Some preferences can't be removed
    /// </summary>
    /// <param name="preference">The preference to remove</param>
    public void RemovePreference(CulturalPreference preference)
    {
        if (!_preferences.ContainsKey(preference.Id))
            return;

        if ((preference.Id == CulturalPreference.AggressionPreferenceId) ||
            (preference.Id == CulturalPreference.IsolationPreferenceId))
        {
            throw new System.Exception(
                "Illegal Op: Preference with id:" + preference.Id + " can't be removed");
        }

        _preferences.Remove(preference.Id);
    }

    public void ResetPreferences()
    {
        foreach (CulturalPreference preference in _preferences.Values)
        {
            preference.Reset();
        }
    }

    protected void AddActivity(CulturalActivity activity)
    {
        if (_activities.ContainsKey(activity.Id))
            return;

        World.AddExistingCulturalActivityInfo(activity);

        _activities.Add(activity.Id, activity);
    }

    protected void RemoveActivity(CulturalActivity activity)
    {
        if (!_activities.ContainsKey(activity.Id))
            return;

        _activities.Remove(activity.Id);
    }

    public void ResetActivities()
    {
        foreach (CulturalActivity activity in _activities.Values)
        {
            activity.Reset();
        }
    }

    protected void AddSkill(CulturalSkill skill)
    {
        if (_skills.ContainsKey(skill.Id))
            return;

        World.AddExistingCulturalSkillInfo(skill);

        _skills.Add(skill.Id, skill);
    }

    protected void RemoveSkill(CulturalSkill skill)
    {
        if (!_skills.ContainsKey(skill.Id))
            return;

        _skills.Remove(skill.Id);
    }

    public void ResetSkills()
    {
        foreach (CulturalSkill skill in _skills.Values)
        {
            skill.Reset();
        }
    }

    public void AddKnowledge(CulturalKnowledge knowledge)
    {
        if (_knowledges.ContainsKey(knowledge.Id))
            return;

        World.AddExistingCulturalKnowledgeInfo(knowledge);

        _knowledges.Add(knowledge.Id, knowledge);
    }

    protected void RemoveKnowledge(CulturalKnowledge knowledge)
    {
        if (!_knowledges.ContainsKey(knowledge.Id))
            return;

        _knowledges.Remove(knowledge.Id);
    }

    public void ResetKnowledges()
    {
        foreach (CulturalKnowledge knowledge in _knowledges.Values)
        {
            knowledge.Reset();
        }
    }

    protected virtual void AddDiscovery(IDiscovery discovery)
    {
        if (Discoveries.ContainsKey(discovery.Id))
            return;

        World.AddExistingDiscovery(discovery);

        Discoveries.Add(discovery.Id, discovery);
    }

    protected virtual void RemoveDiscovery(IDiscovery discovery)
    {
        if (!Discoveries.ContainsKey(discovery.Id))
            return;

        Discoveries.Remove(discovery.Id);
    }

    public void ResetDiscoveries() => Discoveries.Clear();

    public ICollection<CulturalPreference> GetPreferences() => _preferences.Values;

    public CulturalPreference GetPreference(string id)
    {
        if (!_preferences.TryGetValue(id, out CulturalPreference preference))
            return null;

        return preference;
    }

    public bool HasPreference(string id) => _preferences.ContainsKey(id);

    /// <summary>
    /// Returns the current value for the preference for aggression.
    /// Note: This function exists for performance reasons and assumes the
    /// preference is always present (which should be)
    /// </summary>
    /// <returns>the aggression preference value</returns>
    public float GetAggressionPreferenceValue() => _aggressionPreference.Value;

    /// <summary>
    /// Returns the current value for the preference for isolation.
    /// Note: This function exists for performance reasons and assumes the
    /// preference is always present (which should be)
    /// </summary>
    /// <returns>the isolation preference value</returns>
    public float GetIsolationPreferenceValue() => _isolationPreference.Value;

    /// <summary>
    /// Returns the current value for the specified reference or 0 if not present
    /// </summary>
    /// <param name="id">the id of the preference to query</param>
    /// <returns>the value of the preference</returns>
    public float GetPreferenceValue(string id)
    {
        if (!_preferences.TryGetValue(id, out CulturalPreference preference))
            return 0;

        return preference.Value;
    }

    public ICollection<CulturalActivity> GetActivities() => _activities.Values;

    /// <summary>
    /// Gets the activity if present in the culture
    /// </summary>
    /// <param name="id">the id of the activity</param>
    /// <returns>the activity, or null if not present</returns>
    public CulturalActivity GetActivity(string id)
    {
        if (!_activities.TryGetValue(id, out CulturalActivity activity))
            return null;

        return activity;
    }

    /// <summary>
    /// Returns the activity contribution level, or 0 if activity not present
    /// </summary>
    /// <param name="id">the id of the activity</param>
    /// <returns>the contribution level</returns>
    public float GetActivityContribution(string id)
    {
        if (!_activities.TryGetValue(id, out CulturalActivity activity))
            return 0;

        return activity.Contribution;
    }

    public bool HasActivity(string id) => _activities.ContainsKey(id);

    public ICollection<CulturalSkill> GetSkills() => _skills.Values;

    public CulturalSkill GetSkill(string id)
    {
        if (!_skills.TryGetValue(id, out CulturalSkill skill))
            return null;

        return skill;
    }

    public bool TryGetSkillValue(string id, out float value)
    {
        value = 0;

        var skill = GetSkill(id);

        if (skill != null)
        {
            value = skill.Value;

            return true;
        }

        return false;
    }

    public bool HasSkill(string id) => _skills.ContainsKey(id);

    public ICollection<CulturalKnowledge> GetKnowledges() => _knowledges.Values;

    public CulturalKnowledge GetKnowledge(string id)
    {
        if (!_knowledges.TryGetValue(id, out CulturalKnowledge knowledge))
            return null;

        return knowledge;
    }

    public bool TryGetKnowledgeValue(string id, out float value)
    {
        value = 0;

        var knowledge = GetKnowledge(id);

        if (knowledge != null)
        {
            value = knowledge.Value;

            return true;
        }

        return false;
    }

    public bool HasKnowledge(string id) => _knowledges.ContainsKey(id);

    public IDiscovery GetDiscovery(string id)
    {
        if (!Discoveries.TryGetValue(id, out var discovery))
            return null;

        return discovery;
    }

    public bool HasDiscovery(string id) => Discoveries.ContainsKey(id);

    public void ResetAttributes()
    {
        ResetPreferences();
        ResetActivities();
        ResetSkills();
        ResetKnowledges();
        ResetDiscoveries();
    }

    public virtual void Synchronize()
    {
        Preferences = new List<CulturalPreference>(_preferences.Values);
        Activities = new List<CulturalActivity>(_activities.Values);
        Skills = new List<CulturalSkill>(_skills.Values);
        Knowledges = new List<CulturalKnowledge>(_knowledges.Values);

        // Reset property dictionaries to ensure they are ordered in the same way the would be in the save file
        _preferences.Clear();
        _activities.Clear();
        _skills.Clear();
        _knowledges.Clear();

        LoadPreferences();
        LoadActivities();
        LoadSkills();
        LoadKnowledges();

        if (Language != null)
            LanguageId = Language.Id;

        DiscoveryIds = new List<string>(Discoveries.Keys);
    }

    public virtual void FinalizeLoad()
    {
        if (LanguageId != null)
        {
            Language = World.GetLanguage(LanguageId);
        }

        PrefinalizePropertiesLoad();
        FinalizePropertiesLoad();
    }

    public void LoadPreferences()
    {
        foreach (CulturalPreference p in Preferences)
        {
            AddPreference(p);
        }
    }

    public void LoadActivities()
    {
        foreach (CulturalActivity a in Activities)
        {
            AddActivity(a);
        }
    }

    public void LoadSkills()
    {
        foreach (CulturalSkill s in Skills)
        {
            AddSkill(s);
        }
    }

    public void LoadKnowledges()
    {
        foreach (CulturalKnowledge k in Knowledges)
        {
            AddKnowledge(k);
        }
    }

    public virtual void PrefinalizePropertiesLoad()
    {
        LoadPreferences();
        LoadActivities();
        LoadSkills();
        LoadKnowledges();
    }

    public void FinalizePropertiesLoad()
    {
        foreach (CulturalPreference p in Preferences)
        {
            p.FinalizeLoad();
        }

        foreach (CulturalActivity a in Activities)
        {
            a.FinalizeLoad();
        }

        foreach (CulturalSkill s in Skills)
        {
            s.FinalizeLoad();
        }

        foreach (CulturalKnowledge k in Knowledges)
        {
            k.FinalizeLoad();
        }

        foreach (string discoveryId in DiscoveryIds)
        {
            IDiscovery discovery = null;

            discovery = Discovery033.GetDiscovery(discoveryId);

            Discoveries.Add(discoveryId, discovery);
        }
    }

    public static float CalculateAggressionDiff(Culture cultureA, Culture cultureB)
    {
        Profiler.BeginSample("CalculateAggressionTowards");

        float aggrPrefB = cultureB.GetAggressionPreferenceValue();
        float aggrPrefA = cultureA.GetAggressionPreferenceValue();

        Profiler.EndSample(); // ("CalculateAggressionTowards");

        return aggrPrefA - aggrPrefB;
    }
}
