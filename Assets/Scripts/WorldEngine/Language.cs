using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Language {

	public delegate float GetRandomFloatDelegate ();

	public class Word {

		public WordType Type;
		public WordProperties Properties;

		public string String;
	}

	public enum WordType
	{
		Article,
		Noun
	}

	public enum GeneralArticleProperty
	{
		None = 0x00,
		CanBePlural = 0x01,
		CanBeGendered = 0x02,
		HasNeutral = 0x04,
		IsFemaleNeutral = 0x08,
		IsAppended = 0x10,
		IsPrefixed = 0x20,
		IsLinkedWithDash = 0x40,
	}

	public enum WordProperties
	{
		None = 0x00,
		IsPlural = 0x01,
		IsFemale = 0x02,
		IsNeutral = 0x04,
		CanBeContracted = 0x08
	}

	public static char[] OnsetLetters = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };

	public static char[] NucleusLetters = new char[] { 'a', 'e', 'i', 'o', 'u' };

	public static char[] CodaLetters = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };

	public class CharacterGroup : CollectionUtility.ElementWeightPair<string> {

		public CharacterGroup (string characters, float weight) {

			Value = characters;
			Weight = weight;
		}

		public string Characters {
			get { 
				return Value;
			}
		}
	}

	public GeneralArticleProperty GeneralArticleProperties;

	public string[] ArticleWordStartSyllables;
	public string[] ArticleWordNextSyllables;

	public string[] SimpleNounWordStartSyllables;
	public string[] SimpleNounWordNextSyllables;

	public Dictionary<string, Word> Articles = new Dictionary<string, Word> ();
	public Dictionary<string, Word> SimpleNouns = new Dictionary<string, Word> ();

	public void GenerateGeneralArticleProperties (GetRandomFloatDelegate getRandomFloat) {

		GeneralArticleProperties = GeneralArticleProperty.None;

		if (getRandomFloat () < 0.5f) {
		
			GeneralArticleProperties |= GeneralArticleProperty.CanBePlural;
		}

		if (getRandomFloat () < 0.5f) {

			GeneralArticleProperties |= GeneralArticleProperty.CanBeGendered;

			if (getRandomFloat () < 0.33f) {

				GeneralArticleProperties |= GeneralArticleProperty.HasNeutral;

			} else if (getRandomFloat () < 0.5f) {

				GeneralArticleProperties |= GeneralArticleProperty.IsFemaleNeutral;
			}
		}

		if (getRandomFloat () < 0.5f) {

			GeneralArticleProperties |= GeneralArticleProperty.IsAppended;

			if (getRandomFloat () < 0.5f) {

				GeneralArticleProperties |= GeneralArticleProperty.IsPrefixed;
			}

			if (getRandomFloat () < 0.5f) {

				GeneralArticleProperties |= GeneralArticleProperty.IsLinkedWithDash;
			}
		}
	}

	public void GenerateArticleSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.1f, getRandomFloat, 10);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 5);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.25f, 0.1f, getRandomFloat, 4);

		ArticleWordStartSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
		ArticleWordNextSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
	}

	public Word GenerateArticle (string key, WordProperties properties, GetRandomFloatDelegate getRandomFloat) {

		Word word = new Word ();
		word.String = GenerateSimpleWord (ArticleWordStartSyllables, ArticleWordNextSyllables, 0.1f, getRandomFloat);
		word.Properties = properties;
		word.Type = WordType.Article;

		Articles.Add (key, word);

		return word;
	}

	public void GenerateArticles (GetRandomFloatDelegate getRandomFloat) {

		WordProperties properties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;

		GenerateArticle ("singularMale", properties, getRandomFloat);

		if ((GeneralArticleProperties & GeneralArticleProperty.CanBeGendered) == GeneralArticleProperty.CanBeGendered) {
			properties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;
			properties |= WordProperties.IsFemale;

			GenerateArticle ("singularFemale", properties, getRandomFloat);

			if ((GeneralArticleProperties & GeneralArticleProperty.HasNeutral) == GeneralArticleProperty.HasNeutral) {
				properties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;
				properties |= WordProperties.IsNeutral;

				GenerateArticle ("singularNeutral", properties, getRandomFloat);
			}
		}

		if ((GeneralArticleProperties & GeneralArticleProperty.CanBePlural) == GeneralArticleProperty.CanBePlural) {
			properties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;
			properties |= WordProperties.IsPlural;

			GenerateArticle ("pluralMale", properties, getRandomFloat);

			if ((GeneralArticleProperties & GeneralArticleProperty.CanBeGendered) == GeneralArticleProperty.CanBeGendered) {
				properties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;
				properties |= WordProperties.IsFemale | WordProperties.IsPlural;

				GenerateArticle ("pluralFemale", properties, getRandomFloat);

				if ((GeneralArticleProperties & GeneralArticleProperty.HasNeutral) == GeneralArticleProperty.HasNeutral) {
					properties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;
					properties |= WordProperties.IsNeutral | WordProperties.IsPlural;

					GenerateArticle ("pluralNeutral", properties, getRandomFloat);
				}
			}
		}
	}

	public void GenerateSimpleNounSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.25f, getRandomFloat, 10);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 5);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.5f, 0.25f, getRandomFloat, 4);

		SimpleNounWordStartSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
		SimpleNounWordNextSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
	}

	public void GenerateSimpleNoun (string translation, WordProperties properties, GetRandomFloatDelegate getRandomFloat) {

		Word word = new Word ();
		word.String = GenerateSimpleWord (SimpleNounWordStartSyllables, SimpleNounWordNextSyllables, 0.25f, getRandomFloat);
		word.Properties = properties;
		word.Type = WordType.Noun;

		SimpleNouns.Add (translation, word);
	}

	public static CharacterGroup GenerateCharacterGroup (char[] characterSet, float startAddLetterChance, float addLetterChanceDecay, GetRandomFloatDelegate getRandomFloat) {

		float addLetterChance = startAddLetterChance;

		string characters = "";

		while (getRandomFloat () < addLetterChance) {
		
			int charIndex = (int)Mathf.Floor(characterSet.Length * getRandomFloat ());

			characters += characterSet [charIndex];

			addLetterChance *= addLetterChanceDecay;
		}

		return new CharacterGroup (characters, getRandomFloat ());
	}

	public static CharacterGroup[] GenerateCharacterGroups (char[] characterSet, float startAddLetterChance, float addLetterChanceDecay, GetRandomFloatDelegate getRandomFloat, int count) {

		CharacterGroup[] characterGroups = new CharacterGroup[count];

		for (int i = 0; i < count; i++) {
		
			characterGroups [i] = GenerateCharacterGroup (characterSet, startAddLetterChance, addLetterChanceDecay, getRandomFloat);
		}

		return characterGroups;
	}

	private static float GetCharacterGroupsTotalWeight (CharacterGroup[] charGroups) {
	
		float totalWeight = 0;

		foreach (CharacterGroup group in charGroups) {
		
			totalWeight += group.Weight;
		}

		return totalWeight;
	}

	public static string GenerateSyllable (CharacterGroup[] onsetGroups, CharacterGroup[] nucleusGroups, CharacterGroup[] codaGroups, GetRandomFloatDelegate getRandomFloat) {

		CollectionUtility.NormalizedValueGeneratorDelegate valueGeneratorDelegate = new CollectionUtility.NormalizedValueGeneratorDelegate (getRandomFloat);

		string onset = CollectionUtility.WeightedSelection (onsetGroups, GetCharacterGroupsTotalWeight (onsetGroups), valueGeneratorDelegate);
		string nucleus = CollectionUtility.WeightedSelection (nucleusGroups, GetCharacterGroupsTotalWeight (nucleusGroups), valueGeneratorDelegate);
		string coda = CollectionUtility.WeightedSelection (codaGroups, GetCharacterGroupsTotalWeight (codaGroups), valueGeneratorDelegate);

		return onset + nucleus + coda;
	}

	public static string[] GenerateSyllables (CharacterGroup[] onsetGroups, CharacterGroup[] nucleusGroups, CharacterGroup[] codaGroups, GetRandomFloatDelegate getRandomFloat, int maxCount) {

		HashSet<string> genSyllables = new HashSet<string> ();

		int index = 0;
		int genCount = 0;

		while (index < maxCount) {

			string syllable = GenerateSyllable (onsetGroups, nucleusGroups, codaGroups, getRandomFloat);

			if (genSyllables.Add (syllable))
				genCount++;

			index++;
		}

		string[] syllables = new string[genCount];

		genSyllables.CopyTo (syllables);

		return syllables;
	}

	public static string GenerateSimpleWord (string[] startSyllables, string[] nextSyllables, float addSyllableChanceDecay, GetRandomFloatDelegate getRandomFloat) {

		float addSyllableChance = 1;
		bool first = true;

		string word = "";

		while (getRandomFloat () < addSyllableChance) {

			string[] syllables = nextSyllables;

			if (first) {
			
				syllables = startSyllables;
				first = false;
			}

			int syllableIndex = (int)Mathf.Floor(syllables.Length * getRandomFloat ());

			word += syllables[syllableIndex];

			addSyllableChance *= addSyllableChanceDecay;
		}

		return word;
	}
}
