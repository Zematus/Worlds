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
        Language = new Language(World.CurrentDate, Polity.GenerateInitId(Polity.GetHashCode()));

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
        ResetAttributes();

        AddFactionCultures();
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
            AddFactionCulture(faction);
        }

        FinalizeUpdateFromFactions();
    }

    private void AddFactionCulture(Faction faction)
    {
        float influence = faction.Influence;

        foreach (CulturalPreference p in faction.Culture.GetPreferences())
        {
            CulturalPreference preference = GetPreference(p.Id);

            if (preference == null)
            {
                preference = new CulturalPreference(p);
                preference.Value *= influence;

                AddPreference(preference);
            }
            else
            {
                preference.Value += p.Value * influence;
            }
        }

        foreach (CulturalActivity a in faction.Culture.GetActivities())
        {
            CulturalActivity activity = GetActivity(a.Id);

            if (activity == null)
            {
                activity = new CulturalActivity(a);
                activity.Value *= influence;
                activity.Contribution *= influence;

                AddActivity(activity);
            }
            else
            {
                activity.Value += a.Value * influence;
                activity.Contribution += a.Contribution * influence;
            }
        }

        foreach (CulturalSkill s in faction.Culture.GetSkills())
        {
            CulturalSkill skill = GetSkill(s.Id);

            if (skill == null)
            {
                skill = new CulturalSkill(s);
                skill.Value *= influence;

                AddSkill(skill);
            }
            else
            {
                skill.Value += s.Value * influence;
            }
        }

        foreach (CulturalKnowledge k in faction.Culture.GetKnowledges())
        {
            PolityCulturalKnowledge knowledge = GetKnowledge(k.Id) as PolityCulturalKnowledge;

            if (knowledge == null)
            {
                knowledge = new PolityCulturalKnowledge(k);
                knowledge.AccValue += k.Value * influence;

                AddKnowledge(knowledge);
            }
            else
            {
                knowledge.AccValue += k.Value * influence;
            }
        }

        foreach (var d in faction.Culture.Discoveries.Values)
        {
            AddDiscovery(d);
        }
    }
}
