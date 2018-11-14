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
            if (k.IsPresent)
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

                AddKnowledge(new FactionCulturalKnowledge(faction, k, faction.Polity.Culture));
            }
        }

        foreach (CellCulturalDiscovery d in coreCulture.Discoveries.Values)
        {
            if (d.IsPresent)
            {
                AddDiscovery(new FactionCulturalDiscovery(faction, d, faction.Polity.Culture));
            }
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

        Profiler.BeginSample("Culture - Update Preferences");

        foreach (CulturalPreference p in coreCulture.Preferences.Values)
        {
            Profiler.BeginSample("GetPreference");

            CulturalPreference preference = GetPreference(p.Id);

            Profiler.EndSample();

//#if DEBUG
//            float prevValue = 0;
//#endif

            if (preference == null)
            {
                Profiler.BeginSample("new CulturalPreference");

                preference = new CulturalPreference(p);
                AddPreference(preference);

                preference.Value = p.Value * timeFactor;

                Profiler.EndSample();
            }
            else
            {
                Profiler.BeginSample("update preference.Value");

//#if DEBUG
//                prevValue = preference.Value;
//#endif

                preference.Value = (preference.Value * (1f - timeFactor)) + (p.Value * timeFactor);

                Profiler.EndSample();
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
            Profiler.BeginSample("coreCulture.Preferences.ContainsKey");

            if (!coreCulture.Preferences.ContainsKey(p.Id))
            {
                p.Value = (p.Value * (1f - timeFactor));
            }

            Profiler.EndSample();
        }

        Profiler.EndSample();

        ////// Update Activities

        Profiler.BeginSample("Culture - Update Activities");

        foreach (CulturalActivity a in coreCulture.Activities.Values)
        {
            Profiler.BeginSample("GetActivity");

            CulturalActivity activity = GetActivity(a.Id);

            Profiler.EndSample();

            if (activity == null)
            {
                Profiler.BeginSample("new CulturalActivity");

                activity = new CulturalActivity(a);
                AddActivity(activity);

                activity.Value = a.Value * timeFactor;

                Profiler.EndSample();
            }
            else
            {
                Profiler.BeginSample("update activity.Value");

                activity.Value = (activity.Value * (1f - timeFactor)) + (a.Value * timeFactor);

                Profiler.EndSample();
            }
        }

        foreach (CulturalActivity a in Activities.Values)
        {
            Profiler.BeginSample("coreCulture.Activities.ContainsKey");

            if (!coreCulture.Activities.ContainsKey(a.Id))
            {
                a.Value = (a.Value * (1f - timeFactor));
            }

            Profiler.EndSample();
        }

        Profiler.EndSample();

        ////// Update Skills

        Profiler.BeginSample("Culture - Update Skills");

        foreach (CulturalSkill s in coreCulture.Skills.Values)
        {
            Profiler.BeginSample("GetSkill");

            CulturalSkill skills = GetSkill(s.Id);

            Profiler.EndSample();

            if (skills == null)
            {
                Profiler.BeginSample("new CulturalSkill");

                skills = new CulturalSkill(s);
                AddSkill(skills);

                skills.Value = s.Value * timeFactor;

                Profiler.EndSample();
            }
            else
            {
                Profiler.BeginSample("update skills.Value");

                skills.Value = (skills.Value * (1f - timeFactor)) + (s.Value * timeFactor);

                Profiler.EndSample();
            }
        }

        foreach (CulturalSkill s in Skills.Values)
        {
            Profiler.BeginSample("coreCulture.Skills.ContainsKey");

            if (!coreCulture.Skills.ContainsKey(s.Id))
            {
                s.Value = (s.Value * (1f - timeFactor));
            }

            Profiler.EndSample();
        }

        Profiler.EndSample();

        ////// Update Knowledges

        Profiler.BeginSample("Culture - Update Knowledges");

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

        foreach (FactionCulturalKnowledge k in Knowledges.Values)
        {
//#if DEBUG
//            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//            {
//                if (k.IsPresent && (Manager.TracingData.FactionId == Faction.Id))
//                {
//                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                        "FactionCulture:Update - Update Knowledges - Faction.Id:" + Faction.Id,
//                        "CurrentDate: " + World.CurrentDate +
//                        ", coreCulture.Group.Id: " + coreCulture.Group.Id +
//                        ", k.Id: " + k.Id +
//                        ", k.IsPresent: " + k.IsPresent +
//                        ", k.Value: " + k.Value +
//                        "");

//                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//                }
//            }
//#endif

            k.UpdateFromCoreKnowledge(timeFactor);
        }

        Profiler.EndSample();

        ////// Update Discoveries

        Profiler.BeginSample("Culture - Update Discoveries");

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

        foreach (FactionCulturalDiscovery d in Discoveries.Values)
        {
#if DEBUG
            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
            {
                if (d.IsPresent && (Manager.TracingData.FactionId == Faction.Id))
                {
                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                    "FactionCulture:Update - Update Discoveries before TryRemovingDiscovery - Faction.Id:" + Faction.Id,
                    "CurrentDate: " + World.CurrentDate +
                    ", coreCulture.Group.Id: " + coreCulture.Group.Id +
                    ", Discovery Id: " + d.Id +
                    ", d.IsPresent: " + d.IsPresent +
                    //", d.AcquisitionDate: " + d.AcquisitionDate +
                    ", ((d.CoreCulturalDiscovery == null) || (!d.CoreCulturalDiscovery.IsPresent)): " 
                    + ((d.CoreCulturalDiscovery == null) || (!d.CoreCulturalDiscovery.IsPresent)) +
                    "");

                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
                }
            }
#endif

            if ((d.CoreCulturalDiscovery == null) || (!d.CoreCulturalDiscovery.IsPresent))
            {
                if (d.IsPresent)
                {
                    TryRemovingDiscovery(d, timeFactor);
                }
            }
            else
            {
                d.UpdateFromCoreDiscovery();
            }
        }

        Profiler.EndSample();
    }

    private void TryRemovingDiscovery(FactionCulturalDiscovery discovery, float timeFactor)
    {
        int idHash = discovery.Id.GetHashCode();
        
        if (GetNextRandomFloat(RngOffsets.FACTION_CULTURE_DISCOVERY_LOSS_CHANCE + idHash) < timeFactor)
        {
            RemoveDiscovery(discovery);
        }
    }

    public void SetPolityCulture(PolityCulture polityCulture)
    {
        foreach (FactionCulturalKnowledge k in Knowledges.Values)
        {
            k.SetPolityCulturalKnowledge(polityCulture);
        }

        foreach (FactionCulturalDiscovery d in Discoveries.Values)
        {
            d.SetPolityCulturalDiscovery(polityCulture);
        }
    }

    public void SetCoreCulture(CellCulture coreCulture)
    {
        SetCoreCultureKnowledges(coreCulture);
        SetCoreCultureDiscoveries(coreCulture);
    }

    public void SetCoreCultureKnowledges(CellCulture coreCulture)
    {
        foreach (FactionCulturalKnowledge k in Knowledges.Values)
        {
            k.CoreCulturalKnowledge = null;
        }

        foreach (CellCulturalKnowledge k in coreCulture.Knowledges.Values)
        {
            if (!k.IsPresent)
                continue;

            AddCoreKnowledge(k);
        }
    }

    public void AddCoreKnowledge(CellCulturalKnowledge coreKnowledge)
    {
        FactionCulturalKnowledge factionKnowledge = GetKnowledge(coreKnowledge.Id) as FactionCulturalKnowledge;

        if (factionKnowledge == null)
        {
            //#if DEBUG
            //            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
            //            {
            //                if (Manager.TracingData.FactionId == Faction.Id)
            //                {
            //                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
            //                        "FactionCulture:AddCoreKnowledge - Faction.Id:" + Faction.Id,
            //                        "CurrentDate: " + World.CurrentDate +
            //                        ", coreKnowledge.Group.Id: " + coreKnowledge.Group.Id +
            //                        ", coreKnowledge.Id: " + coreKnowledge.Id +
            //                        "");

            //                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            //                }
            //            }
            //#endif

            factionKnowledge = new FactionCulturalKnowledge(Faction, coreKnowledge, Faction.Polity.Culture);

            AddKnowledge(factionKnowledge);
        }
        else
        {
            factionKnowledge.Set();
        }

        factionKnowledge.CoreCulturalKnowledge = coreKnowledge;
    }

    public void SetCoreCultureDiscoveries(CellCulture coreCulture)
    {
        foreach (FactionCulturalDiscovery d in Discoveries.Values)
        {
            d.CoreCulturalDiscovery = null;
        }

        foreach (CellCulturalDiscovery d in coreCulture.Discoveries.Values)
        {
            if (!d.IsPresent)
                continue;

            AddCoreDiscovery(d);
        }
    }

    public void AddCoreDiscovery(CellCulturalDiscovery coreDiscovery)
    {
        FactionCulturalDiscovery factionDiscovery = GetDiscovery(coreDiscovery.Id) as FactionCulturalDiscovery;

        if (factionDiscovery == null)
        {
            factionDiscovery = new FactionCulturalDiscovery(Faction, coreDiscovery, Faction.Polity.Culture);

            AddDiscovery(factionDiscovery);
        }
        else
        {
            factionDiscovery.Set(true);
        }

        factionDiscovery.CoreCulturalDiscovery = coreDiscovery;
    }

    public override void FinalizePropertiesLoad()
    {
        foreach (FactionCulturalKnowledge k in Knowledges.Values)
        {
            k.Faction = Faction;
        }

        foreach (FactionCulturalDiscovery d in Discoveries.Values)
        {
            d.Faction = Faction;
        }

        base.FinalizePropertiesLoad();
    }
}
