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

	public enum LanguageProperty 
	{
		HasDefiniteArticles = 0x01,
		HasIndefiniteArticles = 0x02
	}

	public enum ArticleProperties
	{
		None = 0x00,
		CanBePlural = 0x01,
		CanBeGendered = 0x02,
		HasNeutral = 0x04,
		IsFemenineNeutral = 0x08,
		IsAppended = 0x10,
		IsPrefixed = 0x20,
		IsLinkedWithDash = 0x40,
	}

	public enum WordProperties
	{
		None = 0x00,
		IsPlural = 0x01,
		IsFemenine = 0x02,
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

	public ArticleProperties DefiniteArticleProperties;
	public ArticleProperties IndefiniteArticleProperties;

	public string[] ArticleStartSyllables;
	public string[] ArticleNextSyllables;

	public string[] SimpleNounWordStartSyllables;
	public string[] SimpleNounWordNextSyllables;

	public Dictionary<string, Word> DefiniteArticles;
	public Dictionary<string, Word> IndefiniteArticles;
	public Dictionary<string, Word> SimpleNouns = new Dictionary<string, Word> ();

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

	public static ArticleProperties GenerateArticleProperties (GetRandomFloatDelegate getRandomFloat) {

		ArticleProperties articleProperties = ArticleProperties.None;

		if (getRandomFloat () < 0.5f) {
		
			articleProperties |= ArticleProperties.CanBePlural;
		}

		if (getRandomFloat () < 0.5f) {

			articleProperties |= ArticleProperties.CanBeGendered;

			if (getRandomFloat () < 0.33f) {

				articleProperties |= ArticleProperties.HasNeutral;

			} else if (getRandomFloat () < 0.5f) {

				articleProperties |= ArticleProperties.IsFemenineNeutral;
			}
		}

		if (getRandomFloat () < 0.5f) {

			articleProperties |= ArticleProperties.IsAppended;

			if (getRandomFloat () < 0.5f) {

				articleProperties |= ArticleProperties.IsPrefixed;
			}

			if (getRandomFloat () < 0.5f) {

				articleProperties |= ArticleProperties.IsLinkedWithDash;
			}
		}

		return articleProperties;
	}

	public static string ArticlePropertiesToString (ArticleProperties properties) {

		if (properties == ArticleProperties.None)
			return "None";

		string output = "";

		bool multipleProperties = false;

		if ((properties & ArticleProperties.CanBeGendered) == ArticleProperties.CanBeGendered) {
			
			output += "CanBeGendered";
			multipleProperties = true;
		}

		if ((properties & ArticleProperties.CanBePlural) == ArticleProperties.CanBePlural) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "CanBePlural";
			multipleProperties = true;
		}

		if ((properties & ArticleProperties.HasNeutral) == ArticleProperties.HasNeutral) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "HasNeutral";
			multipleProperties = true;
		}

		if ((properties & ArticleProperties.IsFemenineNeutral) == ArticleProperties.IsFemenineNeutral) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsFemenineNeutral";
			multipleProperties = true;
		}

		if ((properties & ArticleProperties.IsAppended) == ArticleProperties.IsAppended) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsAppended";
			multipleProperties = true;
		}

		if ((properties & ArticleProperties.IsPrefixed) == ArticleProperties.IsPrefixed) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsPrefixed";
			multipleProperties = true;
		}

		if ((properties & ArticleProperties.IsLinkedWithDash) == ArticleProperties.IsLinkedWithDash) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsLinkedWithDash";
			multipleProperties = true;
		}

		return output;
	}

	public static Word GenerateArticle (string[] startSyllables, string[] nextSyllables, WordProperties properties, GetRandomFloatDelegate getRandomFloat) {

		Word word = new Word ();
		word.String = GenerateSimpleWord (startSyllables, nextSyllables, 0.1f, getRandomFloat);
		word.Properties = properties;
		word.Type = WordType.Article;

		return word;
	}

	public static Dictionary<string, Word> GenerateArticles (string[] startSyllables, string[] nextSyllables, ArticleProperties articleProperties, GetRandomFloatDelegate getRandomFloat) {

		WordProperties wordProperties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;

		Dictionary<string, Word> articles = new Dictionary<string, Word> ();

		articles.Add ("singularMasculine", GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat));

		if ((articleProperties & ArticleProperties.CanBeGendered) == ArticleProperties.CanBeGendered) {
			wordProperties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;
			wordProperties |= WordProperties.IsFemenine;

			articles.Add ("singularFemenine", GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat));

			if ((articleProperties & ArticleProperties.HasNeutral) == ArticleProperties.HasNeutral) {
				wordProperties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;
				wordProperties |= WordProperties.IsNeutral;

				articles.Add ("singularNeutral", GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat));
			}
		}

		if ((articleProperties & ArticleProperties.CanBePlural) == ArticleProperties.CanBePlural) {
			wordProperties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;
			wordProperties |= WordProperties.IsPlural;

			articles.Add ("pluralMasculine", GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat));

			if ((articleProperties & ArticleProperties.CanBeGendered) == ArticleProperties.CanBeGendered) {
				wordProperties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;
				wordProperties |= WordProperties.IsFemenine | WordProperties.IsPlural;

				articles.Add ("pluralFemenine", GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat));

				if ((articleProperties & ArticleProperties.HasNeutral) == ArticleProperties.HasNeutral) {
					wordProperties = (getRandomFloat () < 0.5f) ? WordProperties.CanBeContracted : WordProperties.None;
					wordProperties |= WordProperties.IsNeutral | WordProperties.IsPlural;

					articles.Add ("pluralNeutral", GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat));
				}
			}
		}

		return articles;
	}

	public static WordProperties GenerateWordProperties (GetRandomFloatDelegate getRandomFloat, bool isPlural, bool randomGender = false, bool isFemenine = false, bool isNeutral = false) {

		WordProperties properties = WordProperties.None;

		if (isPlural) {
			properties |= WordProperties.IsPlural;
		}

		if (randomGender) {

			float chance = getRandomFloat ();

			if (chance >= 0.66f) {
				isNeutral = true;
			} else if (chance >= 0.33f) {
				isFemenine = true;
			}
		}

		if (isFemenine) {
			properties |= WordProperties.IsFemenine;
		}

		if (isNeutral) {
			properties |= WordProperties.IsNeutral;
		}

		if (getRandomFloat () >= 0.5f) {
			properties |= WordProperties.CanBeContracted;
		}

		return properties;
	}

	public void GenerateAllArticleProperties (GetRandomFloatDelegate getRandomFloat) {
	
		DefiniteArticleProperties = GenerateArticleProperties (getRandomFloat);
		IndefiniteArticleProperties = GenerateArticleProperties (getRandomFloat);
	}

	public void GenerateAllArticleSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.1f, getRandomFloat, 10);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 5);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.25f, 0.1f, getRandomFloat, 4);

		ArticleStartSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
		ArticleNextSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
	}

	public void GenerateAllArticles (GetRandomFloatDelegate getRandomFloat) {

		DefiniteArticles = GenerateArticles (ArticleStartSyllables, ArticleNextSyllables, DefiniteArticleProperties, getRandomFloat);
		IndefiniteArticles = GenerateArticles (ArticleStartSyllables, ArticleNextSyllables, IndefiniteArticleProperties, getRandomFloat);
	}

	public void GenerateSimpleNounSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.25f, getRandomFloat, 10);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 5);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.5f, 0.25f, getRandomFloat, 4);

		SimpleNounWordStartSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
		SimpleNounWordNextSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
	}

	public void GenerateSimpleNoun (string translation, GetRandomFloatDelegate getRandomFloat, bool isPlural, bool randomGender, bool isFemenine = false, bool isNeutral = false) {

		GenerateSimpleNoun (translation, GenerateWordProperties (getRandomFloat, isPlural, randomGender, isFemenine, isNeutral), getRandomFloat);
	}

	public void GenerateSimpleNoun (string translation, WordProperties properties, GetRandomFloatDelegate getRandomFloat) {

		Word word = new Word ();
		word.String = GenerateSimpleWord (SimpleNounWordStartSyllables, SimpleNounWordNextSyllables, 0.25f, getRandomFloat);
		word.Properties = properties;
		word.Type = WordType.Noun;

		SimpleNouns.Add (translation, word);
	}
}
