using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class CellCulture : Culture
{
    public const float MinKnowledgeValue = 1f;
    //	public const float BaseKnowledgeTransferFactor = 0.1f;

    [XmlIgnore]
    public CellGroup Group;

    [XmlIgnore]
    public Dictionary<string, CellCulturalPreference> PreferencesToAcquire = new Dictionary<string, CellCulturalPreference>();
    [XmlIgnore]
    public Dictionary<string, CellCulturalActivity> ActivitiesToPerform = new Dictionary<string, CellCulturalActivity>();
    [XmlIgnore]
    public Dictionary<string, CellCulturalSkill> SkillsToLearn = new Dictionary<string, CellCulturalSkill>();
    [XmlIgnore]
    public Dictionary<string, CellCulturalKnowledge> KnowledgesToLearn = new Dictionary<string, CellCulturalKnowledge>();
    [XmlIgnore]
    public Dictionary<string, CellCulturalDiscovery> DiscoveriesToFind = new Dictionary<string, CellCulturalDiscovery>();

    private HashSet<CellCulturalPreference> _preferencesToLose = new HashSet<CellCulturalPreference>();
    private HashSet<CellCulturalActivity> _activitiesToLose = new HashSet<CellCulturalActivity>();
    private HashSet<CellCulturalSkill> _skillsToLose = new HashSet<CellCulturalSkill>();
    private HashSet<CellCulturalKnowledge> _knowledgesToLose = new HashSet<CellCulturalKnowledge>();
    private HashSet<CellCulturalDiscovery> _discoveriesToLose = new HashSet<CellCulturalDiscovery>();

    public CellCulture()
    {
    }

    public CellCulture(CellGroup group) : base(group.World)
    {
        Group = group;
    }

    public CellCulture(CellGroup group, Culture sourceCulture) : base(group.World, sourceCulture.Language)
    {
        Group = group;

        foreach (CulturalPreference p in sourceCulture.Preferences.Values)
        {
            AddPreference(CellCulturalPreference.CreateCellInstance(group, p));
        }

        foreach (CulturalActivity a in sourceCulture.Activities.Values)
        {
            AddActivity(CellCulturalActivity.CreateCellInstance(group, a));
        }

        foreach (CulturalSkill s in sourceCulture.Skills.Values)
        {
            AddSkill(CellCulturalSkill.CreateCellInstance(group, s));
        }

        foreach (CulturalKnowledge k in sourceCulture.Knowledges.Values)
        {
            if (k.IsPresent)
            {
                CellCulturalKnowledge knowledge = CellCulturalKnowledge.CreateCellInstance(k.Id, group, k.Value);

                AddKnowledge(knowledge);

                knowledge.CalculateAsymptote();
            }
        }

        foreach (CulturalDiscovery d in sourceCulture.Discoveries.Values)
        {
            if (d.IsPresent)
            {
                AddDiscovery(CellCulturalDiscovery.CreateCellInstance(d.Id));

                foreach (CellCulturalKnowledge knowledge in Knowledges.Values)
                {
                    knowledge.CalculateAsymptote(d);
                }
            }
        }

        if (sourceCulture.Language != null)
        {
            SetLanguageUpdateCells();
        }
    }

    public void SetLanguageUpdateCells()
    {
        if (!Manager.ValidUpdateTypeAndSubtype(CellUpdateType.Language, CellUpdateSubType.Membership))
            return;

        Manager.AddUpdatedCell(Group.Cell);

        foreach (CellGroup nGroup in Group.Neighbors.Values)
        {
            Manager.AddUpdatedCell(nGroup.Cell);
        }
    }

    public void AddPreferenceToAcquire(CellCulturalPreference preference)
    {
        if (PreferencesToAcquire.ContainsKey(preference.Id))
            return;

        PreferencesToAcquire.Add(preference.Id, preference);
    }

    public void AddActivityToPerform(CellCulturalActivity activity)
    {
        if (ActivitiesToPerform.ContainsKey(activity.Id))
            return;

        ActivitiesToPerform.Add(activity.Id, activity);
    }

    public void AddSkillToLearn(CellCulturalSkill skill)
    {
        if (SkillsToLearn.ContainsKey(skill.Id))
            return;

        SkillsToLearn.Add(skill.Id, skill);
    }

    public CellCulturalKnowledge TryAddKnowledgeToLearn(string id, CellGroup group, int initialValue = 0)
    {
        CellCulturalKnowledge knowledge = GetKnowledge(id) as CellCulturalKnowledge;

        if ((knowledge != null) && knowledge.IsPresent)
        {
            return knowledge;
        }

        CellCulturalKnowledge tempKnowledge;

        if (KnowledgesToLearn.TryGetValue(id, out tempKnowledge))
        {
            return tempKnowledge;
        }

        if (knowledge == null)
        {
            knowledge = CellCulturalKnowledge.CreateCellInstance(id, group);
        }

        knowledge.SetInitialValue(initialValue);

        KnowledgesToLearn.Add(id, knowledge);

        return knowledge;
    }

    public CellCulturalDiscovery TryAddDiscoveryToFind(string id)
    {
        CellCulturalDiscovery discovery = GetDiscovery(id) as CellCulturalDiscovery;

        if ((discovery != null) && discovery.IsPresent)
        {
            return discovery;
        }

        CellCulturalDiscovery tempDiscovery;

        if (DiscoveriesToFind.TryGetValue(id, out tempDiscovery))
        {
            return tempDiscovery;
        }

        if (discovery == null)
        {
            discovery = CellCulturalDiscovery.CreateCellInstance(id);
        }

        DiscoveriesToFind.Add(id, discovery);

        return discovery;
    }

    public CellCulturalPreference GetAcquiredPerferenceOrToAcquire(string id)
    {
        CellCulturalPreference preference = GetPreference(id) as CellCulturalPreference;

        if (preference != null)
            return preference;

        if (PreferencesToAcquire.TryGetValue(id, out preference))
            return preference;

        return null;
    }

    public CellCulturalActivity GetPerformedActivityOrToPerform(string id)
    {
        CellCulturalActivity activity = GetActivity(id) as CellCulturalActivity;

        if (activity != null)
            return activity;

        if (ActivitiesToPerform.TryGetValue(id, out activity))
            return activity;

        return null;
    }

    public CellCulturalSkill GetLearnedSkillOrToLearn(string id)
    {
        CellCulturalSkill skill = GetSkill(id) as CellCulturalSkill;

        if (skill != null)
            return skill;

        if (SkillsToLearn.TryGetValue(id, out skill))
            return skill;

        return null;
    }

    public bool HasOrWillHaveKnowledge(string id)
    {
        return HasKnowledge(id) | KnowledgesToLearn.ContainsKey(id);
    }

    public bool HasrWillHaveDiscovery(string id)
    {
        return HasDiscovery(id) | DiscoveriesToFind.ContainsKey(id);
    }

    public void MergeCulture(Culture sourceCulture, float percentage)
    {
#if DEBUG
        if ((percentage < 0) || (percentage > 1))
        {
            Debug.LogWarning("percentage value outside the [0,1] range");
        }
#endif

        foreach (CulturalPreference p in sourceCulture.Preferences.Values)
        {
            CellCulturalPreference preference = GetAcquiredPerferenceOrToAcquire(p.Id);

            if (preference == null)
            {
                preference = CellCulturalPreference.CreateCellInstance(Group, p);
                preference.DecreaseValue(percentage);

                AddPreferenceToAcquire(preference);
            }
            else
            {
                preference.Merge(p, percentage);
            }
        }

        foreach (CulturalActivity a in sourceCulture.Activities.Values)
        {
            CellCulturalActivity activity = GetPerformedActivityOrToPerform(a.Id);

            if (activity == null)
            {
                activity = CellCulturalActivity.CreateCellInstance(Group, a);
                activity.DecreaseValue(percentage);

                AddActivityToPerform(activity);
            }
            else
            {
                activity.Merge(a, percentage);
            }
        }

        foreach (CulturalSkill s in sourceCulture.Skills.Values)
        {
            CellCulturalSkill skill = GetLearnedSkillOrToLearn(s.Id);

            if (skill == null)
            {
                skill = CellCulturalSkill.CreateCellInstance(Group, s);
                skill.DecreaseValue(percentage);

                AddSkillToLearn(skill);
            }
            else
            {
                skill.Merge(s, percentage);
            }
        }

        foreach (CulturalKnowledge k in sourceCulture.Knowledges.Values)
        {
            if (!k.IsPresent) continue;
            
            CellCulturalKnowledge knowledge = TryAddKnowledgeToLearn(k.Id, Group);
            knowledge.Merge(k.Value, percentage);
        }

        foreach (CulturalDiscovery d in sourceCulture.Discoveries.Values)
        {
            if (!d.IsPresent) continue;

            TryAddDiscoveryToFind(d.Id);
        }
    }

    public void Update(long timeSpan)
    {
        foreach (CellCulturalPreference preference in Preferences.Values)
        {
            preference.Update(timeSpan);
        }

        foreach (CellCulturalActivity activity in Activities.Values)
        {
            activity.Update(timeSpan);
        }

        foreach (CellCulturalSkill skill in Skills.Values)
        {
            skill.Update(timeSpan);
        }

        foreach (CellCulturalKnowledge knowledge in Knowledges.Values)
        {
            knowledge.Update(timeSpan);
        }
    }

    public void UpdatePolityCulturalProminence(PolityProminence polityProminence, long timeSpan)
    {
        PolityCulture polityCulture = polityProminence.Polity.Culture;

        foreach (CulturalPreference polityPreference in polityCulture.Preferences.Values)
        {
            CellCulturalPreference cellPreference = GetAcquiredPerferenceOrToAcquire(polityPreference.Id);

            if (cellPreference == null)
            {
                cellPreference = CellCulturalPreference.CreateCellInstance(Group, polityPreference, 0);
                AddPreferenceToAcquire(cellPreference);
            }

            cellPreference.PolityCulturalProminence(polityPreference, polityProminence, timeSpan);
        }

        foreach (CulturalActivity polityActivity in polityCulture.Activities.Values)
        {
            CellCulturalActivity cellActivity = GetPerformedActivityOrToPerform(polityActivity.Id);

            if (cellActivity == null)
            {
                cellActivity = CellCulturalActivity.CreateCellInstance(Group, polityActivity, 0);
                AddActivityToPerform(cellActivity);
            }

            cellActivity.PolityCulturalProminence(polityActivity, polityProminence, timeSpan);
        }

        foreach (CulturalSkill politySkill in polityCulture.Skills.Values)
        {
            CellCulturalSkill cellSkill = GetLearnedSkillOrToLearn(politySkill.Id);

            if (cellSkill == null)
            {
                cellSkill = CellCulturalSkill.CreateCellInstance(Group, politySkill, 0);
                AddSkillToLearn(cellSkill);
            }

            cellSkill.PolityCulturalProminence(politySkill, polityProminence, timeSpan);
        }

        foreach (CulturalKnowledge polityKnowledge in polityCulture.Knowledges.Values)
        {
            if (!polityKnowledge.IsPresent) continue;
            
            CellCulturalKnowledge cellKnowledge = TryAddKnowledgeToLearn(polityKnowledge.Id, Group);

            cellKnowledge.PolityCulturalProminence(polityKnowledge, polityProminence, timeSpan);
        }

        foreach (CulturalDiscovery polityDiscovery in polityCulture.Discoveries.Values)
        {
            if (!polityDiscovery.IsPresent) continue;

            TryAddDiscoveryToFind(polityDiscovery.Id);
        }
    }

    public void PostUpdatePolityCulturalProminence(PolityProminence polityProminence)
    {
        PolityCulture polityCulture = polityProminence.Polity.Culture;

        if (((Language == null) ||
            (polityProminence.Value >= Group.HighestPolityProminence.Value)) &&
            (Language != polityCulture.Language))
        {
            Language = polityCulture.Language;
            SetLanguageUpdateCells();
        }
    }

    public void PostUpdateRemoveAttributes()
    {
        bool discoveriesLost = false;

        foreach (CellCulturalPreference p in _preferencesToLose)
        {
            RemovePreference(p);
        }

        foreach (CellCulturalActivity a in _activitiesToLose)
        {
            RemoveActivity(a);
        }

        foreach (CellCulturalSkill s in _skillsToLose)
        {
            RemoveSkill(s);
        }

        foreach (CellCulturalKnowledge k in _knowledgesToLose)
        {
            RemoveKnowledge(k);
            k.LossConsequences();
        }

        foreach (CellCulturalDiscovery d in _discoveriesToLose)
        {
            RemoveDiscovery(d);
            d.LossConsequences(Group);
            discoveriesLost = true;
        }

        if (discoveriesLost)
        {
            foreach (CellCulturalKnowledge knowledge in Knowledges.Values)
            {
                (knowledge as CellCulturalKnowledge).RecalculateAsymptote();
            }
        }

        _preferencesToLose.Clear();
        _activitiesToLose.Clear();
        _skillsToLose.Clear();
        _knowledgesToLose.Clear();
        _discoveriesToLose.Clear();
    }

    public void PostUpdateAddAttributes()
    {
        foreach (CellCulturalPreference preference in PreferencesToAcquire.Values)
        {
            AddPreference(preference);
        }

        foreach (CellCulturalActivity activity in ActivitiesToPerform.Values)
        {
            AddActivity(activity);
        }

        foreach (CellCulturalSkill skill in SkillsToLearn.Values)
        {
            AddSkill(skill);
        }

        foreach (CellCulturalKnowledge knowledge in KnowledgesToLearn.Values)
        {
            try
            {
                AddKnowledge(knowledge);
            }
            catch (System.ArgumentException)
            {
                throw new System.Exception("Attempted to add duplicate knowledge (" + knowledge.Id + ") to group " + Group.Id);
            }

            knowledge.RecalculateAsymptote();
        }

        foreach (CellCulturalDiscovery discovery in DiscoveriesToFind.Values)
        {
            bool setAsPresent = discovery.CanBeHeld(Group);

            try
            {
                AddDiscovery(discovery, setAsPresent);
            }
            catch (System.ArgumentException)
            {
                throw new System.Exception("Attempted to add duplicate discovery (" + discovery.Id + ") to group " + Group.Id);
            }

            if (!setAsPresent) continue;

            discovery.GainConsequences(Group);

            foreach (CellCulturalKnowledge knowledge in Knowledges.Values)
            {
                knowledge.CalculateAsymptote(discovery);
            }
        }
    }

    public void PostUpdateAttributeValues()
    {
        foreach (CellCulturalPreference preference in Preferences.Values)
        {
            preference.PostUpdate();
        }

        float totalActivityValue = 0;

        foreach (CellCulturalActivity activity in Activities.Values)
        {
            activity.PostUpdate();
            totalActivityValue += activity.Value;
        }

        foreach (CellCulturalActivity activity in Activities.Values)
        {
            if (totalActivityValue > 0)
            {
                activity.Contribution = activity.Value / totalActivityValue;
            }
            else
            {
                activity.Contribution = 1f / Activities.Count;
            }
        }

        foreach (CellCulturalSkill skill in Skills.Values)
        {
            skill.PostUpdate();
        }

        foreach (CellCulturalKnowledge knowledge in Knowledges.Values)
        {
            if (!knowledge.IsPresent)
                continue;

            knowledge.PostUpdate();

            if (!knowledge.WillBeLost())
                continue;

            _knowledgesToLose.Add(knowledge);
        }

        foreach (CellCulturalDiscovery discovery in Discoveries.Values)
        {
            if (!discovery.IsPresent)
                continue;

            if (discovery.CanBeHeld(Group))
                continue;

            _discoveriesToLose.Add(discovery);
        }
    }

    public void PostUpdate()
    {
        PostUpdateAddAttributes();

        PostUpdateAttributeValues();

        PostUpdateRemoveAttributes();
    }

    public void UpdateFactionCulture(FactionCulture factionCulture)
    {
        foreach (CellCulturalDiscovery d in DiscoveriesToFind.Values)
        {
            factionCulture.AddCoreDiscovery(d);
        }
    }

    public void CleanUpAtributesToGet()
    {
        PreferencesToAcquire.Clear();
        ActivitiesToPerform.Clear();
        SkillsToLearn.Clear();
        KnowledgesToLearn.Clear();
        DiscoveriesToFind.Clear();
    }

    public float MinimumSkillAdaptationLevel()
    {
        float minAdaptationLevel = 1f;

        foreach (CellCulturalSkill skill in Skills.Values)
        {
            float level = skill.AdaptationLevel;

            if (level < minAdaptationLevel)
            {
                minAdaptationLevel = level;
            }
        }

        return minAdaptationLevel;
    }

    public float MinimumKnowledgeProgressLevel()
    {
        float minProgressLevel = 1f;

        foreach (CellCulturalKnowledge knowledge in Knowledges.Values)
        {
            float level = knowledge.CalculateExpectedProgressLevel();

            if (level < minProgressLevel)
            {
                minProgressLevel = level;
            }
        }

        return minProgressLevel;
    }

    public override void Synchronize()
    {
        foreach (CellCulturalSkill s in Skills.Values)
        {
            s.Synchronize();
        }

        foreach (CellCulturalKnowledge k in Knowledges.Values)
        {
            k.Synchronize();
        }

        base.Synchronize();
    }

    public override void FinalizePropertiesLoad()
    {
        foreach (CellCulturalPreference p in Preferences.Values)
        {
            p.Group = Group;
        }

        foreach (CellCulturalActivity a in Activities.Values)
        {
            a.Group = Group;
        }

        foreach (CellCulturalSkill s in Skills.Values)
        {
            s.Group = Group;
        }

        foreach (CellCulturalKnowledge k in Knowledges.Values)
        {
            k.Group = Group;
        }

        base.FinalizePropertiesLoad();
    }
}
