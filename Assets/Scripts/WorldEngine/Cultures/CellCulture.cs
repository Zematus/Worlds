using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class CellCulture : Culture
{
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
    public Dictionary<string, Discovery> DiscoveriesToFind = new Dictionary<string, Discovery>();

    // DiscoveriesToReceive should only be used when the discovery is gotten through a transfer from other groups or polities
    [XmlIgnore]
    public Dictionary<string, Discovery> DiscoveriesToReceive = new Dictionary<string, Discovery>();

    private HashSet<CellCulturalPreference> _preferencesToLose = new HashSet<CellCulturalPreference>();
    private HashSet<CellCulturalActivity> _activitiesToStop = new HashSet<CellCulturalActivity>();
    private HashSet<CellCulturalSkill> _skillsToLose = new HashSet<CellCulturalSkill>();
    private HashSet<CellCulturalKnowledge> _knowledgesToLose = new HashSet<CellCulturalKnowledge>();
    private HashSet<Discovery> _discoveriesToLose = new HashSet<Discovery>();

    private HashSet<string> _propertiesToAquire = new HashSet<string>();
    private HashSet<string> _propertiesToLose = new HashSet<string>();

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

        foreach (Discovery d in sourceCulture.Discoveries.Values)
        {
            AddDiscovery(d);
        }

        foreach (CulturalKnowledge k in sourceCulture.Knowledges.Values)
        {
            //#if DEBUG
            //                if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
            //                {
            //                    if (Group.Id == Manager.TracingData.GroupId)
            //                    {
            //                        string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

            //                        SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
            //                            "PostUpdateAddAttributes - Group:" + groupId,
            //                            "CurrentDate: " + World.CurrentDate +
            //                            ", k.Id: " + k.Id +
            //                            ", k.IsPresent: " + k.IsPresent +
            //                            "");

            //                        Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            //                    }
            //                }
            //#endif

            CellCulturalKnowledge knowledge = CellCulturalKnowledge.CreateCellInstance(k.Id, group, k.Value, k.Limit);

            AddKnowledge(knowledge);
        }

        if (sourceCulture.Language != null)
        {
            SetLanguageUpdateCells();
        }

        foreach (string property in sourceCulture.GetProperties())
        {
            AddProperty(property);
        }
    }

    public void SetLanguageUpdateCells()
    {
        if (!Manager.ValidUpdateTypeAndSubtype(CellUpdateType.Language, CellUpdateSubType.Membership))
            return;

        Manager.AddUpdatedCell(Group.Cell);

        foreach (CellGroup nGroup in Group.NeighborGroups)
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

    public void AddPropertyToAquire(string property)
    {
        _propertiesToAquire.Add(property);
    }

    public void AddPropertyToLose(string property)
    {
        _propertiesToLose.Add(property);
    }

    public CellCulturalKnowledge TryAddKnowledgeToLearn(string id, int initialValue, int initialLimit = -1)
    {
        CellCulturalKnowledge knowledge = GetKnowledge(id) as CellCulturalKnowledge;
        
        if (knowledge != null)
        {
            knowledge.SetLevelLimit(initialLimit);
            return knowledge;
        }

        CellCulturalKnowledge tempKnowledge;

        if (KnowledgesToLearn.TryGetValue(id, out tempKnowledge))
        {
            tempKnowledge.SetLevelLimit(initialLimit);
            return tempKnowledge;
        }

        if (knowledge == null)
        {
            knowledge = CellCulturalKnowledge.CreateCellInstance(id, Group, initialValue, initialLimit);
        }

        KnowledgesToLearn.Add(id, knowledge);

        return knowledge;
    }

    public void AddDiscoveryToFind(Discovery discovery)
    {
        if (Discoveries.ContainsKey(discovery.Id))
        {
            throw new System.Exception("CellCulture: Discoveries already contains " + discovery.Id);
        }

        if (DiscoveriesToFind.ContainsKey(discovery.Id))
        {
            throw new System.Exception("CellCulture: DiscoveriesToFind already contains " + discovery.Id);
        }
        
        DiscoveriesToFind.Add(discovery.Id, discovery);
    }

    public bool TryReceiveDiscovery(Discovery d)
    {
        if (Discoveries.ContainsKey(d.Id))
        {
            return false;
        }
        
        if (DiscoveriesToFind.ContainsKey(d.Id))
        {
            return false;
        }

        if (DiscoveriesToReceive.ContainsKey(d.Id))
        {
            return false;
        }
        
        DiscoveriesToReceive.Add(d.Id, d);

        return true;
    }

    public CellCulturalPreference GetAcquiredPreferenceOrToAcquire(string id)
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

    public bool HasOrWillHaveDiscovery(string id)
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
            CellCulturalPreference preference = GetAcquiredPreferenceOrToAcquire(p.Id);

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
            CellCulturalKnowledge knowledge = TryAddKnowledgeToLearn(k.Id, 0, k.Limit);
            knowledge.Merge(k.Value, percentage);
        }

        foreach (Discovery d in sourceCulture.Discoveries.Values)
        {
            TryReceiveDiscovery(d);
        }
        
        foreach (string property in sourceCulture.GetProperties())
        {
            AddPropertyToAquire(property);
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
            CellCulturalPreference cellPreference = GetAcquiredPreferenceOrToAcquire(polityPreference.Id);

            if (cellPreference == null)
            {
                cellPreference = CellCulturalPreference.CreateCellInstance(Group, polityPreference, 0);
                AddPreferenceToAcquire(cellPreference);
            }

            cellPreference.AddPolityProminenceEffect(polityPreference, polityProminence, timeSpan);
        }

        foreach (CulturalActivity polityActivity in polityCulture.Activities.Values)
        {
            CellCulturalActivity cellActivity = GetPerformedActivityOrToPerform(polityActivity.Id);

            if (cellActivity == null)
            {
                cellActivity = CellCulturalActivity.CreateCellInstance(Group, polityActivity, 0);
                AddActivityToPerform(cellActivity);
            }

            cellActivity.AddPolityProminenceEffect(polityActivity, polityProminence, timeSpan);
        }

        foreach (CulturalSkill politySkill in polityCulture.Skills.Values)
        {
            CellCulturalSkill cellSkill = GetLearnedSkillOrToLearn(politySkill.Id);

            if (cellSkill == null)
            {
                cellSkill = CellCulturalSkill.CreateCellInstance(Group, politySkill, 0);
                AddSkillToLearn(cellSkill);
            }

            cellSkill.AddPolityProminenceEffect(politySkill, polityProminence, timeSpan);
        }

        foreach (CulturalKnowledge polityKnowledge in polityCulture.Knowledges.Values)
        {
//#if DEBUG
//            if (Manager.RegisterDebugEvent != null)
//            {
//                if (Manager.TracingData.Priority <= 0)
//                {
//                    if (Group.Id == Manager.TracingData.GroupId)
//                    {
//                        string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

//                        SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                            "CellCulture.UpdatePolityCulturalProminence - Group:" + groupId,
//                            "CurrentDate: " + Group.World.CurrentDate +
//                            ", polityCulture.Polity.Id: " + polityCulture.Polity.Id +
//                            ", polityKnowledge.Name: " + polityKnowledge.Name +
//                            "", Group.World.CurrentDate);

//                        Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//                    }
//                }
//            }
//#endif
            
            CellCulturalKnowledge cellKnowledge = TryAddKnowledgeToLearn(polityKnowledge.Id, 0, polityKnowledge.Limit);

            cellKnowledge.AddPolityProminenceEffect(polityKnowledge, polityProminence, timeSpan);
        }

        foreach (Discovery polityDiscovery in polityCulture.Discoveries.Values)
        {
            Discovery discovery = Discovery.GetDiscovery(polityDiscovery.Id);

            TryReceiveDiscovery(discovery);
        }

        foreach (string property in polityCulture.GetProperties())
        {
            AddPropertyToAquire(property);
        }
    }

    public void PostUpdatePolityCulturalProminence(PolityProminence polityProminence)
    {
        PolityCulture polityCulture = polityProminence.Polity.Culture;

        if (Group.HighestPolityProminence == null)
        {
            throw new System.Exception("HighestPolityProminence is null");
        }

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
        // We need to handle discoveries before anything else as they might trigger removal of other cultural attributes
        foreach (Discovery d in _discoveriesToLose)
        {
            RemoveDiscovery(d);
            d.OnLoss(Group);
        }

        foreach (CellCulturalPreference p in _preferencesToLose)
        {
            RemovePreference(p);
        }
        
        foreach (CellCulturalActivity a in _activitiesToStop)
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
        }

        foreach (string property in _propertiesToLose)
        {
            RemoveProperty(property);
        }

        // This should be done only after knowledges have been removed as there are some dependencies
        foreach (Discovery d in _discoveriesToLose)
        {
            d.RetryAssignAfterLoss(Group);
        }

        _preferencesToLose.Clear();
        _activitiesToStop.Clear();
        _skillsToLose.Clear();
        _knowledgesToLose.Clear();
        _discoveriesToLose.Clear();
        _propertiesToLose.Clear();
    }

    public void PostUpdateAddAttributes()
    {
        // We need to handle discoveries before everything else as these can add other type of cultural attributes
        foreach (Discovery discovery in DiscoveriesToFind.Values)
        {
            AddDiscovery(discovery);
            discovery.OnGain(Group);
        }

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

        foreach (Discovery discovery in DiscoveriesToReceive.Values)
        {
            AddDiscovery(discovery);
        }

        foreach (CellCulturalKnowledge knowledge in KnowledgesToLearn.Values)
        {
            try
            {
//#if DEBUG
//                if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//                {
//                    if (Group.Id == Manager.TracingData.GroupId)
//                    {
//                        string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

//                        SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                            "PostUpdateAddAttributes - Group:" + groupId,
//                            "CurrentDate: " + World.CurrentDate +
//                            ", knowledge.Id: " + knowledge.Id +
//                            "");

//                        Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//                    }
//                }
//#endif

                AddKnowledge(knowledge);
            }
            catch (System.ArgumentException)
            {
                throw new System.Exception("Attempted to add duplicate knowledge (" + knowledge.Id + ") to group " + Group.Id);
            }
        }

        foreach (string property in _propertiesToAquire)
        {
            AddProperty(property);
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
//#if DEBUG
//            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//            {
//                if (Group.Id == Manager.TracingData.GroupId)
//                {
//                    string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

//                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                        "CellCulture.PostUpdateAttributeValues before PostUpdate() - Group:" + groupId,
//                        "CurrentDate: " + World.CurrentDate +
//                        ", knowledge.Id: " + knowledge.Id +
//                        ", knowledge.IsPresent: " + knowledge.IsPresent +
//                        ", knowledge.Value: " + knowledge.Value +
//                        "");

//                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//                }
//            }
//#endif

            knowledge.PostUpdate();

//#if DEBUG
//            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//            {
//                if (Group.Id == Manager.TracingData.GroupId)
//                {
//                    string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

//                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                        "CellCulture.PostUpdateAttributeValues before WillBeLost() - Group:" + groupId,
//                        "CurrentDate: " + World.CurrentDate +
//                        ", knowledge.Id: " + knowledge.Id +
//                        ", knowledge.IsPresent: " + knowledge.IsPresent +
//                        ", knowledge.Value: " + knowledge.Value +
//                        "");

//                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//                }
//            }
//#endif

            if (!knowledge.WillBeLost())
                continue;

//#if DEBUG
//            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//            {
//                if (Group.Id == Manager.TracingData.GroupId)
//                {
//                    string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

//                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                        "CellCulture.PostUpdateAttributeValues after WillBeLost() - Group:" + groupId,
//                        "CurrentDate: " + World.CurrentDate +
//                        ", knowledge.Id: " + knowledge.Id +
//                        ", knowledge.IsPresent: " + knowledge.IsPresent +
//                        ", knowledge.Value: " + knowledge.Value +
//                        "");

//                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//                }
//            }
//#endif

            _knowledgesToLose.Add(knowledge);
        }

        foreach (Discovery discovery in Discoveries.Values)
        {
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

    public void CleanUpAtributesToGet()
    {
        PreferencesToAcquire.Clear();
        ActivitiesToPerform.Clear();
        SkillsToLearn.Clear();
        KnowledgesToLearn.Clear();
        DiscoveriesToFind.Clear();
        DiscoveriesToReceive.Clear();
        _propertiesToAquire.Clear();
    }

    public void AddKnowledgeToLose(string knowledgeId)
    {
        CulturalKnowledge knowledge = null;

        if (!Knowledges.TryGetValue(knowledgeId, out knowledge))
        {
            Debug.LogWarning("CellCulture: Trying to remove knowledge that is not present: " + knowledgeId);

            return;
        }

        _knowledgesToLose.Add(knowledge as CellCulturalKnowledge);
    }

    public void AddActivityToStop(string activityId)
    {
        CulturalActivity activity = null;

        if (!Activities.TryGetValue(activityId, out activity))
        {
            Debug.LogWarning("CellCulture: Trying to remove activity that is not present: " + activityId);

            return;
        }

        _activitiesToStop.Add(activity as CellCulturalActivity);
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
//#if DEBUG
//            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//            {
//                if (Group.Id == Manager.TracingData.GroupId)
//                {
//                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                        "CellCulture.MinimumKnowledgeProgressLevel - knowledge.Id:" + knowledge.Id + ", Group.Id:" + Group.Id,
//                        "CurrentDate: " + Group.World.CurrentDate +
//                        ", knowledge.IsPresent: " + knowledge.IsPresent +
//                        //", knowledge.WasPresent: " + knowledge.WasPresent +
//                        "");

//                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//                }
//            }
//#endif

            float level = knowledge.CalculateExpectedProgressLevel();

            if (level < minProgressLevel)
            {
                minProgressLevel = level;
            }

//#if DEBUG
//            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//            {
//                if (Group.Id == Manager.TracingData.GroupId)
//                {
//                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                        "CellCulture.MinimumKnowledgeProgressLevel - knowledge.Id:" + knowledge.Id + ", Group.Id:" + Group.Id,
//                        "CurrentDate: " + Group.World.CurrentDate +
//                        ", knowledge.CalculateExpectedProgressLevel(): " + level +
//                        //", minProgressLevel: " + minProgressLevel +
//                        "");

//                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//                }
//            }
//#endif
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
