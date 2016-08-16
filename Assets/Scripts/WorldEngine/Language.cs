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
		public string Meaning;
		[XmlAttribute]
		public string Text;
	}

	public class WordMeaningPair {

		[XmlAttribute]
		public string Meaning;

		public Word Word;

		public WordMeaningPair () {
			
		}

		public WordMeaningPair (Word word, string meaning) {

			Word = word;
			Meaning = meaning;
		}
	}

	public class Word {

		public WordType Type;
		public WordProperties Properties;

		[XmlAttribute]
		public string Value;
	}

	public class WordPart {

		public string Value;
		public List<string> Attributes = new List<string> ();
	}

	public static class WordPartAttributeId {

		public const string NounPluralizer = "npl";
		public const string FemenineNoun = "fn";
		public const string MasculineNoun = "mn";
		public const string NeutralNoun = "nn";
	}

	public enum WordType
	{
		Article,
		Pluralizer,
		Adposition,
		Noun
	}

	public enum GeneralProperties 
	{
		HasDefiniteSingularArticles = 0x001,
		HasDefinitePluralArticles = 0x002,
		HasIndefiniteSingularArticles = 0x004,
		HasIndefinitePluralArticles = 0x008
	}

	public enum GeneralGenderProperties 
	{
		None = 0x00,
		FemenineIsDerivedFromMasculine = 0x01,
		NeutralIsDerivedFromMasculine = 0x02,
		NeutralIsDerivedFromFemenine = 0x04
	}

	public enum GeneralTypeProperties 
	{
		None = 0x00,
		IsGendered = 0x01,
		HasNeutral = 0x02,
		CanBeIndefinite = 0x04,
		CanBePlural = 0x08
	}

	public enum NounAdjuntProperties
	{
		None = 0x00,
		IsAffixed = 0x01,
		GoesAfterNoun = 0x02,
		IsLinkedWithDash = 0x04
	}

	public enum WordProperties
	{
		None = 0x00,
		Plural = 0x01,
		Indefinite = 0x02,
		Femenine = 0x04,
		Neutral = 0x08,
		Irregular = 0x10
	}

	public static class ArticleType
	{
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

	public static char[] CodaLetters = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };

	public static Regex OptionalWordPartRegex = new Regex (@"(?<break> )?\{(?<word>.+?)\}"); 
	public static Regex WordPartTypeRegex = new Regex (@"\[(?<attr>\w+)\](?:\[w+\])*(?<word>[\w\'\-]*)"); 

	private static Regex NucleousStartRegex = new Regex (@"^[aeiou]w*"); 

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
	public GeneralGenderProperties GenderProperties;

	public GeneralTypeProperties ArticleTypeProperties;
	public GeneralTypeProperties AdpositionTypeProperties;
	public GeneralTypeProperties PluralizerTypeProperties;

	public NounAdjuntProperties ArticleAdjuntProperties;
	public NounAdjuntProperties AdpositionAdjuntProperties;
	public NounAdjuntProperties PluralizerAdjuntProperties;

	public string[] ArticleStartSyllables;
	public string[] ArticleNextSyllables;

	public string[] AdpositionStartSyllables;
	public string[] AdpositionNextSyllables;

	public string[] PluralizerStartSyllables;
	public string[] PluralizerNextSyllables;

	public string[] SimpleNounWordStartSyllables;
	public string[] SimpleNounWordNextSyllables;

	public List<WordMeaningPair> Articles;
	public List<WordMeaningPair> Pluralizers;
	public List<WordMeaningPair> Adpositions = new List<WordMeaningPair> ();
	public List<WordMeaningPair> Nouns = new List<WordMeaningPair> ();

	private Dictionary<string, Word> _articles;
	private Dictionary<string, Word> _pluralizers;
	private Dictionary<string, Word> _adpositions = new Dictionary<string, Word> ();
	private Dictionary<string, Word> _nouns = new Dictionary<string, Word> ();

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

	public static NounAdjuntProperties GenerateNounAdjuntProperties (GetRandomFloatDelegate getRandomFloat) {

		NounAdjuntProperties properties = NounAdjuntProperties.None;

		if (getRandomFloat () < 0.5f) {

			properties |= NounAdjuntProperties.IsAffixed;

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

		if ((properties & NounAdjuntProperties.IsAffixed) == NounAdjuntProperties.IsAffixed) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsAffixed";
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
		GeneralProperties generalProperties, 
		NounAdjuntProperties articleProperties, 
		GetRandomFloatDelegate getRandomFloat) {

		Dictionary<string, Word> articles = new Dictionary<string, Word> ();

		Word root = GenerateArticle (startSyllables, nextSyllables, WordProperties.None, getRandomFloat);

		Word definite = GenerateDerivatedArticle (root, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);
		Word indefinite = GenerateDerivatedArticle (root, startSyllables, nextSyllables, WordProperties.Indefinite, getRandomFloat);

		if ((generalProperties & GeneralProperties.HasDefiniteSingularArticles) == GeneralProperties.HasDefiniteSingularArticles) {

			Word definiteSingular = GenerateDerivatedArticle (definite, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);

			Word femenine, masculine, neutral;
			GenerateGenderedArticles (definiteSingular, startSyllables, nextSyllables, getRandomFloat, out masculine, out femenine, out neutral);

			articles.Add (ArticleType.DefiniteSingularFemenine, femenine);
			articles.Add (ArticleType.DefiniteSingularMasculine, masculine);
			articles.Add (ArticleType.DefiniteSingularNeutral, neutral);
		}

		if ((generalProperties & GeneralProperties.HasDefinitePluralArticles) == GeneralProperties.HasDefinitePluralArticles) {

			Word definitePlural = GenerateDerivatedArticle (definite, startSyllables, nextSyllables, WordProperties.Plural, getRandomFloat);

			Word femenine, masculine, neutral;
			GenerateGenderedArticles (definitePlural, startSyllables, nextSyllables, getRandomFloat, out masculine, out femenine, out neutral);

			articles.Add (ArticleType.DefinitePluralFemenine, femenine);
			articles.Add (ArticleType.DefinitePluralMasculine, masculine);
			articles.Add (ArticleType.DefinitePluralNeutral, neutral);
		}

		if ((generalProperties & GeneralProperties.HasIndefiniteSingularArticles) == GeneralProperties.HasIndefiniteSingularArticles) {

			Word indefiniteSingular = GenerateDerivatedArticle (indefinite, startSyllables, nextSyllables, WordProperties.None, getRandomFloat);

			Word femenine, masculine, neutral;
			GenerateGenderedArticles (indefiniteSingular, startSyllables, nextSyllables, getRandomFloat, out masculine, out femenine, out neutral);

			articles.Add (ArticleType.IndefiniteSingularFemenine, femenine);
			articles.Add (ArticleType.IndefiniteSingularMasculine, masculine);
			articles.Add (ArticleType.IndefiniteSingularNeutral, neutral);
		}

		if ((generalProperties & GeneralProperties.HasIndefinitePluralArticles) == GeneralProperties.HasIndefinitePluralArticles) {

			Word indefinitePlural = GenerateDerivatedArticle (indefinite, startSyllables, nextSyllables, WordProperties.Plural, getRandomFloat);

			Word femenine, masculine, neutral;
			GenerateGenderedArticles (indefinitePlural, startSyllables, nextSyllables, getRandomFloat, out masculine, out femenine, out neutral);

			articles.Add (ArticleType.IndefinitePluralFemenine, femenine);
			articles.Add (ArticleType.IndefinitePluralMasculine, masculine);
			articles.Add (ArticleType.IndefinitePluralNeutral, neutral);
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

		if ((canBeIrregular) && (irregularChance >= 0.01f)) {
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

	public void GenerateGeneralProperties (GetRandomFloatDelegate getRandomFloat) {

		if (getRandomFloat () < 0.75f) {

			Properties |= GeneralProperties.HasDefiniteSingularArticles;
		}

		if (getRandomFloat () < 0.75f) {

			Properties |= GeneralProperties.HasDefinitePluralArticles;
		}

		if (getRandomFloat () < 0.75f) {

			Properties |= GeneralProperties.HasIndefiniteSingularArticles;
		}

		if (getRandomFloat () < 0.75f) {

			Properties |= GeneralProperties.HasIndefinitePluralArticles;
		}
	}

	public void GenerateArticleProperties (GetRandomFloatDelegate getRandomFloat) {

		ArticleAdjuntProperties = GenerateNounAdjuntProperties (getRandomFloat);
	}

	public void GenerateArticleSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.1f, getRandomFloat, 10);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 5);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.25f, 0.1f, getRandomFloat, 4);

		ArticleStartSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
		ArticleNextSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 50);
	}

	public void GenerateAllArticles (GetRandomFloatDelegate getRandomFloat) {

		_articles = GenerateArticles (ArticleStartSyllables, ArticleNextSyllables, Properties, ArticleAdjuntProperties, getRandomFloat);

		Articles = new List<WordMeaningPair> (_articles.Count);

		foreach (KeyValuePair<string, Word> pair in _articles) {
		
			Articles.Add (new WordMeaningPair (pair.Value, pair.Key));
		}
	}

	public void GeneratePluralizerProperties (GetRandomFloatDelegate getRandomFloat) {

		PluralizerAdjuntProperties = GenerateNounAdjuntProperties (getRandomFloat);
	}

	public void GeneratePluralizerSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.2f, 0.1f, getRandomFloat, 5);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 0.5f, 0.35f, getRandomFloat, 10);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.75f, 0.2f, getRandomFloat, 20);

		PluralizerStartSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 10);
		PluralizerNextSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 10);
	}

	public void GeneratePluralizers (GetRandomFloatDelegate getRandomFloat) {

		Word word = new Word ();
		word.Value = GenerateSimpleWord (PluralizerStartSyllables, PluralizerNextSyllables, 0.1f, getRandomFloat);
		word.Properties = WordProperties.None;
		word.Type = WordType.Pluralizer;

		_pluralizers = new Dictionary<string, Word> ();
		_pluralizers.Add ("neutral", word);

		Pluralizers = new List<WordMeaningPair> ();
		Pluralizers.Add (new WordMeaningPair (word, "neutral"));
	}

	public void GenerateAdpositionProperties (GetRandomFloatDelegate getRandomFloat) {

		AdpositionAdjuntProperties = GenerateNounAdjuntProperties (getRandomFloat);
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

		_adpositions.Add (relation, word);

		Adpositions.Add (new WordMeaningPair (word, relation));
	}

	public void GenerateSimpleNounSyllables (GetRandomFloatDelegate getRandomFloat) {

		CharacterGroup[] onsetGroups = Language.GenerateCharacterGroups (Language.OnsetLetters, 0.7f, 0.25f, getRandomFloat, 20);
		CharacterGroup[] nucleusGroups = Language.GenerateCharacterGroups (Language.NucleusLetters, 1.0f, 0.35f, getRandomFloat, 10);
		CharacterGroup[] codaGroups = Language.GenerateCharacterGroups (Language.CodaLetters, 0.5f, 0.25f, getRandomFloat, 8);

		SimpleNounWordStartSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 200);
		SimpleNounWordNextSyllables = Language.GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 200);
	}

	public Word GenerateSimpleNoun (string meaning, GetRandomFloatDelegate getRandomFloat, bool isPlural, bool randomGender, bool isFemenine = false, bool isNeutral = false, bool canBeIrregular = true) {

		return GenerateSimpleNoun (meaning, GenerateWordProperties (getRandomFloat, isPlural, randomGender, isFemenine, isNeutral, canBeIrregular), getRandomFloat);
	}

	public Word GenerateSimpleNoun (string meaning, WordProperties properties, GetRandomFloatDelegate getRandomFloat) {

		if (_nouns.ContainsKey (meaning)) {
		
			return _nouns [meaning];
		}

		Word word = new Word ();
		word.Value = GenerateSimpleWord (SimpleNounWordStartSyllables, SimpleNounWordNextSyllables, 0.5f, getRandomFloat);
		word.Properties = properties;
		word.Type = WordType.Noun;

		_nouns.Add (meaning, word);

		Nouns.Add (new WordMeaningPair (word, meaning));

		return word;
	}

	public Word GetAppropiateArticle (WordProperties nounProperties, bool useIndefiniteForm) {

		Word article = null;

		if (useIndefiniteForm) {
			if ((nounProperties & WordProperties.Plural) == WordProperties.Plural) {

				if ((nounProperties & WordProperties.Femenine) == WordProperties.Femenine) {

					article = _articles [ArticleType.IndefinitePluralFemenine];

				} else if ((nounProperties & WordProperties.Neutral) == WordProperties.Neutral) {

					article = _articles [ArticleType.IndefinitePluralNeutral];

				} else {

					article = _articles [ArticleType.IndefinitePluralMasculine];

				}
			} else {

				if ((nounProperties & WordProperties.Femenine) == WordProperties.Femenine) {

					article = _articles [ArticleType.IndefiniteSingularFemenine];

				} else if ((nounProperties & WordProperties.Neutral) == WordProperties.Neutral) {

					article = _articles [ArticleType.IndefiniteSingularNeutral];

				} else {

					article = _articles [ArticleType.IndefiniteSingularMasculine];
				}
			}
		} else {
			if ((nounProperties & WordProperties.Plural) == WordProperties.Plural) {

				if ((nounProperties & WordProperties.Femenine) == WordProperties.Femenine) {

					article = _articles [ArticleType.DefinitePluralFemenine];

				} else if ((nounProperties & WordProperties.Neutral) == WordProperties.Neutral) {

					article = _articles [ArticleType.DefinitePluralNeutral];

				} else {

					article = _articles [ArticleType.DefinitePluralMasculine];

				}
			} else {

				if ((nounProperties & WordProperties.Femenine) == WordProperties.Femenine) {

					article = _articles [ArticleType.DefiniteSingularFemenine];

				} else if ((nounProperties & WordProperties.Neutral) == WordProperties.Neutral) {

					article = _articles [ArticleType.DefiniteSingularNeutral];

				} else {

					article = _articles [ArticleType.DefiniteSingularMasculine];
				}
			}
		}

		return article;
	}

	public Phrase BuildArticulatedNounPhrase (string noun, bool useIndefiniteForm) {

		Phrase phrase = new Phrase ();
	
		Word word = null;

		if (!_nouns.TryGetValue (noun, out word)) {

			return phrase;
		}

		bool usePluralForm = ((word.Properties & WordProperties.Plural) == WordProperties.Plural);

		string meaning;

		if (useIndefiniteForm) {

			if (usePluralForm) {
				meaning = noun;

			} else {

				if (NucleousStartRegex.IsMatch (noun)) {
					meaning = "an " + noun;
				} else {
					meaning = "a " + noun;
				}
			}

		} else {
			
			meaning = "the " + noun;
		}

		string text = word.Value;

		bool hasArticles = false;

		if (usePluralForm) {
			if (useIndefiniteForm) {
				if ((Properties & GeneralProperties.HasIndefinitePluralArticles) == GeneralProperties.HasIndefinitePluralArticles) {

					hasArticles = true;
				}

			} else {
				if ((Properties & GeneralProperties.HasDefinitePluralArticles) == GeneralProperties.HasDefinitePluralArticles) {

					hasArticles = true;
				}
			}
		} else {
			if (useIndefiniteForm) {
				if ((Properties & GeneralProperties.HasIndefiniteSingularArticles) == GeneralProperties.HasIndefiniteSingularArticles) {

					hasArticles = true;
				}

			} else {
				if ((Properties & GeneralProperties.HasDefiniteSingularArticles) == GeneralProperties.HasDefiniteSingularArticles) {

					hasArticles = true;
				}
			}
		}

		if (hasArticles) {
			Word article = GetAppropiateArticle (word.Properties, useIndefiniteForm);
			string articleString = article.Value;
		
			if ((ArticleAdjuntProperties & NounAdjuntProperties.GoesAfterNoun) == NounAdjuntProperties.GoesAfterNoun) {
		
				if ((ArticleAdjuntProperties & NounAdjuntProperties.IsAffixed) == NounAdjuntProperties.IsAffixed) {

					if ((ArticleAdjuntProperties & NounAdjuntProperties.IsLinkedWithDash) == NounAdjuntProperties.IsLinkedWithDash) {

						text += "-" + articleString;
					} else {
					
						text += articleString;
					}
				} else {

					text += " " + articleString;
				}
			} else {
				if ((ArticleAdjuntProperties & NounAdjuntProperties.IsAffixed) == NounAdjuntProperties.IsAffixed) {

					if ((ArticleAdjuntProperties & NounAdjuntProperties.IsLinkedWithDash) == NounAdjuntProperties.IsLinkedWithDash) {

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

		if (!_adpositions.TryGetValue (relation, out adposition)) {

			return phrase;
		}

		string meaning = relation + " " + complementPhrase.Meaning;

		string text = complementPhrase.Text;

		if ((AdpositionAdjuntProperties & NounAdjuntProperties.GoesAfterNoun) == NounAdjuntProperties.GoesAfterNoun) {

			if ((AdpositionAdjuntProperties & NounAdjuntProperties.IsAffixed) == NounAdjuntProperties.IsAffixed) {

				if ((AdpositionAdjuntProperties & NounAdjuntProperties.IsLinkedWithDash) == NounAdjuntProperties.IsLinkedWithDash) {

					text += "-" + adposition.Value;
				} else {

					text += adposition.Value;
				}
			} else {

				text += " " + adposition.Value;
			}
		} else {
			if ((AdpositionAdjuntProperties & NounAdjuntProperties.IsAffixed) == NounAdjuntProperties.IsAffixed) {

				if ((AdpositionAdjuntProperties & NounAdjuntProperties.IsLinkedWithDash) == NounAdjuntProperties.IsLinkedWithDash) {

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

	public Phrase GenerateNounTranslation (string word, GetRandomFloatDelegate getRandomFloat) {

		string[] wordParts = word.Split (new char[] { ':' });

		List<Word> translatedParts = new List<Word> (wordParts.Length);

		foreach (string wordPart in wordParts) {
		
			WordPart parsedPart = ParseWordPart (wordPart);

			if (parsedPart.Attributes.Contains (WordPartAttributeId.NounPluralizer)) {
			
				translatedParts.Add (_pluralizers ["neutral"]);

			} else {

				bool hasRandomGender = true;
				bool isFemenineNoun = true;
				bool isNeutralNoun = true;

				if (parsedPart.Attributes.Contains (WordPartAttributeId.FemenineNoun)) {
					hasRandomGender = false;
					isFemenineNoun = true;
				} else if (parsedPart.Attributes.Contains (WordPartAttributeId.FemenineNoun)) {
					hasRandomGender = false;
				} else if (parsedPart.Attributes.Contains (WordPartAttributeId.NeutralNoun)) {
					hasRandomGender = false;
					isNeutralNoun = true;
				}

				translatedParts.Add (GenerateSimpleNoun (parsedPart.Value, getRandomFloat, false, hasRandomGender, isFemenineNoun, isNeutralNoun));
			}
		}

		return null;
	}

	public static WordPart ParseWordPart (string wordPart) {

		WordPart parsedWordPart = new WordPart ();

		while (true) {
			Match match = WordPartTypeRegex.Match (wordPart);

			if (!match.Success)
				break;

			wordPart = wordPart.Replace (match.Value, match.Groups ["word"].Value);

			parsedWordPart.Attributes.Add (match.Groups ["attr"].Value);
		}

		parsedWordPart.Value = wordPart;

		return parsedWordPart;
	}

	public static string MakeFirstLetterUpper (string sentence) {

		return sentence.First().ToString().ToUpper() + sentence.Substring(1);
	}

	public static string CleanConstructCharacters (string phrase) {

		while (true) {
			Match match = WordPartTypeRegex.Match (phrase);

			if (!match.Success)
				break;

			phrase = phrase.Replace (match.Value, match.Groups["word"].Value);
		}

		return phrase.Replace (":", string.Empty);
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

		foreach (WordMeaningPair pair in Articles) {
			_articles.Add (pair.Meaning, pair.Word);
		}

		foreach (WordMeaningPair pair in Adpositions) {
			_adpositions.Add (pair.Meaning, pair.Word);
		}

		foreach (WordMeaningPair pair in Nouns) {
			_nouns.Add (pair.Meaning, pair.Word);
		}
	}
}
