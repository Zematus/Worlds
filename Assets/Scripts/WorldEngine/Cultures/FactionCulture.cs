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

        foreach (CulturalPreference p in coreCulture.Preferences.Values)
        {
            AddPreference(new CulturalPreference(p));
        }

        foreach (CulturalActivity a in coreCulture.Activities.Values)
        {
            AddActivity(new CulturalActivity(a));
        }

        foreach (CulturalSkill s in coreCulture.Skills.Values)
        {
            AddSkill(new CulturalSkill(s));
        }

        foreach (CellCulturalKnowledge k in coreCulture.Knowledges.Values)
        {
            //#if DEBUG
            //                if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
            //                {
            //                    if (Manager.TracingData.FactionId == Faction.Id)
            //                    {
            //                        SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
            //                            "FactionCulture:FactionCulture - add coreCulture.Knowledges - Faction.Id:" + Faction.Id,
            //                            "CurrentDate: " + World.CurrentDate +
            //                            ", coreCulture.Group.Id: " + coreCulture.Group.Id +
            //                            ", Knowledge Id: " + k.Id +
            //                            "");

            //                        Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            //                    }
            //                }
            //#endif
            
            AddKnowledge(new CulturalKnowledge(k));
            //}
        }

        foreach (Discovery d in coreCulture.Discoveries.Values)
        {
            AddDiscovery(d);
        }
    }

    public float GetNextRandomFloat(int rngOffset)
    {
        return Faction.GetNextLocalRandomFloat(rngOffset);
    }

    private void UpdatePreferences(CellCulture coreCulture, float timeFactor)
    {
        //Profiler.BeginSample("Culture - Update Preferences");

        foreach (CulturalPreference p in coreCulture.Preferences.Values)
        {
            //Profiler.BeginSample("GetPreference");

            CulturalPreference preference = GetPreference(p.Id);

            //Profiler.EndSample();

            //#if DEBUG
            //            float prevValue = 0;
            //#endif

            if (preference == null)
            {
                //Profiler.BeginSample("new CulturalPreference");

                preference = new CulturalPreference(p);
                AddPreference(preference);

                preference.Value = p.Value * timeFactor;

                //Profiler.EndSample();
            }
            else
            {
                //Profiler.BeginSample("update preference.Value");

                //#if DEBUG
                //                prevValue = preference.Value;
                //#endif

                preference.Value = (preference.Value * (1f - timeFactor)) + (p.Value * timeFactor);

                //Profiler.EndSample();
            }

            //#if DEBUG
            //            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
            //            {
            //                if (Manager.TracingData.FactionId == Faction.Id)
            //                {
            //                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
            //                        "FactionCulture:Update - coreCulture.Preferences - Faction.Id:" + Faction.Id,
            //                        "CurrentDate: " + World.CurrentDate +
            //                        ", coreCulture.Group.Id: " + coreCulture.Group.Id +
            //                        ", preference.Id: " + preference.Id +
            //                        ", prevValue: " + prevValue +
            //                        ", p.Value: " + p.Value +
            //                        ", preference.Value: " + preference.Value +
            //                        "");

            //                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            //                }
            //            }
            //#endif
        }

        foreach (CulturalPreference p in Preferences.Values)
        {
            //Profiler.BeginSample("coreCulture.Preferences.ContainsKey");

            if (!coreCulture.Preferences.ContainsKey(p.Id))
            {
                p.Value = (p.Value * (1f - timeFactor));
            }

            //Profiler.EndSample();
        }

        //Profiler.EndSample();
    }

    private void UpdateActivities(CellCulture coreCulture, float timeFactor)
    {
        //Profiler.BeginSample("Culture - Update Activities");

        foreach (CulturalActivity a in coreCulture.Activities.Values)
        {
            //Profiler.BeginSample("GetActivity");

            CulturalActivity activity = GetActivity(a.Id);

            //Profiler.EndSample();

            if (activity == null)
            {
                //Profiler.BeginSample("new CulturalActivity");

                activity = new CulturalActivity(a);
                AddActivity(activity);

                activity.Value = a.Value * timeFactor;

                //Profiler.EndSample();
            }
            else
            {
                //Profiler.BeginSample("update activity.Value");

                activity.Value = (activity.Value * (1f - timeFactor)) + (a.Value * timeFactor);

                //Profiler.EndSample();
            }
        }

        foreach (CulturalActivity a in Activities.Values)
        {
            //Profiler.BeginSample("coreCulture.Activities.ContainsKey");

            if (!coreCulture.Activities.ContainsKey(a.Id))
            {
                a.Value = (a.Value * (1f - timeFactor));
            }

            //Profiler.EndSample();
        }

        //Profiler.EndSample();
    }

    private void UpdateSkills(CellCulture coreCulture, float timeFactor)
    {
        //Profiler.BeginSample("Culture - Update Skills");

        foreach (CulturalSkill s in coreCulture.Skills.Values)
        {
            //Profiler.BeginSample("GetSkill");

            CulturalSkill skill = GetSkill(s.Id);

            //Profiler.EndSample();

            if (skill == null)
            {
                //Profiler.BeginSample("new CulturalSkill");

                skill = new CulturalSkill(s);
                AddSkill(skill);

                skill.Value = s.Value * timeFactor;

                //Profiler.EndSample();
            }
            else
            {
                //Profiler.BeginSample("update skill.Value");

                skill.Value = (skill.Value * (1f - timeFactor)) + (s.Value * timeFactor);

                //Profiler.EndSample();
            }
        }

        foreach (CulturalSkill s in Skills.Values)
        {
            //Profiler.BeginSample("coreCulture.Skills.ContainsKey");

            if (!coreCulture.Skills.ContainsKey(s.Id))
            {
                s.Value = (s.Value * (1f - timeFactor));
            }

            //Profiler.EndSample();
        }

        //Profiler.EndSample();
    }

    private void UpdateKnowledges(CellCulture coreCulture, float timeFactor)
    {
        //Profiler.BeginSample("Culture - Update Knowledges");

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Manager.TracingData.FactionId == Faction.Id)
        //            {
        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "FactionCulture:Update - Update Knowledges - Faction.Id:" + Faction.Id,
        //                    "CurrentDate: " + World.CurrentDate +
        //                    ", coreCulture.Group.Id: " + coreCulture.Group.Id +
        //                    ", Knowledges.Count: " + Knowledges.Count +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        foreach (CellCulturalKnowledge k in coreCulture.Knowledges.Values)
        {
            //Profiler.BeginSample("GetKnowledge");

            CulturalKnowledge knowledge = GetKnowledge(k.Id);

            //Profiler.EndSample();

            if (knowledge == null)
            {
                //Profiler.BeginSample("new CulturalKnowledge");

                knowledge = new CulturalKnowledge(k);
                AddKnowledge(knowledge);

                //Profiler.EndSample();
            }

            //Profiler.BeginSample("update knowledge.Value");

            float addValue = k.Value * timeFactor;

            if (addValue < 1) // Always try approaching the core cell knowledge value regardless how small the timeFactor is
            {
                if ((knowledge.Value - k.Value) <= -1)
                    knowledge.Value++;
                else if ((knowledge.Value - k.Value) >= 1)
                    knowledge.Value--;
            }
            else
            {
                knowledge.Value = (int)((knowledge.Value * (1f - timeFactor)) + addValue);
            }

            knowledge.Limit = Mathf.Max(k.Limit, knowledge.Limit);

            //Profiler.EndSample();
        }

        foreach (CulturalKnowledge k in Knowledges.Values)
        {
            //Profiler.BeginSample("coreCulture.Skills.ContainsKey");

            if (!coreCulture.Knowledges.ContainsKey(k.Id))
            {
                k.Value = (int)(k.Value * (1f - timeFactor));
            }

            //Profiler.EndSample();
        }

        //Profiler.EndSample();
    }

    private void UpdateDiscoveries(CellCulture coreCulture, float timeFactor)
    {
        //Profiler.BeginSample("Culture - Update Discoveries");

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if (Manager.TracingData.FactionId == Faction.Id)
        //            {
        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                "FactionCulture:Update - Update Discoveries - Faction.Id:" + Faction.Id,
        //                "CurrentDate: " + World.CurrentDate +
        //                ", coreCulture.Group.Id: " + coreCulture.Group.Id +
        //                ", Discoveries.Count: " + Discoveries.Count +
        //                "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        foreach (Discovery d in coreCulture.Discoveries.Values)
        {
            AddDiscovery(d);
        }

        List<Discovery> discoveriesToTryToRemove = new List<Discovery>(Discoveries.Values);

        foreach (Discovery d in discoveriesToTryToRemove)
        {
            //Profiler.BeginSample("coreCulture.Discoveries.ContainsKey");

            if (!coreCulture.Discoveries.ContainsKey(d.Id))
            {
                RemoveDiscovery(d);
                //TryRemovingDiscovery(d, timeFactor); // TODO: Take care of issue #133 before cleaning up this
            }

            //Profiler.EndSample();
        }

        //Profiler.EndSample();
    }

    private void UpdateProperties(CellCulture coreCulture)
    {
        foreach (string property in coreCulture.GetProperties())
        {
            AddProperty(property);
        }

        List<string> propertiesToTryToRemove = new List<string>(_properties);

        foreach (string property in propertiesToTryToRemove)
        {
            if (!coreCulture.HasProperty(property))
            {
                RemoveProperty(property);
            }
        }
    }

    public void Update()
    {
        CellGroup coreGroup = Faction.CoreGroup;

        if ((coreGroup == null) || (!coreGroup.StillPresent))
            throw new System.Exception("CoreGroup is null or no longer present");

        CellCulture coreCulture = coreGroup.Culture;

        long dateSpan = World.CurrentDate - Faction.LastUpdateDate;

        float timeFactor = dateSpan / (float)(dateSpan + OptimalTimeSpan);
        
        UpdatePreferences(coreCulture, timeFactor);
        UpdateActivities(coreCulture, timeFactor);
        UpdateSkills(coreCulture, timeFactor);
        UpdateKnowledges(coreCulture, timeFactor);
        UpdateDiscoveries(coreCulture, timeFactor);
        UpdateProperties(coreCulture);
    }

    // TODO: Take care of issue #133 before cleaning up this
    //private void TryRemovingDiscovery(Discovery discovery, float timeFactor)
    //{
    //    int idHash = discovery.IdHash;

    //    if (GetNextRandomFloat(RngOffsets.FACTION_CULTURE_DISCOVERY_LOSS_CHANCE + idHash) < timeFactor)
    //    {
    //        RemoveDiscovery(discovery);
    //    }
    //}
}
