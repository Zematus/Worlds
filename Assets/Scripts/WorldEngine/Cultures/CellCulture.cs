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

    private HashSet<CellCulturalPreference> _preferencesToLose = new HashSet<CellCulturalPreference>();
    private HashSet<CellCulturalActivity> _activitiesToStop = new HashSet<CellCulturalActivity>();
    private HashSet<CellCulturalSkill> _skillsToLose = new HashSet<CellCulturalSkill>();
    private HashSet<CellCulturalKnowledge> _knowledgesToLose = new HashSet<CellCulturalKnowledge>();
    private HashSet<Discovery> _discoveriesToLose = new HashSet<Discovery>();

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

        foreach (CulturalPreference p in sourceCulture.GetPreferences())
        {
            AddPreference(CellCulturalPreference.CreateCellInstance(group, p));
        }

        foreach (CulturalActivity a in sourceCulture.GetActivities())
        {
            AddActivity(CellCulturalActivity.CreateCellInstance(group, a));
        }

        foreach (CulturalSkill s in sourceCulture.GetSkills())
        {
            AddSkill(CellCulturalSkill.CreateCellInstance(group, s));
        }

        foreach (Discovery d in sourceCulture.Discoveries.Values)
        {
            AddDiscovery(d);
        }

        foreach (CulturalKnowledge k in sourceCulture.GetKnowledges())
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

            CellCulturalKnowledge knowledge = CellCulturalKnowledge.CreateCellInstance(k.Id, group, k.Value);

            AddKnowledge(knowledge);
        }

        if (sourceCulture.Language != null)
        {
            SetLanguageUpdateCells();
        }
    }

    public void Initialize()
    {
        foreach (Discovery d in Discoveries.Values)
        {
            d.OnGain(Group);
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
            return;

        if (DiscoveriesToFind.ContainsKey(discovery.Id))
            return;

        DiscoveriesToFind.Add(discovery.Id, discovery);
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

    /// <summary>
    /// Removes the influence from a reference culture from this culture
    /// </summary>
    /// <param name="referenceCulture">culture with properties to unmerge</param>
    /// <param name="percentage">how much to 'unmerge'</param>
    public void UnmergeCulture(Culture referenceCulture, float percentage)
    {
        if (percentage == 1)
        {
            // It's technically impossible to 'unmerge' a culture by 100% because that
            // would mean the cell was completely abandoned. And this call shouldn't
            // have happened in that scenario. Also, trying to unmerge property values
            // by 100% will generate NaN values.

            Debug.LogWarning("Trying to unmerge culture by 100%");
            return;
        }

        foreach (CulturalPreference p in referenceCulture.GetPreferences())
        {
            CellCulturalPreference preference = GetAcquiredPreferenceOrToAcquire(p.Id);

            if (preference != null)
            {
                preference.Unmerge(p, percentage);
            }
        }

        foreach (CulturalActivity a in referenceCulture.GetActivities())
        {
            CellCulturalActivity activity = GetPerformedActivityOrToPerform(a.Id);

            if (activity != null)
            {
                activity.Unmerge(a, percentage);
            }
        }

        foreach (CulturalSkill s in referenceCulture.GetSkills())
        {
            CellCulturalSkill skill = GetLearnedSkillOrToLearn(s.Id);

            if (skill != null)
            {
                skill.Unmerge(s, percentage);
            }
        }

        // NOTE: Knowledges and discoveries can't be easily 'unmerged' without
        // making some wild assumptions. So it's simpler just to leave them the same
    }

    public void MergeCulture(Culture sourceCulture, float percentage)
    {
        foreach (CulturalPreference p in sourceCulture.GetPreferences())
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

        foreach (CulturalActivity a in sourceCulture.GetActivities())
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

        foreach (CulturalSkill s in sourceCulture.GetSkills())
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

        foreach (CulturalKnowledge k in sourceCulture.GetKnowledges())
        {
            CellCulturalKnowledge knowledge = TryAddKnowledgeToLearn(k.Id, 0);
            knowledge.Merge(k.Value, percentage);
        }

        foreach (Discovery d in sourceCulture.Discoveries.Values)
        {
            AddDiscoveryToFind(d);
        }
    }

    public void Update(long timeSpan)
    {
        foreach (CellCulturalPreference preference in _preferences.Values)
        {
            preference.Update(timeSpan);
        }

        foreach (CellCulturalActivity activity in _activities.Values)
        {
            activity.Update(timeSpan);
        }

        foreach (CellCulturalSkill skill in _skills.Values)
        {
            skill.Update(timeSpan);
        }

        foreach (CellCulturalKnowledge knowledge in _knowledges.Values)
        {
            knowledge.Update(timeSpan);
        }
    }

    public void UpdatePolityCulturalProminence(PolityProminence polityProminence, long timeSpan)
    {
        PolityCulture polityCulture = polityProminence.Polity.Culture;

        foreach (CulturalPreference polityPreference in polityCulture.GetPreferences())
        {
            CellCulturalPreference cellPreference = GetAcquiredPreferenceOrToAcquire(polityPreference.Id);

            if (cellPreference == null)
            {
                cellPreference = CellCulturalPreference.CreateCellInstance(Group, polityPreference, 0);
                AddPreferenceToAcquire(cellPreference);
            }

            cellPreference.AddPolityProminenceEffect(polityPreference, polityProminence, timeSpan);
        }

        foreach (CulturalActivity polityActivity in polityCulture.GetActivities())
        {
            CellCulturalActivity cellActivity = GetPerformedActivityOrToPerform(polityActivity.Id);

            if (cellActivity == null)
            {
                cellActivity = CellCulturalActivity.CreateCellInstance(Group, polityActivity, 0);
                AddActivityToPerform(cellActivity);
            }

            cellActivity.AddPolityProminenceEffect(polityActivity, polityProminence, timeSpan);
        }

        foreach (CulturalSkill politySkill in polityCulture.GetSkills())
        {
            CellCulturalSkill cellSkill = GetLearnedSkillOrToLearn(politySkill.Id);

            if (cellSkill == null)
            {
                cellSkill = CellCulturalSkill.CreateCellInstance(Group, politySkill, 0);
                AddSkillToLearn(cellSkill);
            }

            cellSkill.AddPolityProminenceEffect(politySkill, polityProminence, timeSpan);
        }

        foreach (CulturalKnowledge polityKnowledge in polityCulture.GetKnowledges())
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

            CellCulturalKnowledge cellKnowledge = TryAddKnowledgeToLearn(polityKnowledge.Id, 0);

            cellKnowledge.AddPolityProminenceEffect(polityKnowledge, polityProminence, timeSpan);
        }

        foreach (Discovery polityDiscovery in polityCulture.Discoveries.Values)
        {
            AddDiscoveryToFind(polityDiscovery);
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
                throw new System.Exception("Attempted to add duplicate knowledge (" + knowledge.Id + ") to group " + Group);
            }
        }
    }

    public void PostUpdateAttributeValues()
    {
        foreach (CellCulturalPreference preference in _preferences.Values)
        {
            preference.PostUpdate();
        }

        float totalActivityValue = 0;

        foreach (CellCulturalActivity activity in _activities.Values)
        {
            activity.PostUpdate();
            totalActivityValue += activity.Value;

            if (!activity.CanPerform(Group))
            {
                _activitiesToStop.Add(activity);
            }
        }

        foreach (CellCulturalActivity activity in _activities.Values)
        {
            if (totalActivityValue > 0)
            {
                activity.Contribution = activity.Value / totalActivityValue;
            }
            else
            {
                activity.Contribution = 1f / _activities.Count;
            }
        }

        foreach (CellCulturalSkill skill in _skills.Values)
        {
            skill.PostUpdate();
        }

        foreach (CellCulturalKnowledge knowledge in _knowledges.Values)
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
    }

    public void AddKnowledgeToLose(string knowledgeId)
    {
        CulturalKnowledge knowledge = null;

        if (!_knowledges.TryGetValue(knowledgeId, out knowledge))
        {
            Debug.LogWarning("CellCulture: Trying to remove knowledge that is not present: " + knowledgeId);

            return;
        }

        _knowledgesToLose.Add(knowledge as CellCulturalKnowledge);
    }

    public void AddActivityToStop(string activityId)
    {
        CulturalActivity activity = null;

        if (!_activities.TryGetValue(activityId, out activity))
        {
            Debug.LogWarning("CellCulture: Trying to remove activity that is not present: " + activityId);

            return;
        }

        _activitiesToStop.Add(activity as CellCulturalActivity);
    }

    public void AddSkillToLose(string skillId)
    {
        CulturalSkill skill = null;

        if (!_skills.TryGetValue(skillId, out skill))
        {
            Debug.LogWarning("CellCulture: Trying to remove skill that is not present: " + skillId);

            return;
        }

        _skillsToLose.Add(skill as CellCulturalSkill);
    }

    public float MinimumSkillAdaptationLevel()
    {
        float minAdaptationLevel = 1f;

        foreach (CellCulturalSkill skill in _skills.Values)
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

        foreach (CellCulturalKnowledge knowledge in _knowledges.Values)
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

            // if progress level equals 0 that means the knowledge can't really progress. So we ignore it
            float level = (knowledge.ProgressLevel > 0) ? knowledge.CalculateExpectedProgressLevel() : 1;

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
        foreach (CellCulturalSkill s in _skills.Values)
        {
            s.Synchronize();
        }

        foreach (CellCulturalKnowledge k in _knowledges.Values)
        {
            k.Synchronize();
        }

        base.Synchronize();
    }

    public override void PrefinalizePropertiesLoad()
    {
        base.PrefinalizePropertiesLoad();

        foreach (CellCulturalPreference p in _preferences.Values)
        {
            p.Group = Group;
        }

        foreach (CellCulturalActivity a in _activities.Values)
        {
            a.Group = Group;
        }

        foreach (CellCulturalSkill s in _skills.Values)
        {
            s.Group = Group;
        }

        foreach (CellCulturalKnowledge k in _knowledges.Values)
        {
            k.Group = Group;
        }
    }
}
