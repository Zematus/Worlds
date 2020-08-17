using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class Culture : ISynchronizable
{
    public Identifier LanguageId;

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
    public Dictionary<string, Discovery> Discoveries = new Dictionary<string, Discovery>();

    protected Dictionary<string, CulturalPreference> _preferences = new Dictionary<string, CulturalPreference>();
    protected Dictionary<string, CulturalActivity> _activities = new Dictionary<string, CulturalActivity>();
    protected Dictionary<string, CulturalSkill> _skills = new Dictionary<string, CulturalSkill>();
    protected Dictionary<string, CulturalKnowledge> _knowledges = new Dictionary<string, CulturalKnowledge>();

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

        foreach (Discovery d in sourceCulture.Discoveries.Values)
        {
            AddDiscovery(d);
        }
    }

    /// <summary>
    /// Adds a new preference if not already present to the ulture
    /// </summary>
    /// <param name="preference">The preference to try to add</param>
    public void AddPreference(CulturalPreference preference)
    {
        if (_preferences.ContainsKey(preference.Id))
            return;

        World.AddExistingCulturalPreferenceInfo(preference);

        _preferences.Add(preference.Id, preference);
    }

    /// <summary>
    /// Removes a preference from the culture
    /// </summary>
    /// <param name="preference">The preference to remove</param>
    public void RemovePreference(CulturalPreference preference)
    {
        if (!_preferences.ContainsKey(preference.Id))
            return;

        _preferences.Remove(preference.Id);
    }

    public void RemovePreference(string preferenceId)
    {
        CulturalPreference preference = GetPreference(preferenceId);

        if (preference == null)
            return;

        RemovePreference(preference);
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

    protected void AddKnowledge(CulturalKnowledge knowledge)
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

    protected void AddDiscovery(Discovery discovery)
    {
        if (Discoveries.ContainsKey(discovery.Id))
            return;

        World.AddExistingDiscovery(discovery);

        Discoveries.Add(discovery.Id, discovery);
    }

    protected void RemoveDiscovery(Discovery discovery)
    {
        if (!Discoveries.ContainsKey(discovery.Id))
            return;

        Discoveries.Remove(discovery.Id);
    }

    public void ResetDiscoveries()
    {
        Discoveries.Clear();
    }

    public ICollection<CulturalPreference> GetPreferences()
    {
        return _preferences.Values;
    }

    public CulturalPreference GetPreference(string id)
    {
        if (!_preferences.TryGetValue(id, out CulturalPreference preference))
            return null;

        return preference;
    }

    public ICollection<CulturalActivity> GetActivities()
    {
        return _activities.Values;
    }

    public CulturalActivity GetActivity(string id)
    {
        if (!_activities.TryGetValue(id, out CulturalActivity activity))
            return null;

        return activity;
    }

    public bool HasActivity(string id)
    {
        return _activities.ContainsKey(id);
    }

    public ICollection<CulturalSkill> GetSkills()
    {
        return _skills.Values;
    }

    public CulturalSkill GetSkill(string id)
    {
        if (!_skills.TryGetValue(id, out CulturalSkill skill))
            return null;

        return skill;
    }

    public bool TryGetSkillValue(string id, out float value)
    {
        value = 0;

        CulturalSkill skill = GetSkill(id);

        if (skill != null)
        {
            value = skill.Value;

            return true;
        }

        return false;
    }

    public ICollection<CulturalKnowledge> GetKnowledges()
    {
        return _knowledges.Values;
    }

    public CulturalKnowledge GetKnowledge(string id)
    {
        if (!_knowledges.TryGetValue(id, out CulturalKnowledge knowledge))
            return null;

        return knowledge;
    }

    public bool TryGetKnowledgeValue(string id, out int value)
    {
        value = 0;

        CulturalKnowledge knowledge = GetKnowledge(id);

        if (knowledge != null)
        {
            value = knowledge.Value;

            return true;
        }

        return false;
    }

    public bool TryGetKnowledgeScaledValue(string id, out float scaledValue)
    {
        scaledValue = 0;

        CulturalKnowledge knowledge = GetKnowledge(id);

        if (knowledge != null)
        {
            scaledValue = knowledge.ScaledValue;

            return true;
        }

        return false;
    }

    public bool HasKnowledge(string id)
    {
        CulturalKnowledge knowledge = GetKnowledge(id);

        if (knowledge != null)
            return true;

        return false;
    }

    public Discovery GetDiscovery(string id)
    {
        if (!Discoveries.TryGetValue(id, out Discovery discovery))
            return null;

        return discovery;
    }

    public bool HasDiscovery(string id)
    {
        Discovery discovery = GetDiscovery(id);

        if (discovery != null)
            return true;

        return false;
    }

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
            _preferences.Add(p.Id, p);
        }
    }

    public void LoadActivities()
    {
        foreach (CulturalActivity a in Activities)
        {
            _activities.Add(a.Id, a);
        }
    }

    public void LoadSkills()
    {
        foreach (CulturalSkill s in Skills)
        {
            _skills.Add(s.Id, s);
        }
    }

    public void LoadKnowledges()
    {
        foreach (CulturalKnowledge k in Knowledges)
        {
            _knowledges.Add(k.Id, k);
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
            Discovery discovery = Discovery.GetDiscovery(discoveryId);

            Discoveries.Add(discoveryId, discovery);
        }
    }
}
