using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class FactionCulture : Culture {

	[XmlIgnore]
	public Faction Faction;

//	[XmlIgnore]
//	private float _totalGroupInfluenceValue;

	public FactionCulture () {
	
	}

	public FactionCulture (Faction faction) : base (faction.World) {

		Faction = faction;

		CellGroup coreGroup = Faction.CoreGroup;

		if (coreGroup == null)
			throw new System.Exception ("CoreGroup can't be null at this point");

		CellCulture coreCulture = coreGroup.Culture;

		foreach (CulturalPreference p in coreCulture.Preferences) {
			AddPreference (new CulturalPreference (p));
		}

		foreach (CulturalActivity a in coreCulture.Activities) {
			AddActivity (new CulturalActivity (a));
		}

		foreach (CulturalSkill s in coreCulture.Skills) {
			AddSkill (new CulturalSkill (s));
		}

		foreach (CulturalKnowledge k in coreCulture.Knowledges) {
			AddKnowledge (new CulturalKnowledge (k));
		}

		foreach (CulturalDiscovery d in coreCulture.Discoveries) {
			AddDiscovery (new CulturalDiscovery (d));
		}

		Language = coreCulture.Language;

		if (Language == null) {
		
			GenerateNewLanguage ();
		}
	}

	public float GetNextRandomFloat (int rngOffset) {

		return Faction.GetNextLocalRandomFloat (rngOffset);
	}

	private void GenerateNewLanguage () {

		Language = new Language (Faction.GenerateUniqueIdentifier (World.CurrentDate, 100L, Faction.Id));

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
	}
}
