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

    private Dictionary<string, CellCulturalPreference> _preferencesToAcquire = new Dictionary<string, CellCulturalPreference>();
    private Dictionary<string, CellCulturalActivity> _activitiesToPerform = new Dictionary<string, CellCulturalActivity>();
    private Dictionary<string, CellCulturalSkill> _skillsToLearn = new Dictionary<string, CellCulturalSkill>();
    private Dictionary<string, CellCulturalKnowledge> _knowledgesToLearn = new Dictionary<string, CellCulturalKnowledge>();
    private Dictionary<string, IDiscovery> _discoveriesToFind = new Dictionary<string, IDiscovery>();

    private HashSet<CellCulturalPreference> _preferencesToLose = new HashSet<CellCulturalPreference>();
    private HashSet<CellCulturalActivity> _activitiesToStop = new HashSet<CellCulturalActivity>();
    private HashSet<CellCulturalSkill> _skillsToLose = new HashSet<CellCulturalSkill>();
    private HashSet<CellCulturalKnowledge> _knowledgesToLose = new HashSet<CellCulturalKnowledge>();
    private HashSet<IDiscovery> _discoveriesToLose = new HashSet<IDiscovery>();

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

        foreach (var d in sourceCulture.Discoveries.Values)
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
        foreach (var d in Discoveries.Values)
        {
            d.OnGain(Group);
        }
    }

    public void SetLanguageUpdateCells()
    {
        if (!Manager.ValidUpdateTypeAndSubtype(CellUpdateType.Language, CellUpdateSubType.Membership))
            return;

        Manager.AddUpdatedCell(Group.Cell);

        foreach (KeyValuePair<Direction, CellGroup> pair in Group.Neighbors)
        {
            Manager.AddUpdatedCell(pair.Value.Cell);
        }
    }

    public void AddPreferenceToAcquire(CellCulturalPreference preference)
    {
        if (_preferencesToAcquire.ContainsKey(preference.Id))
            return;

        _preferencesToAcquire.Add(preference.Id, preference);
    }

    public void AddActivityToPerform(CellCulturalActivity activity)
    {
        if (_activitiesToPerform.ContainsKey(activity.Id))
            return;

        _activitiesToPerform.Add(activity.Id, activity);
    }

    public void AddSkillToLearn(CellCulturalSkill skill)
    {
        if (_skillsToLearn.ContainsKey(skill.Id))
            return;

        _skillsToLearn.Add(skill.Id, skill);
    }

    public CellCulturalKnowledge TryAddKnowledgeToLearn(string id, int initialValue = 0, int initialLimit = -1)
    {
        CellCulturalKnowledge knowledge = GetKnowledge(id) as CellCulturalKnowledge;

        if (knowledge != null)
        {
            knowledge.SetLevelLimit(initialLimit);
            return knowledge;
        }

        CellCulturalKnowledge tempKnowledge;

        if (_knowledgesToLearn.TryGetValue(id, out tempKnowledge))
        {
            tempKnowledge.SetLevelLimit(initialLimit);
            return tempKnowledge;
        }

        if (knowledge == null)
        {
            knowledge = CellCulturalKnowledge.CreateCellInstance(id, Group, initialValue, initialLimit);
        }

        _knowledgesToLearn.Add(id, knowledge);

        return knowledge;
    }

    public void AddDiscoveryToFind(IDiscovery discovery)
    {
        if (Discoveries.ContainsKey(discovery.Id))
            return;

        if (_discoveriesToFind.ContainsKey(discovery.Id))
            return;

        _discoveriesToFind.Add(discovery.Id, discovery);
    }

    public void AddDiscoveryToLose(IDiscovery discovery)
    {
        if (!Discoveries.ContainsKey(discovery.Id))
            return;

        if (_discoveriesToLose.Contains(discovery))
            return;

        _discoveriesToLose.Add(discovery);
    }

    public CellCulturalPreference GetAcquiredPreferenceOrToAcquire(string id)
    {
        CellCulturalPreference preference = GetPreference(id) as CellCulturalPreference;

        if (preference != null)
            return preference;

        if (_preferencesToAcquire.TryGetValue(id, out preference))
            return preference;

        return null;
    }

    public CellCulturalActivity GetPerformedActivityOrToPerform(string id)
    {
        CellCulturalActivity activity = GetActivity(id) as CellCulturalActivity;

        if (activity != null)
            return activity;

        if (_activitiesToPerform.TryGetValue(id, out activity))
            return activity;

        return null;
    }

    public CellCulturalSkill GetLearnedSkillOrToLearn(string id)
    {
        CellCulturalSkill skill = GetSkill(id) as CellCulturalSkill;

        if (skill != null)
            return skill;

        if (_skillsToLearn.TryGetValue(id, out skill))
            return skill;

        return null;
    }

    public bool HasOrWillHaveKnowledge(string id)
    {
        return HasKnowledge(id) | _knowledgesToLearn.ContainsKey(id);
    }

    public bool HasOrWillHaveDiscovery(string id)
    {
        return HasDiscovery(id) | _discoveriesToFind.ContainsKey(id);
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
            // Trying to unmerge property values by 100% will generate NaN values,
            // and the prominence will get removed anyway (hopefully), so skipping....

            //Debug.LogWarning("Trying to unmerge culture by 100%");
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
            CellCulturalKnowledge knowledge = TryAddKnowledgeToLearn(k.Id);
            knowledge.Merge(k.Value, percentage);
        }

        foreach (var d in sourceCulture.Discoveries.Values)
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

    /// <summary>
    /// Updates a cell's culture with the influence of a prominence's polity culture
    /// </summary>
    /// <param name="polityProminence">the influencing prominence</param>
    /// <param name="timeSpan">the time span since the last cell update</param>
    public void UpdateProminenceCulturalProperties(PolityProminence polityProminence, long timeSpan)
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

            CellCulturalKnowledge cellKnowledge = TryAddKnowledgeToLearn(polityKnowledge.Id);

            cellKnowledge.AddPolityProminenceEffect(polityKnowledge, polityProminence, timeSpan);
        }

        foreach (var polityDiscovery in polityCulture.Discoveries.Values)
        {
            AddDiscoveryToFind(polityDiscovery);
        }
    }

    /// <summary>
    /// Post updates a cell culture through the influence of a polity prominence
    /// </summary>
    /// <param name="polityProminence">the influencing prominence</param>
    public void PostUpdateProminenceCulturalProperties(PolityProminence polityProminence)
    {
        PolityCulture polityCulture = polityProminence.Polity.Culture;

        if (Group.HighestPolityProminence == null)
        {
            throw new System.Exception("HighestPolityProminence is null. Group: " + Group.Id);
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
        foreach (var d in _discoveriesToLose)
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
        foreach (var d in _discoveriesToLose)
        {
            if (d is Discovery033 d033)
            {
                d033.RetryAssignAfterLoss(Group);
            }
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
        foreach (var discovery in _discoveriesToFind.Values)
        {
            AddDiscovery(discovery);
            discovery.OnGain(Group);
        }

        foreach (CellCulturalPreference preference in _preferencesToAcquire.Values)
        {
            AddPreference(preference);
        }

        foreach (CellCulturalActivity activity in _activitiesToPerform.Values)
        {
            AddActivity(activity);
        }

        foreach (CellCulturalSkill skill in _skillsToLearn.Values)
        {
            AddSkill(skill);
        }

        foreach (CellCulturalKnowledge knowledge in _knowledgesToLearn.Values)
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

        foreach (var d in Discoveries.Values)
        {
            if (d is Discovery033 d033)
            {
                if (d033.CanBeHeld(Group))
                {
                    continue;
                }

                _discoveriesToLose.Add(d);
            }
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
        _preferencesToAcquire.Clear();
        _activitiesToPerform.Clear();
        _skillsToLearn.Clear();
        _knowledgesToLearn.Clear();
        _discoveriesToFind.Clear();
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
