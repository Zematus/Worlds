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
                AddKnowledge(new FactionCulturalKnowledge(faction, k, faction.Polity.Culture));
            }
        }

        foreach (CellCulturalDiscovery d in coreCulture.Discoveries.Values)
        {
            if (d.IsPresent)
            {
                AddDiscovery(new FactionCulturalDiscovery(d, faction.Polity.Culture));
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

#if DEBUG
            float prevValue = 0;
#endif

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

#if DEBUG
                prevValue = preference.Value;
#endif

                preference.Value = (preference.Value * (1f - timeFactor)) + (p.Value * timeFactor);

                Profiler.EndSample();
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

        foreach (FactionCulturalKnowledge k in Knowledges.Values)
        {
            k.UpdateFromCoreKnowledge(timeFactor);
        }

        Profiler.EndSample();

        ////// Update Discoveries

        Profiler.BeginSample("Culture - Update Discoveries");

        foreach (FactionCulturalDiscovery d in Discoveries.Values)
        {
            if ((d.CoreCulturalDiscovery == null) || (!d.CoreCulturalDiscovery.IsPresent))
            {
                TryRemovingDiscovery(d, timeFactor);
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
            AddCoreKnowledge(k);
        }
    }

    public void AddCoreKnowledge(CellCulturalKnowledge coreKnowledge)
    {
        FactionCulturalKnowledge factionKnowledge = GetKnowledge(coreKnowledge.Id) as FactionCulturalKnowledge;

        if (factionKnowledge == null)
        {
            factionKnowledge = new FactionCulturalKnowledge(Faction, coreKnowledge, Faction.Polity.Culture);

            AddKnowledge(factionKnowledge);
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
            AddCoreDiscovery(d);
        }
    }

    public void AddCoreDiscovery(CellCulturalDiscovery coreDiscovery)
    {
        FactionCulturalDiscovery factionDiscovery = GetDiscovery(coreDiscovery.Id) as FactionCulturalDiscovery;

        if (factionDiscovery == null)
        {
            factionDiscovery = new FactionCulturalDiscovery(coreDiscovery, Faction.Polity.Culture);

            AddDiscovery(factionDiscovery);
        }

        factionDiscovery.CoreCulturalDiscovery = coreDiscovery;
    }
}
