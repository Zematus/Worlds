using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

/// <summary>
/// An abstraction of a faction's culture
/// </summary>
public class FactionCulture : Culture
{
    public const long OptimalTimeSpan = CellGroup.GenerationSpan * 500;

    [XmlIgnore]
    public Faction Faction;

    /// <summary>
    /// XML deserialization constructor (should only be use by the deserializer)
    /// </summary>
    public FactionCulture()
    {

    }

    /// <summary>
    /// Builds a new culture object for a specific faction
    /// </summary>
    /// <param name="faction">the faction this culture is going to be assigned to</param>
    public FactionCulture(Faction faction) : base(faction.World)
    {
        Faction = faction;

        CellGroup coreGroup = Faction.CoreGroup;

        if (coreGroup == null)
            throw new System.Exception("CoreGroup can't be null at this point");

        CellCulture coreCulture = coreGroup.Culture;

        foreach (CulturalPreference p in coreCulture.GetPreferences())
        {
            AddPreference(new CulturalPreference(p));
        }

        foreach (CulturalActivity a in coreCulture.GetActivities())
        {
            AddActivity(new CulturalActivity(a));
        }

        foreach (CulturalSkill s in coreCulture.GetSkills())
        {
            AddSkill(new CulturalSkill(s));
        }

        foreach (CellCulturalKnowledge k in coreCulture.GetKnowledges())
        {
            AddKnowledge(new CulturalKnowledge(k));
        }

        foreach (var d in coreCulture.Discoveries.Values)
        {
            AddDiscovery(d);
        }
    }

    /// <summary>
    /// Pushes the faction's cultural preference values to match those in the
    /// faction's core cell culture
    /// </summary>
    /// <param name="coreCulture">the culture from the faction's core cell</param>
    /// <param name="timeFactor">a factor based on the amount of time has passed
    /// since the last faction update</param>
    private void UpdatePreferences(CellCulture coreCulture, float timeFactor)
    {
        foreach (CulturalPreference p in coreCulture.GetPreferences())
        {
            CulturalPreference preference = GetPreference(p.Id);

            if (preference == null)
            {
                preference = new CulturalPreference(p);
                AddPreference(preference);

                preference.Value = p.Value * timeFactor;
            }
            else
            {
                preference.Value = (preference.Value * (1f - timeFactor)) + (p.Value * timeFactor);
            }
        }

        foreach (CulturalPreference p in _preferences.Values)
        {
            if (coreCulture.GetPreference(p.Id) == null)
            {
                p.Value *= (1f - timeFactor);
            }
        }
    }

    /// <summary>
    /// Pushes the faction's cultural activity values to match those in the
    /// faction's core cell culture
    /// </summary>
    /// <param name="coreCulture">the culture from the faction's core cell</param>
    /// <param name="timeFactor">a factor based on the amount of time has passed
    /// since the last faction update</param>
    private void UpdateActivities(CellCulture coreCulture, float timeFactor)
    {
        foreach (CulturalActivity a in coreCulture.GetActivities())
        {
            CulturalActivity activity = GetActivity(a.Id);

            if (activity == null)
            {
                activity = new CulturalActivity(a);
                AddActivity(activity);

                activity.Value = a.Value * timeFactor;
            }
            else
            {
                activity.Value = (activity.Value * (1f - timeFactor)) + (a.Value * timeFactor);
            }
        }

        foreach (CulturalActivity a in _activities.Values)
        {
            if (coreCulture.GetActivity(a.Id) == null)
            {
                a.Value *= (1f - timeFactor);
            }
        }
    }

    /// <summary>
    /// Pushes the faction's cultural skill values to match those in the
    /// faction's core cell culture
    /// </summary>
    /// <param name="coreCulture">the culture from the faction's core cell</param>
    /// <param name="timeFactor">a factor based on the amount of time has passed
    /// since the last faction update</param>
    private void UpdateSkills(CellCulture coreCulture, float timeFactor)
    {
        foreach (CulturalSkill s in coreCulture.GetSkills())
        {
            CulturalSkill skill = GetSkill(s.Id);

            if (skill == null)
            {
                skill = new CulturalSkill(s);
                AddSkill(skill);

                skill.Value = s.Value * timeFactor;
            }
            else
            {
                skill.Value = (skill.Value * (1f - timeFactor)) + (s.Value * timeFactor);
            }
        }

        foreach (CulturalSkill s in _skills.Values)
        {
            if (coreCulture.GetSkill(s.Id) == null)
            {
                s.Value = s.Value * (1f - timeFactor);
            }
        }
    }

    /// <summary>
    /// Pushes the faction's cultural knowledge values to match those in the
    /// faction's core cell culture
    /// </summary>
    /// <param name="coreCulture">the culture from the faction's core cell</param>
    /// <param name="timeFactor">a factor based on the amount of time has passed
    /// since the last faction update</param>
    private void UpdateKnowledges(CellCulture coreCulture, float timeFactor)
    {
        foreach (CellCulturalKnowledge k in coreCulture.GetKnowledges())
        {
            CulturalKnowledge knowledge = GetKnowledge(k.Id);

            if (knowledge == null)
            {
                knowledge = new CulturalKnowledge(k);
                AddKnowledge(knowledge);
            }

            int newValue = (int)((knowledge.Value * (1f - timeFactor)) + (k.Value * timeFactor));

            if (newValue != knowledge.Value)
            {
                knowledge.Value = newValue;
                Faction.GenerateKnowledgeLevelFallsBelowEvents(k.Id, knowledge.Value);
                Faction.GenerateKnowledgeLevelRaisesAboveEvents(k.Id, knowledge.Value);
            }
        }

        foreach (CulturalKnowledge k in _knowledges.Values)
        {
            if ((coreCulture.GetKnowledge(k.Id) == null) && (k.Value > 0))
            {
                int newValue = (int)(k.Value * (1f - timeFactor));

                if (newValue != k.Value)
                {
                    k.Value = newValue;
                    Faction.GenerateKnowledgeLevelFallsBelowEvents(k.Id, k.Value);
                    Faction.GenerateKnowledgeLevelRaisesAboveEvents(k.Id, k.Value);
                }
            }
        }
    }

    protected override void AddDiscovery(IDiscovery discovery)
    {
        base.AddDiscovery(discovery);

        Faction.GenerateGainedDiscoveryEvents(discovery.Id);
    }

    /// <summary>
    /// Adds or removes discoveries that are present/not present on the
    /// faction's core cell culture
    /// </summary>
    /// <param name="coreCulture">the culture from the faction's core cell</param>
    private void UpdateDiscoveries(CellCulture coreCulture)
    {
        foreach (var d in coreCulture.Discoveries.Values)
        {
            AddDiscovery(d);
        }

        List<IDiscovery> discoveriesToTryToRemove = new List<IDiscovery>(Discoveries.Values);

        foreach (var d in discoveriesToTryToRemove)
        {
            if (!coreCulture.Discoveries.ContainsKey(d.Id))
            {
                RemoveDiscovery(d);
            }
        }
    }

    /// <summary>
    /// Updates the faction cultural attributes
    /// </summary>
    public void Update()
    {
        CellGroup coreGroup = Faction.CoreGroup;

        if ((coreGroup == null) || (!coreGroup.StillPresent))
            throw new System.Exception("CoreGroup is null or no longer present: Faction id: " + Faction.Id);

        CellCulture coreCulture = coreGroup.Culture;

        long dateSpan = World.CurrentDate - Faction.LastUpdateDate;

        float timeFactor = dateSpan / (float)(dateSpan + OptimalTimeSpan);
        
        UpdatePreferences(coreCulture, timeFactor);
        UpdateActivities(coreCulture, timeFactor);
        UpdateSkills(coreCulture, timeFactor);
        UpdateKnowledges(coreCulture, timeFactor);
        UpdateDiscoveries(coreCulture);
    }

    public override void SetHolderToUpdate(bool warnIfUnexpected = true) => Faction.SetToUpdate(warnIfUnexpected);
}
