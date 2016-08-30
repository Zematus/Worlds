using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Serialization;

public class Language : ISynchronizable {

	[XmlAttribute]
	public long Id;

	public delegate float GetRandomFloatDelegate ();

	public class Phrase {

		[XmlAttribute]
		public string Original;
		[XmlAttribute]
		public string Meaning;
		[XmlAttribute]
		public string Text;
	}

	public class NounPhrase : Phrase {

		public PhraseProperties Properties;
	}

	public class Word {

		[XmlAttribute]
		public string Meaning;
		[XmlAttribute]
		public string Value;

		public WordType Type;
		public WordProperties Properties;
	}

	public class ParsedWord {

		public string Value;
		public Dictionary<string, string> Attributes = new Dictionary<string, string> ();
	}

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

	public static class ParsedWordAttributeId {

		public const string NounPluralIndicative = "npl";
		public const string FemenineNoun = "fn";
		public const string MasculineNoun = "mn";
		public const string NeutralNoun = "nn";
		public const string IrregularPluralNoun = "ipn";
		public const string NounAdjunct = "nad";
		public const string Adjective = "adj";
		public const string Preposition = "pre";
		public const string Import = "import";
	}

	public enum WordType
	{
		Article,
		Indicative,
		Adposition,
		Adjective,
		Noun
	}

	public enum GeneralArticleProperties 
	{
		HasDefiniteSingularArticles = 0x001,
		HasDefinitePluralArticles = 0x002,
		HasIndefiniteSingularArticles = 0x004,
		HasIndefinitePluralArticles = 0x008
	}

//	public enum GeneralGenderProperties 
//	{
//		None = 0x00,
//		FemenineIsDerivedFromMasculine = 0x01,
//		NeutralIsDerivedFromMasculine = 0x02,
//		NeutralIsDerivedFromFemenine = 0x04
//	}

	public enum GeneralIndicativeProperties 
	{
		HasMasculineIndicative = 0x001,
		HasFemenineIndicative = 0x002,
		HasNeutralIndicative = 0x004,
		HasSingularIndicative = 0x008,
		HasPluralIndicative = 0x010,
		HasDefiniteIndicative = 0x020,
		HasIndefiniteIndicative = 0x040,

		HasDefiniteSingularMasculineIndicative = 0x029,
		HasDefiniteSingularFemenineIndicative = 0x02a,
		HasDefiniteSingularNeutralIndicative = 0x02c,
		HasDefinitePluralMasculineIndicative = 0x031,
		HasDefinitePluralFemenineIndicative = 0x032,
		HasDefinitePluralNeutralIndicative = 0x034,
		HasIndefiniteSingularMasculineIndicative = 0x049,
		HasIndefiniteSingularFemenineIndicative = 0x04a,
		HasIndefiniteSingularNeutralIndicative = 0x04c,
		HasIndefinitePluralMasculineIndicative = 0x051,
		HasIndefinitePluralFemenineIndicative = 0x052,
		HasIndefinitePluralNeutralIndicative = 0x054
	}

	public enum AdjunctionProperties
	{
		None = 0x00,
		IsAffixed = 0x01,
		GoesAfterNoun = 0x02,
		IsLinkedWithDash = 0x04,

		IsSuffixed = 0x03,
		GoesAfterNounAndLinkedWithDash = 0x06
	}

	public enum WordProperties
	{
		None = 0x00,
		Plural = 0x01,
		Indefinite = 0x02,
		Femenine = 0x04,
		Neutral = 0x08,
		Irregular = 0x10,

		IsNotMasculine = 0x0c
	}

	public enum PhraseProperties
	{
		None = 0x00,
		Plural = 0x01,
		Indefinite = 0x02,
		Femenine = 0x04,
		Neutral = 0x08
	}

	public static class IndicativeType
	{
		public const string Definite = "Definite";
		public const string Indefinite = "Indefinite";
		public const string Singular = "Singular";
		public const string Plural = "Plural";
		public const string Masculine = "Masculine";
		public const string Femenine = "Femenine";
		public const string Neutral = "Neutral";
		public const string DefiniteSingularMasculine = "DefiniteSingularMasculine";
		public const string DefiniteSingularFemenine = "DefiniteSingularFemenine";
		public const string DefiniteSingularNeutral = "DefiniteSingularNeutral";
		public const string DefinitePluralMasculine = "DefinitePluralMasculine";
		public const string DefinitePluralFemenine = "DefinitePluralFemenine";
		public const string DefinitePluralNeutral = "DefinitePluralNeutral";
		public const string IndefiniteSingularMasculine = "IndefiniteSingularMasculine";
		public const string IndefiniteSingularFemenine = "IndefiniteSingularFemenine";
		public const string IndefiniteSingularNeutral = "IndefiniteSingularNeutral";
		public const string IndefinitePluralMasculine = "IndefinitePluralMasculine";
		public const string IndefinitePluralFemenine = "IndefinitePluralFemenine";
		public const string IndefinitePluralNeutral = "IndefinitePluralNeutral";
	}

	public static char[] OnsetLetters = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };

	public static char[] NucleusLetters = new char[] { 'a', 'e', 'i', 'o', 'u' };

	public static char[] CodaLetters = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z', '\'' };

	public static Regex WordPartRegex = new Regex (@"\[(?<attr>\w+)(?:\((?<param>\w+)\))?\](?:\[w+\])*(?<word>[\w\'\-]*)");
	public static Regex ArticleRegex = new Regex (@"^((?<def>the)|(?<indef>(a|an)))$");
	public static Regex PluralIndicativeRegex = new Regex (@"^(es|s)$");

	private static Regex NucleousStartRegex = new Regex (@"^[aeiou]w*"); 

//	public GeneralGenderProperties GenderProperties;
	public GeneralArticleProperties ArticleProperties;
	public GeneralIndicativeProperties IndicativeProperties;

//	public GeneralTypeProperties ArticleTypeProperties;
//	public GeneralTypeProperties AdpositionTypeProperties;

	public AdjunctionProperties ArticleAdjunctionProperties;
	public AdjunctionProperties IndicativeAdjunctionProperties;
	public AdjunctionProperties AdpositionAdjunctionProperties;
	public AdjunctionProperties AdjectiveAdjunctionProperties;
	public AdjunctionProperties NounAdjunctionProperties;

	public string[] ArticleStartSyllables;
	public string[] ArticleNextSyllables;

	public string[] IndicativeStartSyllables;
	public string[] IndicativeNextSyllables;

	public string[] AdpositionStartSyllables;
	public string[] AdpositionNextSyllables;

	public string[] AdjectiveStartSyllables;
	public string[] AdjectiveNextSyllables;

	public string[] NounStartSyllables;
	public string[] NounNextSyllables;

	public List<Word> Articles;
	public List<Word> Indicatives;
	public List<Word> Adpositions = new List<Word> ();
	public List<Word> Adjectives = new List<Word> ();
	public List<Word> Nouns = new List<Word> ();

	private Dictionary<string, Word> _articles;
	private Dictionary<string, Word> _indicatives;
	private Dictionary<string, Word> _adpositions = new Dictionary<string, Word> ();
	private Dictionary<string, Word> _adjectives = new Dictionary<string, Word> ();
	private Dictionary<string, Word> _nouns = new Dictionary<string, Word> ();

	public Language () {
		
	}

	public Language (long id) {
	
		Id = id;
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

		if (nucleus == string.Empty) {
		
			return coda;
		}

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

			int syllableIndex = (int)Mathf.Floor (syllables.Length * getRandomFloat ());

			if (syllableIndex == syllables.Length) {
				throw new System.Exception ("syllable index out of bounds");
			}

			word += syllables[syllableIndex];

			addSyllableChance *= addSyllableChanceDecay * addSyllableChanceDecay;
		}

		return word;
	}

	public static string GenerateDerivatedWord (
		string rootWord, 
		float noChangeChance, 
		float replaceChance, 
		string[] startSyllables, 
		string[] nextSyllables, 
		float addSyllableChanceDecay, 
		GetRandomFloatDelegate getRandomFloat) {

		float randomFloat = getRandomFloat ();

		if (randomFloat < noChangeChance)
			return rootWord;

		if (randomFloat >= (1f - replaceChance)) {
		
			return GenerateSimpleWord (startSyllables, nextSyllables, addSyllableChanceDecay, getRandomFloat);
		}

		if (getRandomFloat () < 0.5f) {
		
			return GenerateSimpleWord (startSyllables, nextSyllables, addSyllableChanceDecay, getRandomFloat) + rootWord;
		}

		return rootWord + GenerateSimpleWord (nextSyllables, nextSyllables, addSyllableChanceDecay, getRandomFloat);
	}

	public static AdjunctionProperties GenerateAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		AdjunctionProperties properties = AdjunctionProperties.None;

		if (getRandomFloat () < 0.5f) {

			properties |= AdjunctionProperties.GoesAfterNoun;
		}

		float random = getRandomFloat ();

		if (random < 0.33f) {

			properties |= AdjunctionProperties.IsAffixed;

		} else if (random < 0.66f) {

			properties |= AdjunctionProperties.IsLinkedWithDash;
		}

		return properties;
	}

	public static string NounAdjunctionPropertiesToString (AdjunctionProperties properties) {

		if (properties == AdjunctionProperties.None)
			return "None";

		string output = "";

		bool multipleProperties = false;

		if ((properties & AdjunctionProperties.IsAffixed) == AdjunctionProperties.IsAffixed) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsAffixed";
			multipleProperties = true;
		}

		if ((properties & AdjunctionProperties.GoesAfterNoun) == AdjunctionProperties.GoesAfterNoun) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "GoesAfterNoun";
			multipleProperties = true;
		}

		if ((properties & AdjunctionProperties.IsLinkedWithDash) == AdjunctionProperties.IsLinkedWithDash) {

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
		word.Value = GenerateSimpleWord (startSyllables, nextSyllables, 0.0f, getRandomFloat);
		word.Properties = properties;
		word.Type = WordType.Article;

		return word;
	}

	public static Word GenerateDerivatedArticle (
		Word rootArticle, 
		string[] startSyllables, 
		string[] nextSyllables, 
		WordProperties properties, 
		GetRandomFloatDelegate getRandomFloat) {

		Word word = new Word ();
		word.Value = GenerateDerivatedWord (rootArticle.Value, 0.4f, 0.5f, startSyllables, nextSyllables, 0.0f, getRandomFloat);
		word.Properties = rootArticle.Properties | properties;
		word.Type = WordType.Article;

		return word;
	}

	public static void GenerateGenderedArticles (
		Word root,
		string[] startSyllables, 
		string[] nextSyllables,
		GetRandomFloatDelegate getRandomFloat, 
		out Word masculine, 
		out Word femenine,
		out Word neutral) {

		Word firstVariant = GenerateDerivatedArticle (root, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);

		Word secondVariant;
		if (getRandomFloat () < 0.5f) {
			secondVariant = GenerateDerivatedArticle (root, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);
		} else {
			secondVariant = GenerateDerivatedArticle (firstVariant, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);
		}

		float randomFloat = getRandomFloat ();

		if (randomFloat < 0.33f) {
			masculine = root;

			if (getRandomFloat () < 0.5f) {
				femenine = firstVariant;
				neutral = secondVariant;
			} else {
				femenine = secondVariant;
				neutral = firstVariant;
			}

		} else if (randomFloat < 0.66f) {
			masculine = firstVariant;

			if (getRandomFloat () < 0.5f) {
				femenine = root;
				neutral = secondVariant;
			} else {
				femenine = secondVariant;
				neutral = root;
			}

		}else {
			masculine = secondVariant;

			if (getRandomFloat () < 0.5f) {
				femenine = firstVariant;
				neutral = root;
			} else {
				femenine = root;
				neutral = firstVariant;
			}
		}

		femenine.Properties |= WordProperties.Femenine;
		neutral.Properties |= WordProperties.Neutral;
	}

	public static Dictionary<string, Word> GenerateArticles (
		string[] startSyllables, 
		string[] nextSyllables, 
		GeneralArticleProperties generalProperties, 
		GetRandomFloatDelegate getRandomFloat) {

		Dictionary<string, Word> articles = new Dictionary<string, Word> ();

		Word root = GenerateArticle (startSyllables, nextSyllables, WordProperties.None, getRandomFloat);

		Word definite = GenerateDerivatedArticle (root, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);
		Word indefinite = GenerateDerivatedArticle (root, startSyllables, nextSyllables, WordProperties.Indefinite, getRandomFloat);

		if ((generalProperties & GeneralArticleProperties.HasDefiniteSingularArticles) == GeneralArticleProperties.HasDefiniteSingularArticles) {

			Word definiteSingular = GenerateDerivatedArticle (definite, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);

			Word femenine, masculine, neutral;
			GenerateGenderedArticles (definiteSingular, startSyllables, nextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.DefiniteSingularFemenine;
			masculine.Meaning = IndicativeType.DefiniteSingularMasculine;
			neutral.Meaning = IndicativeType.DefiniteSingularNeutral;

			articles.Add (IndicativeType.DefiniteSingularFemenine, femenine);
			articles.Add (IndicativeType.DefiniteSingularMasculine, masculine);
			articles.Add (IndicativeType.DefiniteSingularNeutral, neutral);
		}

		if ((generalProperties & GeneralArticleProperties.HasDefinitePluralArticles) == GeneralArticleProperties.HasDefinitePluralArticles) {

			Word definitePlural = GenerateDerivatedArticle (definite, startSyllables, nextSyllables, WordProperties.Plural, getRandomFloat);

			Word femenine, masculine, neutral;
			GenerateGenderedArticles (definitePlural, startSyllables, nextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.DefinitePluralFemenine;
			masculine.Meaning = IndicativeType.DefinitePluralMasculine;
			neutral.Meaning = IndicativeType.DefinitePluralNeutral;

			articles.Add (IndicativeType.DefinitePluralFemenine, femenine);
			articles.Add (IndicativeType.DefinitePluralMasculine, masculine);
			articles.Add (IndicativeType.DefinitePluralNeutral, neutral);
		}

		if ((generalProperties & GeneralArticleProperties.HasIndefiniteSingularArticles) == GeneralArticleProperties.HasIndefiniteSingularArticles) {

			Word indefiniteSingular = GenerateDerivatedArticle (indefinite, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);

			Word femenine, masculine, neutral;
			GenerateGenderedArticles (indefiniteSingular, startSyllables, nextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.IndefiniteSingularFemenine;
			masculine.Meaning = IndicativeType.IndefiniteSingularMasculine;
			neutral.Meaning = IndicativeType.IndefiniteSingularNeutral;

			articles.Add (IndicativeType.IndefiniteSingularFemenine, femenine);
			articles.Add (IndicativeType.IndefiniteSingularMasculine, masculine);
			articles.Add (IndicativeType.IndefiniteSingularNeutral, neutral);
		}

		if ((generalProperties & GeneralArticleProperties.HasIndefinitePluralArticles) == GeneralArticleProperties.HasIndefinitePluralArticles) {

			Word indefinitePlural = GenerateDerivatedArticle (indefinite, startSyllables, nextSyllables, WordProperties.Plural, getRandomFloat);

			Word femenine, masculine, neutral;
			GenerateGenderedArticles (indefinitePlural, startSyllables, nextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.IndefinitePluralFemenine;
			masculine.Meaning = IndicativeType.IndefinitePluralMasculine;
			neutral.Meaning = IndicativeType.IndefinitePluralNeutral;

			articles.Add (IndicativeType.IndefinitePluralFemenine, femenine);
			articles.Add (IndicativeType.IndefinitePluralMasculine, masculine);
			articles.Add (IndicativeType.IndefinitePluralNeutral, neutral);
		}

		return articles;
	}

	public static WordProperties GenerateWordProperties (
		GetRandomFloatDelegate getRandomFloat, 
		bool isPlural, 
		bool randomGender = false, 
		bool isFemenine = false, 
		bool isNeutral = false, 
		bool canBeIrregular = false) {

		WordProperties properties = WordProperties.None;

		if (isPlural) {
			properties |= WordProperties.Plural;
		}

		if (randomGender) {

			float genderChance = getRandomFloat ();

			if (genderChance >= 0.66f) {
				isNeutral = true;
			} else if (genderChance >= 0.33f) {
				isFemenine = true;
			}
		}

		if (isFemenine) {
			properties |= WordProperties.Femenine;
		}

		if (isNeutral) {
			properties |= WordProperties.Neutral;
		}

		float irregularChance = getRandomFloat ();

		if ((canBeIrregular) && (irregularChance < 0.05f)) {
			properties |= WordProperties.Irregular;
		}

		return properties;
	}

	public static string WordPropertiesToString (WordProperties properties) {

		if (properties == WordProperties.None)
			return "None";

		string output = "";

		bool multipleProperties = false;

		if ((properties & WordProperties.Femenine) == WordProperties.Femenine) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsFemenine";
			multipleProperties = true;
		}

		if ((properties & WordProperties.Neutral) == WordProperties.Neutral) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsNeutral";
			multipleProperties = true;
		}

		if ((properties & WordProperties.Plural) == WordProperties.Plural) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsPlural";
			multipleProperties = true;
		}

		if ((properties & WordProperties.Irregular) == WordProperties.Irregular) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsIrregular";
			multipleProperties = true;
		}

		return output;
	}

	public void GenerateArticleProperties (GetRandomFloatDelegate getRandomFloat) {

		if (getRandomFloat () < 0.75f) {

			ArticleProperties |= GeneralArticleProperties.HasDefiniteSingularArticles;
		}

		if (getRandomFloat () < 0.75f) {

			ArticleProperties |= GeneralArticleProperties.HasDefinitePluralArticles;
		}

		if (getRandomFloat () < 0.75f) {

			ArticleProperties |= GeneralArticleProperties.HasIndefiniteSingularArticles;
		}

		if (getRandomFloat () < 0.75f) {

			ArticleProperties |= GeneralArticleProperties.HasIndefinitePluralArticles;
		}
	}

	public void GenerateIndicativeProperties (GetRandomFloatDelegate getRandomFloat) {

		if (getRandomFloat () < 0.25f) {

			IndicativeProperties |= GeneralIndicativeProperties.HasDefiniteIndicative;
		}

		if (getRandomFloat () < 0.25f) {

			IndicativeProperties |= GeneralIndicativeProperties.HasIndefiniteIndicative;
		}

		if (getRandomFloat () < 0.25f) {

			IndicativeProperties |= GeneralIndicativeProperties.HasMasculineIndicative;
		}

		if (getRandomFloat () < 0.25f) {

			IndicativeProperties |= GeneralIndicativeProperties.HasNeutralIndicative;
		}

		if (getRandomFloat () < 0.25f) {

			IndicativeProperties |= GeneralIndicativeProperties.HasFemenineIndicative;
		}

		if (getRandomFloat () < 0.25f) {

			IndicativeProperties |= GeneralIndicativeProperties.HasSingularIndicative;
		}

		if (getRandomFloat () < 0.25f) {

			IndicativeProperties |= GeneralIndicativeProperties.HasPluralIndicative;
		}
	}

	public void GenerateArticleAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		ArticleAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat);
	}

	public void GenerateArticleSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.1f, getRandomFloat, 10);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 5);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.25f, 0.1f, getRandomFloat, 4);

		ArticleStartSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
		ArticleNextSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
	}

	public void GenerateAllArticles (GetRandomFloatDelegate getRandomFloat) {

		_articles = GenerateArticles (ArticleStartSyllables, ArticleNextSyllables, ArticleProperties, getRandomFloat);

		Articles = new List<Word> (_articles.Count);

		foreach (KeyValuePair<string, Word> pair in _articles) {
		
			Articles.Add (pair.Value);
		}
	}

	public void GenerateIndicativeAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		IndicativeAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat);
	}

	public void GenerateIndicativeSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.1f, getRandomFloat, 10);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 5);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.25f, 0.1f, getRandomFloat, 4);

		IndicativeStartSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
		IndicativeNextSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
	}

	public static Word GenerateIndicative (string[] startSyllables, string[] nextSyllables, WordProperties properties, GetRandomFloatDelegate getRandomFloat) {

		Word word = new Word ();
		word.Value = GenerateSimpleWord (startSyllables, nextSyllables, 0.0f, getRandomFloat);
		word.Properties = properties;
		word.Type = WordType.Indicative;

		return word;
	}

	public static Word GenerateNullWord (WordType type, WordProperties properties = WordProperties.None) {

		Word word = new Word ();
		word.Value = string.Empty;
		word.Properties = properties;
		word.Type = type;
		word.Meaning = string.Empty;

		return word;
	}

	public static Word CopyWord (Word sourceWord) {

		Word word = new Word ();
		word.Value = sourceWord.Value;
		word.Properties = sourceWord.Properties;
		word.Type = sourceWord.Type;
		word.Meaning = sourceWord.Meaning;

		return word;
	}

	public static Word GenerateDerivatedIndicative (
		Word rootIndicative, 
		string[] startSyllables, 
		string[] nextSyllables, 
		WordProperties properties, 
		GetRandomFloatDelegate getRandomFloat) {

		Word word = new Word ();
		word.Value = GenerateDerivatedWord (rootIndicative.Value, 0.0f, 0.5f, startSyllables, nextSyllables, 0.0f, getRandomFloat);
		word.Properties = rootIndicative.Properties | properties;
		word.Type = WordType.Indicative;

		return word;
	}

	public static void GenerateGenderedIndicatives (
		Word root,
		string[] startSyllables, 
		string[] nextSyllables,
		GeneralIndicativeProperties indicativeProperties, 
		GetRandomFloatDelegate getRandomFloat, 
		out Word masculine, 
		out Word femenine,
		out Word neutral) {

		if ((indicativeProperties & GeneralIndicativeProperties.HasMasculineIndicative) == GeneralIndicativeProperties.HasMasculineIndicative)
			masculine = GenerateDerivatedIndicative (root, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);
		else
			masculine = CopyWord (root);

		if ((indicativeProperties & GeneralIndicativeProperties.HasFemenineIndicative) == GeneralIndicativeProperties.HasFemenineIndicative)
			femenine = GenerateDerivatedIndicative (root, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);
		else
			femenine = CopyWord (root);

		if ((indicativeProperties & GeneralIndicativeProperties.HasNeutralIndicative) == GeneralIndicativeProperties.HasNeutralIndicative)
			neutral = GenerateDerivatedIndicative (root, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);
		else
			neutral = CopyWord (root);

		femenine.Properties |= WordProperties.Femenine;
		neutral.Properties |= WordProperties.Neutral;
	}

	public static Dictionary<string, Word> GenerateIndicatives (
		string[] startSyllables, 
		string[] nextSyllables, 
		GeneralIndicativeProperties indicativeProperties, 
		GetRandomFloatDelegate getRandomFloat) {

		Dictionary<string, Word> indicatives = new Dictionary<string, Word> ();

		Word definite;
		if ((indicativeProperties & GeneralIndicativeProperties.HasDefiniteIndicative) == GeneralIndicativeProperties.HasDefiniteIndicative)
			definite = GenerateIndicative (startSyllables, nextSyllables, WordProperties.None, getRandomFloat);
		else
			definite = GenerateNullWord (WordType.Indicative);

		Word indefinite;
		if ((indicativeProperties & GeneralIndicativeProperties.HasIndefiniteIndicative) == GeneralIndicativeProperties.HasIndefiniteIndicative)
			indefinite = GenerateIndicative (startSyllables, nextSyllables, WordProperties.Indefinite, getRandomFloat);
		else
			indefinite = GenerateNullWord (WordType.Indicative, WordProperties.Indefinite);

		///

		Word definiteSingular;
		if ((indicativeProperties & GeneralIndicativeProperties.HasSingularIndicative) == GeneralIndicativeProperties.HasSingularIndicative)
			definiteSingular = GenerateDerivatedIndicative (definite, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);
		else
			definiteSingular = CopyWord (definite);

		Word femenine, masculine, neutral;
		GenerateGenderedIndicatives (definiteSingular, startSyllables, nextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.DefiniteSingularFemenine;
		masculine.Meaning = IndicativeType.DefiniteSingularMasculine;
		neutral.Meaning = IndicativeType.DefiniteSingularNeutral;

		indicatives.Add (IndicativeType.DefiniteSingularFemenine, femenine);
		indicatives.Add (IndicativeType.DefiniteSingularMasculine, masculine);
		indicatives.Add (IndicativeType.DefiniteSingularNeutral, neutral);

		///

		Word definitePlural;
		if ((indicativeProperties & GeneralIndicativeProperties.HasPluralIndicative) == GeneralIndicativeProperties.HasPluralIndicative)
			definitePlural = GenerateDerivatedIndicative (definite, startSyllables, nextSyllables, WordProperties.Plural, getRandomFloat);
		else {
			definitePlural = CopyWord (definite);
			definitePlural.Properties |= WordProperties.Plural;
		}

		GenerateGenderedIndicatives (definitePlural, startSyllables, nextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.DefinitePluralFemenine;
		masculine.Meaning = IndicativeType.DefinitePluralMasculine;
		neutral.Meaning = IndicativeType.DefinitePluralNeutral;

		indicatives.Add (IndicativeType.DefinitePluralFemenine, femenine);
		indicatives.Add (IndicativeType.DefinitePluralMasculine, masculine);
		indicatives.Add (IndicativeType.DefinitePluralNeutral, neutral);

		///

		Word indefiniteSingular;
		if ((indicativeProperties & GeneralIndicativeProperties.HasSingularIndicative) == GeneralIndicativeProperties.HasSingularIndicative)
			indefiniteSingular = GenerateDerivatedIndicative (indefinite, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);
		else
			indefiniteSingular = CopyWord (indefinite);

		GenerateGenderedIndicatives (indefiniteSingular, startSyllables, nextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.IndefiniteSingularFemenine;
		masculine.Meaning = IndicativeType.IndefiniteSingularMasculine;
		neutral.Meaning = IndicativeType.IndefiniteSingularNeutral;

		indicatives.Add (IndicativeType.IndefiniteSingularFemenine, femenine);
		indicatives.Add (IndicativeType.IndefiniteSingularMasculine, masculine);
		indicatives.Add (IndicativeType.IndefiniteSingularNeutral, neutral);

		///

		Word indefinitePlural;
		if ((indicativeProperties & GeneralIndicativeProperties.HasPluralIndicative) == GeneralIndicativeProperties.HasPluralIndicative)
			indefinitePlural = GenerateDerivatedIndicative (indefinite, startSyllables, nextSyllables, WordProperties.Plural, getRandomFloat);
		else {
			indefinitePlural = CopyWord (indefinite);
			indefinitePlural.Properties |= WordProperties.Plural;
		}

		GenerateGenderedIndicatives (indefinitePlural, startSyllables, nextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.IndefinitePluralFemenine;
		masculine.Meaning = IndicativeType.IndefinitePluralMasculine;
		neutral.Meaning = IndicativeType.IndefinitePluralNeutral;

		indicatives.Add (IndicativeType.IndefinitePluralFemenine, femenine);
		indicatives.Add (IndicativeType.IndefinitePluralMasculine, masculine);
		indicatives.Add (IndicativeType.IndefinitePluralNeutral, neutral);

		return indicatives;
	}

	public void GenerateAllIndicatives (GetRandomFloatDelegate getRandomFloat) {

		_indicatives = GenerateIndicatives (IndicativeStartSyllables, IndicativeNextSyllables, IndicativeProperties, getRandomFloat);

		Indicatives = new List<Word> (_indicatives.Count);

		foreach (KeyValuePair<string, Word> pair in _indicatives) {

			Indicatives.Add (pair.Value);
		}
	}

	public void GenerateAdpositionAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		AdpositionAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat);
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
		word.Value = GenerateSimpleWord (AdpositionStartSyllables, AdpositionNextSyllables, 0.5f, getRandomFloat);
		word.Properties = WordProperties.None;
		word.Type = WordType.Adposition;
		word.Meaning = relation;

		_adpositions.Add (relation, word);

		Adpositions.Add (word);
	}

	public void GenerateAdjectiveAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		AdjectiveAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat);
	}

	public void GenerateAdjectiveSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.1f, getRandomFloat, 20);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 10);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.25f, 0.1f, getRandomFloat, 8);

		AdjectiveStartSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 100);
		AdjectiveNextSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 100);
	}

	public Word GenerateAdjective (string meaning, GetRandomFloatDelegate getRandomFloat) {

		Word word = new Word ();
		word.Value = GenerateSimpleWord (AdjectiveStartSyllables, AdjectiveNextSyllables, 0.5f, getRandomFloat);
		word.Properties = WordProperties.None;
		word.Type = WordType.Adjective;
		word.Meaning = meaning;

		_adjectives.Add (meaning, word);

		Adjectives.Add (word);

		return word;
	}

	public void GenerateNounAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		NounAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat);
	}

	public void GenerateNounSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.25f, getRandomFloat, 20);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 10);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.5f, 0.25f, getRandomFloat, 8);

		NounStartSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 200);
		NounNextSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 200);
	}

	public Word GenerateNoun (string meaning, GetRandomFloatDelegate getRandomFloat, bool isPlural, bool randomGender, bool isFemenine = false, bool isNeutral = false, bool canBeIrregular = true) {

		return GenerateNoun (meaning, GenerateWordProperties (getRandomFloat, isPlural, randomGender, isFemenine, isNeutral, canBeIrregular), getRandomFloat);
	}

	public Word GenerateNoun (string meaning, WordProperties properties, GetRandomFloatDelegate getRandomFloat) {

		if (_nouns.ContainsKey (meaning)) {
		
			return _nouns [meaning];
		}

		Word word = new Word ();
		word.Value = GenerateSimpleWord (NounStartSyllables, NounNextSyllables, 0.5f, getRandomFloat);
		word.Properties = properties;
		word.Type = WordType.Noun;
		word.Meaning = meaning;

		_nouns.Add (meaning, word);

		Nouns.Add (word);

		return word;
	}

	public Word GetAppropiateArticle (PhraseProperties phraseProperties) {

		Word article = null;

		if ((phraseProperties & PhraseProperties.Indefinite) == PhraseProperties.Indefinite) {
			if ((phraseProperties & PhraseProperties.Plural) == PhraseProperties.Plural) {

				if ((ArticleProperties & GeneralArticleProperties.HasIndefinitePluralArticles) == GeneralArticleProperties.HasIndefinitePluralArticles) {
					if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

						article = _articles [IndicativeType.IndefinitePluralFemenine];

					} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

						article = _articles [IndicativeType.IndefinitePluralNeutral];

					} else {
						article = _articles [IndicativeType.IndefinitePluralMasculine];
					}
				}
			} else {
				if ((ArticleProperties & GeneralArticleProperties.HasIndefiniteSingularArticles) == GeneralArticleProperties.HasIndefiniteSingularArticles) {
					if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

						article = _articles [IndicativeType.IndefiniteSingularFemenine];

					} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

						article = _articles [IndicativeType.IndefiniteSingularNeutral];

					} else {
						article = _articles [IndicativeType.IndefiniteSingularMasculine];
					}
				}
			}
		} else {
			if ((phraseProperties & PhraseProperties.Plural) == PhraseProperties.Plural) {

				if ((ArticleProperties & GeneralArticleProperties.HasDefinitePluralArticles) == GeneralArticleProperties.HasDefinitePluralArticles) {
					if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

						article = _articles [IndicativeType.DefinitePluralFemenine];

					} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

						article = _articles [IndicativeType.DefinitePluralNeutral];

					} else {
						article = _articles [IndicativeType.DefinitePluralMasculine];
					}
				}
			} else {
				if ((ArticleProperties & GeneralArticleProperties.HasDefiniteSingularArticles) == GeneralArticleProperties.HasDefiniteSingularArticles) {
					if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

						article = _articles [IndicativeType.DefiniteSingularFemenine];

					} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

						article = _articles [IndicativeType.DefiniteSingularNeutral];

					} else {
						article = _articles [IndicativeType.DefiniteSingularMasculine];
					}
				}
			}
		}

		return article;
	}

	public Phrase BuildAdpositionalPhrase (string relation, Phrase complementPhrase) {

		Phrase phrase = new Phrase ();

		Word adposition = null;

		if (!_adpositions.TryGetValue (relation, out adposition)) {

			return phrase;
		}

		string meaning = relation + " " + complementPhrase.Meaning;

		string text = complementPhrase.Text;

		if ((AdpositionAdjunctionProperties & AdjunctionProperties.GoesAfterNoun) == AdjunctionProperties.GoesAfterNoun) {

			if ((AdpositionAdjunctionProperties & AdjunctionProperties.IsAffixed) == AdjunctionProperties.IsAffixed) {

				if ((AdpositionAdjunctionProperties & AdjunctionProperties.IsLinkedWithDash) == AdjunctionProperties.IsLinkedWithDash) {

					text += "-" + adposition.Value;
				} else {

					text += adposition.Value;
				}
			} else {

				text += " " + adposition.Value;
			}
		} else {
			if ((AdpositionAdjunctionProperties & AdjunctionProperties.IsAffixed) == AdjunctionProperties.IsAffixed) {

				if ((AdpositionAdjunctionProperties & AdjunctionProperties.IsLinkedWithDash) == AdjunctionProperties.IsLinkedWithDash) {

					text = adposition.Value + "-" + text;
				} else {

					text = adposition.Value + text;
				}
			} else {

				text = adposition.Value + " " + text;
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

	public Word GetAppropiateIndicative (PhraseProperties phraseProperties) {

		Word indicative = null;

		if ((phraseProperties & PhraseProperties.Indefinite) == PhraseProperties.Indefinite) {
			if ((phraseProperties & PhraseProperties.Plural) == PhraseProperties.Plural) {

				if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

					indicative = _indicatives [IndicativeType.IndefinitePluralFemenine];

				} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

					indicative = _indicatives [IndicativeType.IndefinitePluralNeutral];

				} else {

					indicative = _indicatives [IndicativeType.IndefinitePluralMasculine];

				}
			} else {

				if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

					indicative = _indicatives [IndicativeType.IndefiniteSingularFemenine];

				} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

					indicative = _indicatives [IndicativeType.IndefiniteSingularNeutral];

				} else {

					indicative = _indicatives [IndicativeType.IndefiniteSingularMasculine];
				}
			}
		} else {
			if ((phraseProperties & PhraseProperties.Plural) == PhraseProperties.Plural) {

				if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

					indicative = _indicatives [IndicativeType.DefinitePluralFemenine];

				} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

					indicative = _indicatives [IndicativeType.DefinitePluralNeutral];

				} else {

					indicative = _indicatives [IndicativeType.DefinitePluralMasculine];

				}
			} else {

				if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

					indicative = _indicatives [IndicativeType.DefiniteSingularFemenine];

				} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

					indicative = _indicatives [IndicativeType.DefiniteSingularNeutral];

				} else {

					indicative = _indicatives [IndicativeType.DefiniteSingularMasculine];
				}
			}
		}

		return indicative;
	}

	public static string AddAdjunctionToNounPhrase (string phrase, string adjunction, AdjunctionProperties properties, bool forceAffixed = false) {
		
		if (string.IsNullOrEmpty (adjunction))
			return phrase;
			
		if ((properties & AdjunctionProperties.GoesAfterNoun) == AdjunctionProperties.GoesAfterNoun) {

			if (forceAffixed || ((properties & AdjunctionProperties.IsAffixed) == AdjunctionProperties.IsAffixed)) {

				phrase += adjunction;

			} else if ((properties & AdjunctionProperties.IsLinkedWithDash) == AdjunctionProperties.IsLinkedWithDash) {

				phrase += "-" + adjunction;

			} else {

				phrase += " " + adjunction;
			}
		} else {
			if (forceAffixed || ((properties & AdjunctionProperties.IsAffixed) == AdjunctionProperties.IsAffixed)) {

				phrase = adjunction + phrase;

			} else if ((properties & AdjunctionProperties.IsLinkedWithDash) == AdjunctionProperties.IsLinkedWithDash) {

				phrase = adjunction + "-" + phrase;

			} else {

				phrase = adjunction + " " + phrase;
			}
		}

		return phrase;
	}

	public NounPhrase TranslateNounPhrase (string untranslatedNounPhrase, GetRandomFloatDelegate getRandomFloat) {

		bool absentArticle = true;
		PhraseProperties phraseProperties = PhraseProperties.None;

		NounPhrase nounPhrase = null;

		List<NounPhrase> nounAdjunctionPhrases = new List<NounPhrase> ();
		List<Word> adjectives = new List<Word> ();

		string[] phraseParts = untranslatedNounPhrase.Split (new char[] { ' ' });

		foreach (string phrasePart in phraseParts) {

			Match articleMatch = ArticleRegex.Match (phrasePart);
		
			if (articleMatch.Success) {
				absentArticle = false;

				if (articleMatch.Groups ["indef"].Success) {
					phraseProperties |= PhraseProperties.Indefinite;
				}

				continue;
			}

			if (absentArticle) {
				phraseProperties |= PhraseProperties.Indefinite;
			}

			ParsedWord parsedPhrasePart = ParseWord (phrasePart);

			if (parsedPhrasePart.Attributes.ContainsKey (ParsedWordAttributeId.NounAdjunct)) {
				
				nounAdjunctionPhrases.Add (TranslateNoun (phrasePart, phraseProperties, getRandomFloat));

			} else if (parsedPhrasePart.Attributes.ContainsKey (ParsedWordAttributeId.Adjective)) {
			
				adjectives.Add (GenerateAdjective (parsedPhrasePart.Value, getRandomFloat));

			} else {

				nounPhrase = TranslateNoun (phrasePart, phraseProperties, getRandomFloat);
				phraseProperties = nounPhrase.Properties;
			}
		}

		foreach (NounPhrase nounAdjunctionPhrase in nounAdjunctionPhrases) {

			nounPhrase.Text = AddAdjunctionToNounPhrase (nounPhrase.Text, nounAdjunctionPhrase.Text, NounAdjunctionProperties);
		}

		foreach (Word adjective in adjectives) {

			nounPhrase.Text = AddAdjunctionToNounPhrase (nounPhrase.Text, adjective.Value, AdjectiveAdjunctionProperties);
		}

		Word article = GetAppropiateArticle (phraseProperties);

		if (article != null) {
			nounPhrase.Text = AddAdjunctionToNounPhrase (nounPhrase.Text, article.Value, ArticleAdjunctionProperties);
		}

		nounPhrase.Original = untranslatedNounPhrase;
		nounPhrase.Meaning = ClearConstructCharacters (untranslatedNounPhrase);

		return nounPhrase;
	}

	public NounPhrase TranslateNoun (string untranslatedNoun, PhraseProperties properties, GetRandomFloatDelegate getRandomFloat) {

		string[] nounParts = untranslatedNoun.Split (new char[] { ':' });

		Word mainNounWord = null;

		List<Word> nounComponents = new List<Word> ();

		bool isPlural = false;
		bool hasRandomGender = true;
		bool isFemenineNoun = false;
		bool isNeutralNoun = false;

		foreach (string nounPart in nounParts) {

			Match plIndicativeMatch = PluralIndicativeRegex.Match (nounPart);

			if (plIndicativeMatch.Success) {
				isPlural = true;

				continue;
			}

			ParsedWord parsedWordPart = ParseWord (nounPart);

			if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.NounPluralIndicative)) {

				isPlural = true;

			} else {

				hasRandomGender = true;
				isFemenineNoun = false;
				isNeutralNoun = false;

				if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.FemenineNoun)) {
					hasRandomGender = false;
					isFemenineNoun = true;
				} else if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.MasculineNoun)) {
					hasRandomGender = false;
				} else if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.NeutralNoun)) {
					hasRandomGender = false;
					isNeutralNoun = true;
				}

				Word nounWord = null;

				string singularNoun = parsedWordPart.Value;
				if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.IrregularPluralNoun)) {
					singularNoun = parsedWordPart.Attributes[ParsedWordAttributeId.IrregularPluralNoun];
					isPlural = true;
				}
				
				nounWord = GenerateNoun (singularNoun, getRandomFloat, false, hasRandomGender, isFemenineNoun, isNeutralNoun);

				isFemenineNoun = ((nounWord.Properties & WordProperties.Femenine) == WordProperties.Femenine);
				isNeutralNoun = ((nounWord.Properties & WordProperties.Neutral) == WordProperties.Neutral);
				hasRandomGender = false;

				if (mainNounWord != null) {
					nounComponents.Add (mainNounWord);
				}

				mainNounWord = nounWord;
			}
		}

		if (isPlural)
			properties |= PhraseProperties.Plural;

		if (isFemenineNoun)
			properties |= PhraseProperties.Femenine;
		else if (isNeutralNoun)
			properties |= PhraseProperties.Neutral;

		string text = mainNounWord.Value;

		foreach (Word nounComponent in nounComponents) {
			text = AddAdjunctionToNounPhrase (text, nounComponent.Value, NounAdjunctionProperties, true);
		}

		Word indicative = GetAppropiateIndicative (properties);

		text = AddAdjunctionToNounPhrase (text, indicative.Value, IndicativeAdjunctionProperties);

		NounPhrase phrase = new NounPhrase ();
		phrase.Text = text;
		phrase.Original = untranslatedNoun;
		phrase.Meaning = ClearConstructCharacters (untranslatedNoun);
		phrase.Properties = properties;

		return phrase;
	}

	public NounPhrase MakeProperNoun (NounPhrase originalPhrase) {

		NounPhrase phrase = new NounPhrase ();
		phrase.Text = MakeFirstLetterUpper (TurnIntoWord (originalPhrase.Text));
		phrase.Original = originalPhrase.Text;
		phrase.Meaning = originalPhrase.Meaning;
		phrase.Properties = originalPhrase.Properties;

		return phrase;
	}

	public static ParsedWord ParseWord (string word) {

		ParsedWord parsedWord = new ParsedWord ();

		while (true) {
			Match match = WordPartRegex.Match (word);

			if (!match.Success)
				break;

			word = word.Replace (match.Value, match.Groups ["word"].Value);

			parsedWord.Attributes.Add (match.Groups ["attr"].Value, match.Groups ["param"].Success ? match.Groups ["param"].Value : string.Empty);
		}

		parsedWord.Value = word;

		return parsedWord;
	}

	public static string TurnIntoWord (string sentence) {

		sentence = sentence.ToLower ();
		sentence = sentence.Replace (" ", string.Empty);
		sentence = sentence.Replace ("-", string.Empty);

		return sentence;
	}

	public static void MakeProperName (NounPhrase phrase) {

		phrase.Text = MakeProperName (phrase.Text);
		phrase.Meaning = MakeProperName (phrase.Meaning);
	}

	public static string MakeProperName (string sentence) {
		
		string[] words = sentence.Split (new char[] {' '});

		string newSentence = null;

		bool first = true;
		foreach (string word in words) {

			if (first) {

				newSentence = MakeFirstLetterUpper (word);
				first = false;

				continue;
			}

			newSentence += " " + MakeFirstLetterUpper (word);
		}

		return newSentence;
	}

	public static string MakeFirstLetterUpper (string sentence) {

		return sentence.First().ToString().ToUpper() + sentence.Substring(1);
	}

	public static string ClearConstructCharacters (string sentence) {

		while (true) {
			Match match = WordPartRegex.Match (sentence);

			if (!match.Success)
				break;

			sentence = sentence.Replace (match.Value, match.Groups["word"].Value);
		}

		return sentence.Replace (":", string.Empty);
	}

	// For now it will only make the first letter in the phrase uppercase
	public void LocalizePhrase (Phrase phrase) {

		phrase.Text = MakeFirstLetterUpper (phrase.Text);
		phrase.Meaning = MakeFirstLetterUpper (phrase.Meaning);
	}

	public void Synchronize () {
		
	}

	public void FinalizeLoad () {

		_articles = new Dictionary<string, Word> (Articles.Count);
		_indicatives = new Dictionary<string, Word> (Indicatives.Count);

		foreach (Word word in Articles) {
			_articles.Add (word.Meaning, word);
		}

		foreach (Word word in Indicatives) {
			_indicatives.Add (word.Meaning, word);
		}

		foreach (Word word in Adpositions) {
			_adpositions.Add (word.Meaning, word);
		}

		foreach (Word word in Adjectives) {
			_adjectives.Add (word.Meaning, word);
		}

		foreach (Word word in Nouns) {
			_nouns.Add (word.Meaning, word);
		}
	}
}
