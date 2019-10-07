using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityCulture : Culture
{
    [XmlIgnore]
    public Polity Polity;

    public PolityCulture()
    {

    }

    public PolityCulture(Polity polity) : base(polity.World)
    {
        Polity = polity;

        CellGroup coreGroup = Polity.CoreGroup;

        if (coreGroup == null)
            throw new System.Exception("CoreGroup can't be null at this point");

        CellCulture coreCulture = coreGroup.Culture;

        Language = coreCulture.Language;

        if (Language == null)
        {
            GenerateNewLanguage();
        }
    }

    public void Initialize()
    {
        AddFactionCultures();
    }

    private void GenerateNewLanguage()
    {
        Language = new Language(Polity.GenerateUniqueIdentifier(World.CurrentDate, 100L, Polity.Id));

        // Generate Articles

        Language.GenerateArticleProperties();

        Language.GenerateArticleAdjunctionProperties();
        Language.GenerateArticleSyllables();
        Language.GenerateAllArticles();

        // Generate Noun Indicatives

        Language.GenerateNounIndicativeProperties();

        Language.GenerateNounIndicativeAdjunctionProperties();
        Language.GenerateNounIndicativeSyllables();
        Language.GenerateAllNounIndicatives();

        // Generate Verb Indicatives

        Language.GenerateVerbIndicativeProperties();

        Language.GenerateVerbIndicativeAdjunctionProperties();
        Language.GenerateVerbIndicativeSyllables();
        Language.GenerateAllVerbIndicatives();

        // Generate Noun, Adjective and Adposition Properties and Syllables

        Language.GenerateVerbSyllables();

        Language.GenerateNounAdjunctionProperties();
        Language.GenerateNounSyllables();

        Language.GenerateAdjectiveAdjunctionProperties();
        Language.GenerateAdjectiveSyllables();

        Language.GenerateAdpositionAdjunctionProperties();
        Language.GenerateAdpositionSyllables();

        World.AddLanguage(Language);
    }

    public void Update()
    {
        Profiler.BeginSample("Clear Attributes");

        ResetAttributes();

        Profiler.EndSample();

        Profiler.BeginSample("Add Faction Cultures");

        AddFactionCultures();

        Profiler.EndSample();
    }

    private void FinalizeUpdateFromFactions()
    {
        List<CulturalKnowledge> knowledges = new List<CulturalKnowledge>(_knowledges.Values);

        foreach (CulturalKnowledge k in knowledges)
        {
            var knowledge = k as PolityCulturalKnowledge;

            if (knowledge == null)
            {
                throw new System.Exception("FinalizeUpdateFromFactions: CulturalKnowledge is not a PolityCulturalKnowledge. Polity Id: " + Polity.Id + ", knowledge Id: " + k.Id);
            }

            knowledge.FinalizeUpdateFromFactions();

            // This knowledge might no longer be present on any of the influencing factions and thus 
            // we should remove it from the polity culture
            if (k.Value <= 0)
            {
                RemoveKnowledge(k);
            }
        }
    }

    private void AddFactionCultures()
    {
        foreach (Faction faction in Polity.GetFactions())
        {
            Profiler.BeginSample("AddFactionCulture");

            AddFactionCulture(faction);

            Profiler.EndSample();
        }

        FinalizeUpdateFromFactions();
    }

    private void AddFactionCulture(Faction faction)
    {
        float influence = faction.Influence;

        //Profiler.BeginSample("foreach CulturalPreference");

        foreach (CulturalPreference p in faction.Culture.GetPreferences())
        {
            //Profiler.BeginSample("GetPreference");

            CulturalPreference preference = GetPreference(p.Id);

            //Profiler.EndSample();

            if (preference == null)
            {
                //Profiler.BeginSample("AddPreference");

                preference = new CulturalPreference(p);
                preference.Value *= influence;

                AddPreference(preference);

                //Profiler.EndSample();
            }
            else
            {
                //Profiler.BeginSample("update preference value");

                preference.Value += p.Value * influence;

                //Profiler.EndSample();
            }
        }

        //Profiler.EndSample();

        //Profiler.BeginSample("foreach CulturalActivity");

        foreach (CulturalActivity a in faction.Culture.GetActivities())
        {
            //Profiler.BeginSample("GetActivity");

            CulturalActivity activity = GetActivity(a.Id);

            //Profiler.EndSample();

            if (activity == null)
            {
                //Profiler.BeginSample("AddActivity");

                activity = new CulturalActivity(a);
                activity.Value *= influence;
                activity.Contribution *= influence;

                AddActivity(activity);

                //Profiler.EndSample();
            }
            else
            {
                //Profiler.BeginSample("update activity value");

                activity.Value += a.Value * influence;
                activity.Contribution += a.Contribution * influence;

                //Profiler.EndSample();
            }
        }

        //Profiler.EndSample();

        //Profiler.BeginSample("foreach CulturalSkill");

        foreach (CulturalSkill s in faction.Culture.GetSkills())
        {
            //Profiler.BeginSample("GetSkill");

            CulturalSkill skill = GetSkill(s.Id);

            //Profiler.EndSample();

            if (skill == null)
            {
                //Profiler.BeginSample("AddSkill");

                skill = new CulturalSkill(s);
                skill.Value *= influence;

                AddSkill(skill);

                //Profiler.EndSample();
            }
            else
            {
                //Profiler.BeginSample("update skill value");

                skill.Value += s.Value * influence;

                //Profiler.EndSample();
            }
        }

        //Profiler.EndSample();

        //Profiler.BeginSample("foreach CulturalKnowledge");

        foreach (CulturalKnowledge k in faction.Culture.GetKnowledges())
        {
            //Profiler.BeginSample("GetKnowledge");

            PolityCulturalKnowledge knowledge = GetKnowledge(k.Id) as PolityCulturalKnowledge;

            //Profiler.EndSample();

            if (knowledge == null)
            {
                //Profiler.BeginSample("AddKnowledge");

                knowledge = new PolityCulturalKnowledge(k);
                knowledge.AccValue += k.Value * influence;

                AddKnowledge(knowledge);

                //Profiler.EndSample();
            }
            else
            {
                //Profiler.BeginSample("update knowledge value");
                
                knowledge.AccValue += k.Value * influence;

                //Profiler.EndSample();
            }
        }

        //Profiler.EndSample();

        //Profiler.BeginSample("foreach CulturalDiscovery");

        foreach (Discovery d in faction.Culture.Discoveries.Values)
        {
            AddDiscovery(d);
        }

        //Profiler.EndSample();
    }
}
