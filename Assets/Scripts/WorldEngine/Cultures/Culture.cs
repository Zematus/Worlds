using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class Culture : ISynchronizable
{
    [XmlAttribute("LId")]
    public long LanguageId = -1;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public Language Language { get; protected set; }
    
    public XmlSerializableDictionary<string, CulturalPreference> Preferences = new XmlSerializableDictionary<string, CulturalPreference>();
    public XmlSerializableDictionary<string, CulturalActivity> Activities = new XmlSerializableDictionary<string, CulturalActivity>();
    public XmlSerializableDictionary<string, CulturalSkill> Skills = new XmlSerializableDictionary<string, CulturalSkill>();
    public XmlSerializableDictionary<string, CulturalKnowledge> Knowledges = new XmlSerializableDictionary<string, CulturalKnowledge>();
    public XmlSerializableDictionary<string, CulturalDiscovery> Discoveries = new XmlSerializableDictionary<string, CulturalDiscovery>();

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
        foreach (CulturalPreference p in sourceCulture.Preferences.Values)
        {
            AddPreference(new CulturalPreference(p));
        }

        foreach (CulturalActivity a in sourceCulture.Activities.Values)
        {
            AddActivity(new CulturalActivity(a));
        }

        foreach (CulturalSkill s in sourceCulture.Skills.Values)
        {
            AddSkill(new CulturalSkill(s));
        }

        foreach (CulturalKnowledge k in sourceCulture.Knowledges.Values)
        {
            AddKnowledge(new CulturalKnowledge(k));
        }

        foreach (CulturalDiscovery d in sourceCulture.Discoveries.Values)
        {
            if (d is Discovery)
            {
                AddDiscovery(d);
            }
            else
            {
                AddDiscovery(new CulturalDiscovery(d));
            }
        }
    }

    protected void AddPreference(CulturalPreference preference)
    {
        if (Preferences.ContainsKey(preference.Id))
            return;

        World.AddExistingCulturalPreferenceInfo(preference);
        
        Preferences.Add(preference.Id, preference);
    }

    protected void RemovePreference(CulturalPreference preference)
    {
        if (!Preferences.ContainsKey(preference.Id))
            return;
        
        Preferences.Remove(preference.Id);
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
        foreach (CulturalPreference preference in Preferences.Values)
        {
            preference.Reset();
        }
    }

    protected void AddActivity(CulturalActivity activity)
    {
        if (Activities.ContainsKey(activity.Id))
            return;

        World.AddExistingCulturalActivityInfo(activity);
        
        Activities.Add(activity.Id, activity);
    }

    protected void RemoveActivity(CulturalActivity activity)
    {
        if (!Activities.ContainsKey(activity.Id))
            return;
        
        Activities.Remove(activity.Id);
    }

    public void RemoveActivity(string activityId)
    {
        CulturalActivity activity = GetActivity(activityId);

        if (activity == null)
            return;

        RemoveActivity(activity);
    }

    public void ResetActivities()
    {
        foreach (CulturalActivity activity in Activities.Values)
        {
            activity.Reset();
        }
    }

    protected void AddSkill(CulturalSkill skill)
    {
        if (Skills.ContainsKey(skill.Id))
            return;

        World.AddExistingCulturalSkillInfo(skill);
        
        Skills.Add(skill.Id, skill);
    }

    protected void RemoveSkill(CulturalSkill skill)
    {
        if (!Skills.ContainsKey(skill.Id))
            return;
        
        Skills.Remove(skill.Id);
    }

    public void ResetSkills()
    {
        foreach (CulturalSkill skill in Skills.Values)
        {
            skill.Reset();
        }
    }

    protected void AddKnowledge(CulturalKnowledge knowledge)
    {
        if (Knowledges.ContainsKey(knowledge.Id))
            return;

        World.AddExistingCulturalKnowledgeInfo(knowledge);

        Knowledges.Add(knowledge.Id, knowledge);
    }

    protected void RemoveKnowledge(CulturalKnowledge knowledge)
    {
        if (!Knowledges.ContainsKey(knowledge.Id))
            return;

        Knowledges.Remove(knowledge.Id);
    }

    public void ResetKnowledges()
    {
        foreach (CulturalKnowledge knowledge in Knowledges.Values)
        {
            knowledge.Reset();
        }
    }

    protected void AddDiscovery(CulturalDiscovery discovery)
    {
        if (Discoveries.ContainsKey(discovery.Id))
            return;

        World.AddExistingCulturalDiscoveryInfo(discovery);

        Discoveries.Add(discovery.Id, discovery);
    }

    protected void RemoveDiscovery(CulturalDiscovery discovery)
    {
        if (!Discoveries.ContainsKey(discovery.Id))
            return;

        Discoveries.Remove(discovery.Id);
    }

    public CulturalPreference GetPreference(string id)
    {
        CulturalPreference preference = null;

        if (!Preferences.TryGetValue(id, out preference))
            return null;

        return preference;
    }

    public CulturalActivity GetActivity(string id)
    {
        CulturalActivity activity = null;

        if (!Activities.TryGetValue(id, out activity))
            return null;

        return activity;
    }

    public CulturalSkill GetSkill(string id)
    {
        CulturalSkill skill = null;

        if (!Skills.TryGetValue(id, out skill))
            return null;

        return skill;
    }

    public CulturalKnowledge GetKnowledge(string id)
    {
        CulturalKnowledge knowledge = null;

        if (!Knowledges.TryGetValue(id, out knowledge))
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

    public CulturalDiscovery GetDiscovery(string id)
    {
        CulturalDiscovery discovery = null;

        if (!Discoveries.TryGetValue(id, out discovery))
            return null;

        return discovery;
    }

    public bool HasDiscovery(string id)
    {
        CulturalDiscovery discovery = GetDiscovery(id);
        
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
        //ResetDiscoveries();
    }

    public virtual void Synchronize()
    {
        if (Language != null)
            LanguageId = Language.Id;
    }

    public virtual void FinalizeLoad()
    {
        if (LanguageId != -1)
        {
            Language = World.GetLanguage(LanguageId);
        }

        FinalizePropertiesLoad();
    }

    public virtual void FinalizePropertiesLoad()
    {
        foreach (CulturalPreference p in Preferences.Values)
        {
            p.FinalizeLoad();
        }

        foreach (CulturalActivity a in Activities.Values)
        {
            a.FinalizeLoad();
        }

        foreach (CulturalSkill s in Skills.Values)
        {
            s.FinalizeLoad();
        }

        foreach (CulturalKnowledge k in Knowledges.Values)
        {
            k.FinalizeLoad();
        }

        foreach (CulturalDiscovery d in Discoveries.Values)
        {
            d.FinalizeLoad();
        }
    }
}

public class BufferCulture : Culture
{
    public BufferCulture()
    {
    }

    public BufferCulture(Culture sourceCulture) : base(sourceCulture)
    {
    }
}
