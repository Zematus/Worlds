using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class Language {

	public delegate float GetRandomFloatDelegate ();

	public class Phrase {

		public string Text;
		public string Meaning;
	}

	public class Word {

		public WordType Type;
		public WordProperties Properties;

		public string String;
	}

	public enum WordType
	{
		Article,
		Adposition,
		Noun
	}

	public enum GeneralProperties 
	{
		HasDefiniteArticles = 0x01,
		HasIndefiniteArticles = 0x02
	}

	public enum NounAdjuntProperties
	{
		None = 0x0000,
		CanBePlural = 0x0001,
		IsGendered = 0x0002,
		HasNeutral = 0x0004,
		IsFemenineNeutral = 0x0008,
		IsAppended = 0x0010,
		GoesAfterNoun = 0x0020,
		IsLinkedWithDash = 0x0040,
	}

	public enum WordProperties
	{
		None = 0x00,
		IsPlural = 0x01,
		IsFemenine = 0x02,
		IsNeutral = 0x04
	}

	public static class ArticleTypes
	{
		public const string SingularMasculine = "SingularMasculine";
		public const string SingularFemenine = "SingularFemenine";
		public const string SingularNeutral = "SingularNeutral";
		public const string PluralMasculine = "PluralMasculine";
		public const string PluralFemenine = "PluralFemenine";
		public const string PluralNeutral = "PluralNeutral";
	}

	public static char[] OnsetLetters = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };

	public static char[] NucleusLetters = new char[] { 'a', 'e', 'i', 'o', 'u' };

	public static char[] CodaLetters = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };

	private static Regex startsWithNucleus = new Regex (@"^[aeiou]w*"); 

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

	public GeneralProperties Properties;

	public NounAdjuntProperties DefiniteArticleProperties;
	public NounAdjuntProperties IndefiniteArticleProperties;

	public NounAdjuntProperties AdpositionProperties;

	public string[] ArticleStartSyllables;
	public string[] ArticleNextSyllables;

	public string[] AdpositionStartSyllables;
	public string[] AdpositionNextSyllables;

	public string[] SimpleNounWordStartSyllables;
	public string[] SimpleNounWordNextSyllables;

	public Dictionary<string, Word> DefiniteArticles;
	public Dictionary<string, Word> IndefiniteArticles;
	public Dictionary<string, Word> Adpositions = new Dictionary<string, Word> ();
	public Dictionary<string, Word> Nouns = new Dictionary<string, Word> ();

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

			addSyllableChance *= addSyllableChanceDecay * addSyllableChanceDecay;
		}

		return word;
	}

	public static NounAdjuntProperties GenerateNounAdjuntProperties (GetRandomFloatDelegate getRandomFloat) {

		NounAdjuntProperties properties = NounAdjuntProperties.None;

		if (getRandomFloat () < 0.5f) {
		
			properties |= NounAdjuntProperties.CanBePlural;
		}

		if (getRandomFloat () < 0.5f) {

			properties |= NounAdjuntProperties.IsGendered;

			if (getRandomFloat () < 0.33f) {

				properties |= NounAdjuntProperties.HasNeutral;

			} else if (getRandomFloat () < 0.5f) {

				properties |= NounAdjuntProperties.IsFemenineNeutral;
			}
		}

		if (getRandomFloat () < 0.5f) {

			properties |= NounAdjuntProperties.IsAppended;

			if (getRandomFloat () < 0.5f) {

				properties |= NounAdjuntProperties.GoesAfterNoun;
			}

			if (getRandomFloat () < 0.25f) {

				properties |= NounAdjuntProperties.IsLinkedWithDash;
			}
		}

		return properties;
	}

	public static string NounAdjuntPropertiesToString (NounAdjuntProperties properties) {

		if (properties == NounAdjuntProperties.None)
			return "None";

		string output = "";

		bool multipleProperties = false;

		if ((properties & NounAdjuntProperties.IsGendered) == NounAdjuntProperties.IsGendered) {
			
			output += "IsGendered";
			multipleProperties = true;
		}

		if ((properties & NounAdjuntProperties.CanBePlural) == NounAdjuntProperties.CanBePlural) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "CanBePlural";
			multipleProperties = true;
		}

		if ((properties & NounAdjuntProperties.HasNeutral) == NounAdjuntProperties.HasNeutral) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "HasNeutral";
			multipleProperties = true;
		}

		if ((properties & NounAdjuntProperties.IsFemenineNeutral) == NounAdjuntProperties.IsFemenineNeutral) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsFemenineNeutral";
			multipleProperties = true;
		}

		if ((properties & NounAdjuntProperties.IsAppended) == NounAdjuntProperties.IsAppended) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsAppended";
			multipleProperties = true;
		}

		if ((properties & NounAdjuntProperties.GoesAfterNoun) == NounAdjuntProperties.GoesAfterNoun) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "GoesAfterNoun";
			multipleProperties = true;
		}

		if ((properties & NounAdjuntProperties.IsLinkedWithDash) == NounAdjuntProperties.IsLinkedWithDash) {

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
		word.String = GenerateSimpleWord (startSyllables, nextSyllables, 0.0f, getRandomFloat);
		word.Properties = properties;
		word.Type = WordType.Article;

		return word;
	}

	public static Dictionary<string, Word> GenerateArticles (string[] startSyllables, string[] nextSyllables, NounAdjuntProperties articleProperties, GetRandomFloatDelegate getRandomFloat) {

		WordProperties wordProperties = WordProperties.None;

		Dictionary<string, Word> articles = new Dictionary<string, Word> ();

		Word singularMasculine = GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat);
		Word singularFemenine = singularMasculine;
		Word singularNeutral = singularMasculine;
		Word pluralMasculine = singularMasculine;
		Word pluralFemenine = singularMasculine;
		Word pluralNeutral = singularMasculine;

		if ((articleProperties & NounAdjuntProperties.IsGendered) == NounAdjuntProperties.IsGendered) {
			wordProperties = WordProperties.IsFemenine;

			singularFemenine = GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat);
			pluralFemenine = singularFemenine;

			if ((articleProperties & NounAdjuntProperties.HasNeutral) == NounAdjuntProperties.HasNeutral) {
				wordProperties = WordProperties.IsNeutral;

				singularNeutral = GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat);
				pluralNeutral = singularNeutral;
			}
		}

		if ((articleProperties & NounAdjuntProperties.CanBePlural) == NounAdjuntProperties.CanBePlural) {
			wordProperties = WordProperties.IsPlural;

			pluralMasculine = GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat);
			pluralFemenine = pluralMasculine;
			pluralNeutral = pluralMasculine;

			if ((articleProperties & NounAdjuntProperties.IsGendered) == NounAdjuntProperties.IsGendered) {
				wordProperties = WordProperties.IsFemenine | WordProperties.IsPlural;

				pluralFemenine = GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat);

				if ((articleProperties & NounAdjuntProperties.HasNeutral) == NounAdjuntProperties.HasNeutral) {
					wordProperties = WordProperties.IsNeutral | WordProperties.IsPlural;

					pluralNeutral = GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat);
				}
			}
		}

		if ((articleProperties & NounAdjuntProperties.IsFemenineNeutral) == NounAdjuntProperties.IsFemenineNeutral) {

			singularNeutral = singularFemenine;
			pluralNeutral = pluralFemenine;
		}

		articles.Add (ArticleTypes.SingularMasculine, singularMasculine);
		articles.Add (ArticleTypes.SingularFemenine, singularFemenine);
		articles.Add (ArticleTypes.SingularNeutral, singularNeutral);
		articles.Add (ArticleTypes.PluralMasculine, pluralMasculine);
		articles.Add (ArticleTypes.PluralFemenine, pluralFemenine);
		articles.Add (ArticleTypes.PluralNeutral, pluralNeutral);

		return articles;
	}

	public static Dictionary<string, Word> GenerateArticles2 (string[] startSyllables, string[] nextSyllables, NounAdjuntProperties articleProperties, GetRandomFloatDelegate getRandomFloat) {

		WordProperties wordProperties = WordProperties.None;

		Dictionary<string, Word> articles = new Dictionary<string, Word> ();

		Word baseArticle = GenerateArticle (startSyllables, nextSyllables, wordProperties, getRandomFloat);

		return null;
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

		return properties;
	}

	public static string WordPropertiesToString (WordProperties properties) {

		if (properties == WordProperties.None)
			return "None";

		string output = "";

		bool multipleProperties = false;

		if ((properties & WordProperties.IsFemenine) == WordProperties.IsFemenine) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsFemenine";
			multipleProperties = true;
		}

		if ((properties & WordProperties.IsNeutral) == WordProperties.IsNeutral) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsNeutral";
			multipleProperties = true;
		}

		if ((properties & WordProperties.IsPlural) == WordProperties.IsPlural) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsPlural";
			multipleProperties = true;
		}

		return output;
	}

	public void GenerateLanguageProperties (GetRandomFloatDelegate getRandomFloat) {

		if (getRandomFloat () < 0.75f) {

			Properties |= GeneralProperties.HasDefiniteArticles;
		}

		if (getRandomFloat () < 0.75f) {

			Properties |= GeneralProperties.HasIndefiniteArticles;
		}
	}

	public void GenerateAllArticleProperties (GetRandomFloatDelegate getRandomFloat) {

		if ((Properties & GeneralProperties.HasDefiniteArticles) == GeneralProperties.HasDefiniteArticles)
			DefiniteArticleProperties = GenerateNounAdjuntProperties (getRandomFloat);

		if ((Properties & GeneralProperties.HasIndefiniteArticles) == GeneralProperties.HasIndefiniteArticles)
		IndefiniteArticleProperties = GenerateNounAdjuntProperties (getRandomFloat);
	}

	public void GenerateAllArticleSyllables (GetRandomFloatDelegate getRandomFloat) {

		if (((Properties & GeneralProperties.HasDefiniteArticles) != GeneralProperties.HasDefiniteArticles) &&
		    ((Properties & GeneralProperties.HasIndefiniteArticles) != GeneralProperties.HasIndefiniteArticles))
			return;

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.1f, getRandomFloat, 10);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 5);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.25f, 0.1f, getRandomFloat, 4);

		ArticleStartSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
		ArticleNextSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
	}

	public void GenerateAllArticles (GetRandomFloatDelegate getRandomFloat) {

		if ((Properties & GeneralProperties.HasDefiniteArticles) == GeneralProperties.HasDefiniteArticles)
			DefiniteArticles = GenerateArticles (ArticleStartSyllables, ArticleNextSyllables, DefiniteArticleProperties, getRandomFloat);

		if ((Properties & GeneralProperties.HasIndefiniteArticles) == GeneralProperties.HasIndefiniteArticles)
			IndefiniteArticles = GenerateArticles (ArticleStartSyllables, ArticleNextSyllables, IndefiniteArticleProperties, getRandomFloat);
	}

	public void GenerateAdpositionProperties (GetRandomFloatDelegate getRandomFloat) {

		AdpositionProperties = GenerateNounAdjuntProperties (getRandomFloat);
	}

	public void GenerateAdpositionSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.1f, getRandomFloat, 20);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 10);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.25f, 0.1f, getRandomFloat, 8);

		AdpositionStartSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 100);
		AdpositionNextSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 100);
	}

	public void GenerateAdposition (string relation, GetRandomFloatDelegate getRandomFloat) {

		Word word = new Word ();
		word.String = GenerateSimpleWord (AdpositionStartSyllables, AdpositionNextSyllables, 0.5f, getRandomFloat);
		word.Properties = WordProperties.None;
		word.Type = WordType.Adposition;

		Adpositions.Add (relation, word);
	}

	public void GenerateSimpleNounSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.25f, getRandomFloat, 20);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 10);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.5f, 0.25f, getRandomFloat, 8);

		SimpleNounWordStartSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 200);
		SimpleNounWordNextSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 200);
	}

	public void GenerateSimpleNoun (string meaning, GetRandomFloatDelegate getRandomFloat, bool isPlural, bool randomGender, bool isFemenine = false, bool isNeutral = false) {

		GenerateSimpleNoun (meaning, GenerateWordProperties (getRandomFloat, isPlural, randomGender, isFemenine, isNeutral), getRandomFloat);
	}

	public void GenerateSimpleNoun (string meaning, WordProperties properties, GetRandomFloatDelegate getRandomFloat) {

		Word word = new Word ();
		word.String = GenerateSimpleWord (SimpleNounWordStartSyllables, SimpleNounWordNextSyllables, 0.5f, getRandomFloat);
		word.Properties = properties;
		word.Type = WordType.Noun;

		Nouns.Add (meaning, word);
	}

	public Word GetAppropiateArticle (WordProperties nounProperties, bool useIndefiniteArticles) {

		Dictionary <string, Word> articles = DefiniteArticles;

		if (useIndefiniteArticles)
			articles = IndefiniteArticles;

		Word article = null;

		if ((nounProperties & WordProperties.IsPlural) == WordProperties.IsPlural) {

			if ((nounProperties & WordProperties.IsFemenine) == WordProperties.IsFemenine) {

				article = articles [ArticleTypes.PluralFemenine];

			} else if ((nounProperties & WordProperties.IsNeutral) == WordProperties.IsNeutral) {

				article = articles [ArticleTypes.PluralNeutral];

			} else {

				article = articles [ArticleTypes.PluralMasculine];

			}
		} else {

			if ((nounProperties & WordProperties.IsFemenine) == WordProperties.IsFemenine) {

				article = articles [ArticleTypes.SingularFemenine];

			} else if ((nounProperties & WordProperties.IsNeutral) == WordProperties.IsNeutral) {

				article = articles [ArticleTypes.SingularNeutral];

			} else {

				article = articles [ArticleTypes.SingularMasculine];

			}
		}

		return article;
	}

	public Phrase BuildArticulatedNounPhrase (string noun, bool useIndefiniteArticles) {

		Phrase phrase = new Phrase ();
	
		Word word = null;

		if (!Nouns.TryGetValue (noun, out word)) {

			return phrase;
		}

		string meaning;

		if (useIndefiniteArticles) {

			if ((word.Properties & WordProperties.IsPlural) == WordProperties.IsPlural) {
				meaning = noun;

			} else {

				if (startsWithNucleus.IsMatch (noun)) {
					meaning = "an " + noun;
				} else {
					meaning = "a " + noun;
				}
			}

		} else {
			
			meaning = "the " + noun;
		}

		string text = word.String;

		NounAdjuntProperties articleProperties;

		bool hasArticles = false;

		if (useIndefiniteArticles) {
			if ((Properties & GeneralProperties.HasIndefiniteArticles) == GeneralProperties.HasIndefiniteArticles) {

				hasArticles = true;
			}

			articleProperties = IndefiniteArticleProperties;

		} else {
			if ((Properties & GeneralProperties.HasDefiniteArticles) == GeneralProperties.HasDefiniteArticles) {

				hasArticles = true;
			}

			articleProperties = DefiniteArticleProperties;
		}

		if (hasArticles) {
			Word article = GetAppropiateArticle (word.Properties, useIndefiniteArticles);
			string articleString = article.String;
		
			if ((articleProperties & NounAdjuntProperties.GoesAfterNoun) == NounAdjuntProperties.GoesAfterNoun) {
		
				if ((articleProperties & NounAdjuntProperties.IsAppended) == NounAdjuntProperties.IsAppended) {

					if ((articleProperties & NounAdjuntProperties.IsLinkedWithDash) == NounAdjuntProperties.IsLinkedWithDash) {

						text += "-" + articleString;
					} else {
					
						text += articleString;
					}
				} else {

					text += " " + articleString;
				}
			} else {
				if ((articleProperties & NounAdjuntProperties.IsAppended) == NounAdjuntProperties.IsAppended) {

					if ((articleProperties & NounAdjuntProperties.IsLinkedWithDash) == NounAdjuntProperties.IsLinkedWithDash) {

						text = articleString + "-" + text;
					} else {

						text = articleString + text;
					}
				} else {

					text = articleString + " " + text;
				}
			}
		}

		phrase.Meaning = meaning;
		phrase.Text = text;

		return phrase;
	}

	public Phrase BuildAdpositionalPhrase (string relation, Phrase complementPhrase) {

		Phrase phrase = new Phrase ();

		Word adposition = null;

		if (!Adpositions.TryGetValue (relation, out adposition)) {

			return phrase;
		}

		string meaning = relation + " " + complementPhrase.Meaning;

		string text = complementPhrase.Text;

		if ((AdpositionProperties & NounAdjuntProperties.GoesAfterNoun) == NounAdjuntProperties.GoesAfterNoun) {

			if ((AdpositionProperties & NounAdjuntProperties.IsAppended) == NounAdjuntProperties.IsAppended) {

				if ((AdpositionProperties & NounAdjuntProperties.IsLinkedWithDash) == NounAdjuntProperties.IsLinkedWithDash) {

					text += "-" + adposition.String;
				} else {

					text += adposition.String;
				}
			} else {

				text += " " + adposition.String;
			}
		} else {
			if ((AdpositionProperties & NounAdjuntProperties.IsAppended) == NounAdjuntProperties.IsAppended) {

				if ((AdpositionProperties & NounAdjuntProperties.IsLinkedWithDash) == NounAdjuntProperties.IsLinkedWithDash) {

					text = adposition.String + "-" + text;
				} else {

					text = adposition.String + text;
				}
			} else {

				text = adposition.String + " " + text;
			}
		}

		phrase.Meaning = meaning;
		phrase.Text = text;

		return phrase;
	}

	public Phrase MergePhrases (Phrase prePhrase, Phrase postPhrase) {

		Phrase phrase = new Phrase ();

		phrase.Meaning = prePhrase.Meaning + " " + postPhrase.Meaning;
		phrase.Text = prePhrase.Text + " " + postPhrase.Text;

		return phrase;
	}

	// For now it will only make the first letter in the phrase uppercase
	public void LocalizePhrase (Phrase phrase) {

		phrase.Text = phrase.Text.First().ToString().ToUpper() + phrase.Text.Substring(1);
		phrase.Meaning = phrase.Meaning.First().ToString().ToUpper() + phrase.Meaning.Substring(1);
	}
}
