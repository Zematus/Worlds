using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityCulture : Culture {

	[XmlIgnore]
	public Polity Polity;

	public PolityCulture () {
	
	}

	public PolityCulture (Polity polity) : base (polity.World) {

		Polity = polity;

		CellGroup coreGroup = Polity.CoreGroup;

		if (coreGroup == null)
			throw new System.Exception ("CoreGroup can't be null at this point");

		CellCulture coreCulture = coreGroup.Culture;

		Language = coreCulture.Language;

		if (Language == null) {

			GenerateNewLanguage ();
		}
	}

	public void Initialize () {
	
		AddFactionCultures ();
	}

	private void GenerateNewLanguage () {

		Language = new Language (Polity.GenerateUniqueIdentifier (World.CurrentDate, 100L, Polity.Id));

		// Generate Articles

		Language.GenerateArticleProperties ();

		Language.GenerateArticleAdjunctionProperties ();
		Language.GenerateArticleSyllables ();
		Language.GenerateAllArticles ();

		// Generate Noun Indicatives

		Language.GenerateNounIndicativeProperties ();

		Language.GenerateNounIndicativeAdjunctionProperties ();
		Language.GenerateNounIndicativeSyllables ();
		Language.GenerateAllNounIndicatives ();

		// Generate Verb Indicatives

		Language.GenerateVerbIndicativeProperties ();

		Language.GenerateVerbIndicativeAdjunctionProperties ();
		Language.GenerateVerbIndicativeSyllables ();
		Language.GenerateAllVerbIndicatives ();

		// Generate Noun, Adjective and Adposition Properties and Syllables

		Language.GenerateVerbSyllables ();

		Language.GenerateNounAdjunctionProperties ();
		Language.GenerateNounSyllables ();

		Language.GenerateAdjectiveAdjunctionProperties ();
		Language.GenerateAdjectiveSyllables ();

		Language.GenerateAdpositionAdjunctionProperties ();
		Language.GenerateAdpositionSyllables ();

		World.AddLanguage (Language);
	}

	public void Update () {

		ClearAttributes ();

		AddFactionCultures ();
	}

	private void AddFactionCultures () {

		foreach (Faction faction in Polity.GetFactions ()) {
		
			AddFactionCulture (faction);
		}
	}

	private void AddFactionCulture (Faction faction) {

		float prominence = faction.Prominence;

		foreach (CulturalPreference p in faction.Culture.Preferences) {

			CulturalPreference preference = GetPreference (p.Id);

			if (preference == null) {

				preference = new CulturalPreference (p);
				preference.Value *= prominence;

				AddPreference (preference);

			} else {

				preference.Value += p.Value * prominence;
			}
		}

		foreach (CulturalActivity a in faction.Culture.Activities) {
		
			CulturalActivity activity = GetActivity (a.Id);

			if (activity == null) {
			
				activity = new CulturalActivity (a);
				activity.Value *= prominence;
				activity.Contribution *= prominence;

				AddActivity (activity);

			} else {
			
				activity.Value += a.Value * prominence;
				activity.Contribution += a.Contribution * prominence;
			}
		}

		foreach (CulturalSkill s in faction.Culture.Skills) {

			CulturalSkill skill = GetSkill (s.Id);

			if (skill == null) {

				skill = new CulturalSkill (s);
				skill.Value *= prominence;

				AddSkill (skill);

			} else {

				skill.Value += s.Value * prominence;
			}
		}

		foreach (CulturalKnowledge k in faction.Culture.Knowledges) {

			CulturalKnowledge knowledge = GetKnowledge (k.Id);

			if (knowledge == null) {

				knowledge = new CulturalKnowledge (k);
				knowledge.Value = (int)(k.Value * prominence);

				AddKnowledge (knowledge);

			} else {
				
				knowledge.Value += (int)(k.Value * prominence);
			}
		}

		foreach (CulturalDiscovery groupDiscovery in faction.Culture.Discoveries) {

			CulturalDiscovery discovery = GetDiscovery (groupDiscovery.Id) as CulturalDiscovery;

			if (discovery == null) {

				discovery = new CulturalDiscovery (groupDiscovery);

				AddDiscovery (discovery);
			}
		}
	}
}
