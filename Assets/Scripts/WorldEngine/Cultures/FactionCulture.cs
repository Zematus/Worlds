using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class FactionCulture : Culture
{
    public const long OptimalTimeSpan = CellGroup.GenerationSpan * 500;

    [XmlIgnore]
    public Faction Faction;

    public FactionCulture()
    {

    }

    public FactionCulture(Faction faction) : base(faction.World)
    {
        Faction = faction;

        CellGroup coreGroup = Faction.CoreGroup;

        if (coreGroup == null)
            throw new System.Exception("CoreGroup can't be null at this point");

        CellCulture coreCulture = coreGroup.Culture;

        foreach (CulturalPreference p in coreCulture.Preferences)
        {
            AddPreference(new CulturalPreference(p));
        }

        foreach (CulturalActivity a in coreCulture.Activities)
        {
            AddActivity(new CulturalActivity(a));
        }

        foreach (CulturalSkill s in coreCulture.Skills)
        {
            AddSkill(new CulturalSkill(s));
        }

        foreach (CulturalKnowledge k in coreCulture.Knowledges)
        {
            AddKnowledge(new CulturalKnowledge(k));
        }

        foreach (CulturalDiscovery d in coreCulture.Discoveries)
        {
            AddDiscovery(new CulturalDiscovery(d));
        }
    }

    public float GetNextRandomFloat(int rngOffset)
    {
        return Faction.GetNextLocalRandomFloat(rngOffset);
    }

    public void Update()
    {
        CellGroup coreGroup = Faction.CoreGroup;

        if ((coreGroup == null) || (!coreGroup.StillPresent))
            throw new System.Exception("CoreGroup is null or no longer present");

        CellCulture coreCulture = coreGroup.Culture;

        long dateSpan = World.CurrentDate - Faction.LastUpdateDate;

        float timeFactor = dateSpan / (float)(dateSpan + OptimalTimeSpan);

        ////// Update Preferences

        HashSet<string> foundPreferenceIds = new HashSet<string>();

        foreach (CulturalPreference p in coreCulture.Preferences)
        {
            foundPreferenceIds.Add(p.Id);

            CulturalPreference preference = GetPreference(p.Id);

#if DEBUG
            float prevValue = 0;
#endif

            if (preference == null)
            {
                preference = new CulturalPreference(p);
                AddPreference(preference);

                preference.Value = p.Value * timeFactor;
            }
            else
            {
#if DEBUG
                prevValue = preference.Value;
#endif

                preference.Value = (preference.Value * (1f - timeFactor)) + (p.Value * timeFactor);
            }

#if DEBUG
            if (Manager.RegisterDebugEvent != null)
            {
                if (Manager.TracingData.FactionId == Faction.Id)
                {
                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                        "FactionCulture:Update - coreCulture.Preferences - Faction.Id:" + Faction.Id,
                        "CurrentDate: " + World.CurrentDate +
                        ", coreCulture.Group.Id: " + coreCulture.Group.Id +
                        ", preference.Id: " + preference.Id +
                        ", prevValue: " + prevValue +
                        ", p.Value: " + p.Value +
                        ", preference.Value: " + preference.Value +
                        "");

                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
                }
            }
#endif
        }

        foreach (CulturalPreference p in Preferences)
        {
            if (!foundPreferenceIds.Contains(p.Id))
            {
                p.Value = (p.Value * (1f - timeFactor));
            }
        }

        ////// Update Activities

        HashSet<string> foundActivityIds = new HashSet<string>();

        foreach (CulturalActivity a in coreCulture.Activities)
        {
            foundActivityIds.Add(a.Id);

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

        foreach (CulturalActivity a in Activities)
        {
            if (!foundActivityIds.Contains(a.Id))
            {
                a.Value = (a.Value * (1f - timeFactor));
            }
        }

        ////// Update Skills

        HashSet<string> foundSkillIds = new HashSet<string>();

        foreach (CulturalSkill s in coreCulture.Skills)
        {
            foundSkillIds.Add(s.Id);

            CulturalSkill skills = GetSkill(s.Id);

            if (skills == null)
            {
                skills = new CulturalSkill(s);
                AddSkill(skills);

                skills.Value = s.Value * timeFactor;
            }
            else
            {
                skills.Value = (skills.Value * (1f - timeFactor)) + (s.Value * timeFactor);
            }
        }

        foreach (CulturalSkill s in Skills)
        {
            if (!foundSkillIds.Contains(s.Id))
            {

                s.Value = (s.Value * (1f - timeFactor));
            }
        }

        ////// Update Knowledges

        //#if DEBUG
        //        if (Manager.RegisterDebugEvent != null)
        //        {
        //            if (Manager.TracingData.FactionId == Faction.Id)
        //            {
        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "FactionCulture:Update - Knowledges Counts - Faction.Id:" + Faction.Id,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", coreCulture.Group.Id: " + coreCulture.Group.Id +
        //                    ", Knowledges.Count: " + Knowledges.Count +
        //                    ", coreCulture.Knowledges.Count: " + coreCulture.Knowledges.Count +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        HashSet<string> foundKnowledgeIds = new HashSet<string>();

        foreach (CulturalKnowledge k in coreCulture.Knowledges)
        {
            foundKnowledgeIds.Add(k.Id);

            CulturalKnowledge knowledge = GetKnowledge(k.Id);

#if DEBUG
            int oldKnowledgeValue = 0;
#endif

            if (knowledge == null)
            {
                knowledge = new CulturalKnowledge(k);

                AddKnowledge(knowledge);

                knowledge.Value = (int)(k.Value * timeFactor);
            }
            else
            {
#if DEBUG
                oldKnowledgeValue = knowledge.Value;
#endif

                knowledge.Value = (int)((knowledge.Value * (1f - timeFactor)) + (k.Value * timeFactor));
            }

#if DEBUG
            if (Manager.RegisterDebugEvent != null)
            {
                if (Manager.TracingData.FactionId == Faction.Id)
                {
                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                        "FactionCulture:Update - coreCulture.Knowledges - Faction.Id:" + Faction.Id,
                        "CurrentDate: " + World.CurrentDate +
                        ", coreCulture.Group.Id: " + coreCulture.Group.Id +
                        ", knowledge.Id: " + knowledge.Id +
                        ", oldKnowledgeValue: " + oldKnowledgeValue +
                        ", k.Value: " + k.Value +
                        ", knowledge.Value: " + knowledge.Value +
                        "");

                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
                }
            }
#endif
        }

        foreach (CulturalKnowledge k in Knowledges)
        {
            if (!foundKnowledgeIds.Contains(k.Id))
            {
                k.Value = (int)(k.Value * (1f - timeFactor));

                //#if DEBUG
                //                if (Manager.RegisterDebugEvent != null)
                //                {
                //                    if (Manager.TracingData.FactionId == Faction.Id)
                //                    {
                //                        SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                //                            "FactionCulture:Update - Knowledges - Faction.Id:" + Faction.Id,
                //                            "CurrentDate: " + World.CurrentDate +
                //                            ", k.Id: " + k.Id +
                //                            ", k.Value: " + k.Value +
                //                            "");

                //                        Manager.RegisterDebugEvent("DebugMessage", debugMessage);
                //                    }
                //                }
                //#endif
            }
        }

        ////// Update Discoveries

        HashSet<string> foundDiscoveryIds = new HashSet<string>();

        foreach (CulturalDiscovery d in coreCulture.Discoveries)
        {
            foundDiscoveryIds.Add(d.Id);

            CulturalDiscovery discovery = GetDiscovery(d.Id);

            if (discovery == null)
            {
                discovery = new CulturalDiscovery(d);
                AddDiscovery(discovery);
            }
        }

        List<CulturalDiscovery> discoveriesToRemove = new List<CulturalDiscovery>(Discoveries.Count);

        foreach (CulturalDiscovery d in Discoveries)
        {
            int idHash = d.Id.GetHashCode();

            if (!foundDiscoveryIds.Contains(d.Id))
            {
                if (GetNextRandomFloat(RngOffsets.FACTION_CULTURE_DISCOVER_LOSS_CHANCE + idHash) < timeFactor)
                {
                    discoveriesToRemove.Add(d);
                }
            }
        }

        foreach (CulturalDiscovery d in discoveriesToRemove)
        {
            RemoveDiscovery(d);
        }
    }
}
