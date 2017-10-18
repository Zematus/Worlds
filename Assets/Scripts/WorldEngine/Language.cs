using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

public class Language : ISynchronizable {

	[XmlAttribute]
	public long Id;

	public delegate float GetRandomFloatDelegate ();

	public class Phrase : ISynchronizable {

		[XmlAttribute]
		public string Original;
		[XmlAttribute]
		public string Meaning;
		[XmlAttribute]
		public string Text;

		[XmlAttribute("Properties")]
		public int PropertiesInt;

		[XmlIgnore]
		public PhraseProperties Properties;

		public void Synchronize () {

			PropertiesInt = (int)Properties;
		}

		public void FinalizeLoad () {

			Properties = (PhraseProperties)PropertiesInt;
		}

		public Phrase () {
			
		}

		public Phrase (Phrase phrase) {

			Original = phrase.Original;
			Meaning = phrase.Meaning;
			Text = phrase.Text;

			Properties = phrase.Properties;
		}
	}

	public class Morpheme : ISynchronizable {

		[XmlAttribute]
		public string Meaning;
		[XmlAttribute]
		public string Value;


		[XmlAttribute]
		public WordType Type;

		[XmlAttribute("Properties")]
		public int PropertiesInt;

		[XmlIgnore]
		public MorphemeProperties Properties;

		public void Synchronize () {

			PropertiesInt = (int)Properties;
		}

		public void FinalizeLoad () {

			Properties = (MorphemeProperties)PropertiesInt;
		}
	}

	public class ParsedWord {

		public string Value;
		public Dictionary<string, string[]> Attributes = new Dictionary<string, string[]> ();
	}

	public class ParsedPhrase {

		public string Value;
		public Dictionary<string, string[]> Attributes = new Dictionary<string, string[]> ();

		public Dictionary<int, ParsedPhrase> SubPhrases = new Dictionary<int, ParsedPhrase> ();
	}

	public class Letter : CollectionUtility.ElementWeightPair<string> {

		public Letter () {

		}

		public Letter (string letter, float weight) : base (letter, weight) {

		}
	}

	public class CharacterGroup : CollectionUtility.ElementWeightPair<string> {

		public CharacterGroup () {
			
		}

		public CharacterGroup (string characters, float weight) : base (characters, weight) {

		}

		public string Characters {
			get { 
				return Value;
			}
		}
	}

	public static class VerbConjugationKeys {
		
		public const string FirstPerson = "first";
		public const string SecondPerson = "second";
		public const string ThirdPerson = "third";

		public const string FirstPersonSingular = "fs";
		public const string SecondPersonSingular = "ss";
		public const string ThirdPersonSingular = "ts";

		public const string FirstPersonPlural = "fp";
		public const string SecondPersonPlural = "sp";
		public const string ThirdPersonPlural = "tp";
	}

	public static class VerbTenses {

		public const string Null = "null";
		public const string Present = "present";
		public const string Past = "past";
		public const string Future = "future";
		public const string Infinitive = "infinitive";
	}

	public static class ParsedWordAttributeId {

		public const string FemenineNoun = "fn";
		public const string MasculineNoun = "mn";
		public const string NeutralNoun = "nn";
		public const string UncountableNoun = "un";
		public const string NominalizedRegularVerb = "nrv";
		public const string NominalizedIrregularVerb = "niv";
		public const string IrregularPluralNoun = "ipn";
		public const string NounAdjunct = "nad";
		public const string Adjective = "adj";
		public const string RegularVerb = "rv";
		public const string IrregularVerb = "iv";
		public const string Name = "name";
//		public const string Preposition = "pre";
//		public const string Import = "import";
	}

	public static class ParsedPhraseAttributeId {

		public const string PhrasePlusPrepositionalPhrase = "PpPP";
		public const string PrepositionalPhrase = "PP";
		public const string NounPhrase = "NP";
		public const string Noun = "Noun";
	}

	public enum WordType
	{
		Article,
		Indicative,
		Adposition,
		Adjective,
		Noun,
		Verb
	}

	public enum GeneralArticleProperties 
	{
		HasDefiniteSingularArticles = 0x001,
		HasDefinitePluralArticles = 0x002,
		HasIndefiniteSingularArticles = 0x004,
		HasIndefinitePluralArticles = 0x008,
		HasUncountableArticles = 0x010
	}

	public enum GeneralNounIndicativeProperties 
	{
		HasMasculineIndicative = 0x001,
		HasFemenineIndicative = 0x002,
		HasNeutralIndicative = 0x004,
		HasSingularIndicative = 0x008,
		HasPluralIndicative = 0x010,
		HasDefiniteIndicative = 0x020,
		HasIndefiniteIndicative = 0x040,
		HasUncountableIndicative = 0x080
	}

	public enum GeneralVerbIndicativeProperties 
	{
		HasFirstPersonIndicative = 0x0001,
		HasSecondPersonIndicative = 0x0002,
		HasThirdPersonIndicative = 0x0004,

		HasSingularIndicative = 0x0008,
		HasPluralIndicative = 0x0010,

		HasActiveNominalizationIndicative = 0x0020,
		HasPassiveNominalizationIndicative = 0x0040,

		HasPresentTenseIndicative = 0x0080,
		HasPastTenseIndicative = 0x0100,
		HasFutureTenseIndicative = 0x0200,
		HasInfinitiveTenseIndicative = 0x0400
	}

	public enum AdjunctionProperties
	{
		None = 0x00,
		IsAffixed = 0x01,
		GoesAfter = 0x02,
		IsLinkedWithDash = 0x04,

		IsSuffixed = 0x03,
		GoesAfterNounAndLinkedWithDash = 0x06
	}

	public enum MorphemeProperties
	{
		None = 0x0000,

		Plural = 0x0001,
		Uncountable = 0x0002,

		Indefinite = 0x0004,

		Femenine = 0x0008,
		Neutral = 0x0010,

		FirstPerson = 0x0020,
		SecondPerson = 0x0040,
		ThirdPerson = 0x0080,

		Passive = 0x0100
	}

	public enum PhraseProperties
	{
		None = 0x0000,

		Plural = 0x0001,
		Uncountable = 0x0002,

		Indefinite = 0x0004,

		Femenine = 0x0008,
		Neutral = 0x0010,

		FirstPerson = 0x0020,
		SecondPerson = 0x0040,
		ThirdPerson = 0x0080,

		Passive = 0x0100
	}

	public static class IndicativeType
	{
		public const string Definite = "Definite";
		public const string Indefinite = "Indefinite";

		public const string Singular = "Singular";
		public const string Plural = "Plural";

		public const string Uncountable = "Uncountable";

		public const string Masculine = "Masculine";
		public const string Femenine = "Femenine";
		public const string Neutral = "Neutral";

		public const string FirstPerson = "FirstPerson";
		public const string SecondPerson = "SecondPerson";
		public const string ThirdPerson = "ThirdPerson";

		public const string NullTense = "Null";
		public const string PresentTense = "Present";
		public const string PastTense = "Past";
		public const string FutureTense = "Future";
		public const string InfinitiveTense = "Infinitive";

		public const string ActiveNominalization = "ActiveNominalization";
		public const string PassiveNominalization = "PassiveNominalization";

		public const string DefiniteSingularMasculine = Definite + Singular + Masculine;// = "DefiniteSingularMasculine";
		public const string DefiniteSingularFemenine = Definite + Singular + Femenine;// = "DefiniteSingularFemenine";
		public const string DefiniteSingularNeutral = Definite + Singular + Neutral;// = "DefiniteSingularNeutral";
		public const string DefinitePluralMasculine = Definite + Plural + Masculine;// = "DefinitePluralMasculine";
		public const string DefinitePluralFemenine = Definite + Plural + Femenine;// "DefinitePluralFemenine";
		public const string DefinitePluralNeutral = Definite + Plural + Neutral;// "DefinitePluralNeutral";
		public const string IndefiniteSingularMasculine = Indefinite + Singular + Masculine;// = "IndefiniteSingularMasculine";
		public const string IndefiniteSingularFemenine = Indefinite + Singular + Femenine;// = "IndefiniteSingularFemenine";
		public const string IndefiniteSingularNeutral = Indefinite + Singular + Neutral;// = "IndefiniteSingularNeutral";
		public const string IndefinitePluralMasculine = Indefinite + Plural + Masculine;// = "IndefinitePluralMasculine";
		public const string IndefinitePluralFemenine = Indefinite + Plural + Femenine;// = "IndefinitePluralFemenine";
		public const string IndefinitePluralNeutral = Indefinite + Plural + Neutral;// = "IndefinitePluralNeutral";
		public const string UncountableMasculine = Uncountable + Masculine;// = "UncountableMasculine";
		public const string UncountableFemenine = Uncountable + Femenine;// = "UncountableFemenine";
		public const string UncountableNeutral = Uncountable + Neutral;// = "UncountableNeutral";
	}

	// based on frequency of consonants across languages. source: http://phoible.org/
	public static Letter[] OnsetLetters = new Letter[] { 
		new Letter ("m", 0.95f),
		new Letter ("k", 0.94f),
		new Letter ("j", 0.88f),
		new Letter ("p", 0.87f),
		new Letter ("w", 0.84f),
		new Letter ("n", 0.81f),
		new Letter ("s", 0.77f),
		new Letter ("t", 0.74f),
		new Letter ("b", 0.70f),
		new Letter ("l", 0.65f),
		new Letter ("h", 0.65f),
		new Letter ("d", 0.53f),
		new Letter ("f", 0.48f),
		new Letter ("r", 0.37f),
		new Letter ("z", 0.31f),
		new Letter ("v", 0.29f),
		new Letter ("ts", 0.23f),
		new Letter ("x", 0.18f),
		new Letter ("kp", 0.17f),
		new Letter ("c", 0.14f),
		new Letter ("mb", 0.14f),
		new Letter ("nd", 0.12f),
		new Letter ("dz", 0.1f),
		new Letter ("q", 0.09f),
		new Letter ("y", 0.038f),
		new Letter ("ndz", 0.02f),
		new Letter ("nz", 0.02f),
		new Letter ("mp", 0.016f),
		new Letter ("pf", 0.017f),
		new Letter ("nts", 0.0037f),
		new Letter ("tr", 0.0028f),
		new Letter ("dr", 0.0028f),
		new Letter ("tx", 0.0023f),
		new Letter ("kx", 0.0023f),
		new Letter ("ndr", 0.0023f),
		new Letter ("ps", 0.0018f),
		new Letter ("dl", 0.00093f),
		new Letter ("nr", 0.00092f),
		new Letter ("nh", 0.00092f),
		new Letter ("nl", 0.00092f),
		new Letter ("tn", 0.00092f),
		new Letter ("pm", 0.00092f),
		new Letter ("tl", 0.00092f),
		new Letter ("xh", 0.00046f),
		new Letter ("mv", 0.00046f),
		new Letter ("ld", 0.00046f),
		new Letter ("mw", 0.00046f),
		new Letter ("br", 0.00046f),
		new Letter ("qn", 0.00046f)
	};

	// based on frequency of vowels across languages. source: http://phoible.org/
	public static Letter[] NucleusLetters = new Letter[] { 
		new Letter ("i", 0.93f),
		new Letter ("a", 0.91f),
		new Letter ("u", 0.87f),
		new Letter ("o", 0.68f),
		new Letter ("e", 0.68f),
		new Letter ("y", 0.04f),
		new Letter ("ai", 0.03f),
		new Letter ("au", 0.02f),
		new Letter ("ia", 0.01f),
		new Letter ("ui", 0.01f),
		new Letter ("ie", 0.005f),
		new Letter ("iu", 0.004f),
		new Letter ("uo", 0.0037f),
		new Letter ("ea", 0.0028f),
		new Letter ("oa", 0.0023f),
		new Letter ("ao", 0.0023f),
		new Letter ("eu", 0.0023f),
		new Letter ("ue", 0.0018f),
		new Letter ("ae", 0.0018f),
		new Letter ("oe", 0.0013f),
		new Letter ("ay", 0.00092f),
		new Letter ("ye", 0.00046f)
	};

	// based on frequency of consonants across languages. source: http://phoible.org/
	public static Letter[] CodaLetters = new Letter[] { 
		new Letter ("m", 0.95f),
		new Letter ("k", 0.94f),
		new Letter ("j", 0.88f),
		new Letter ("p", 0.87f),
		new Letter ("w", 0.84f),
		new Letter ("n", 0.81f),
		new Letter ("s", 0.77f),
		new Letter ("t", 0.74f),
		new Letter ("b", 0.70f),
		new Letter ("l", 0.65f),
		new Letter ("h", 0.65f),
		new Letter ("d", 0.53f),
		new Letter ("f", 0.48f),
		new Letter ("r", 0.37f),
		new Letter ("z", 0.31f),
		new Letter ("v", 0.29f),
		new Letter ("ts", 0.23f),
		new Letter ("x", 0.18f),
		new Letter ("kp", 0.17f),
		new Letter ("c", 0.14f),
		new Letter ("mb", 0.14f),
		new Letter ("nd", 0.12f),
		new Letter ("dz", 0.1f),
		new Letter ("q", 0.09f),
		new Letter ("y", 0.038f),
		new Letter ("ndz", 0.02f),
		new Letter ("nz", 0.02f),
		new Letter ("mp", 0.016f),
		new Letter ("pf", 0.017f),
		new Letter ("nts", 0.0037f),
		new Letter ("tr", 0.0028f),
		new Letter ("dr", 0.0028f),
		new Letter ("tx", 0.0023f),
		new Letter ("kx", 0.0023f),
		new Letter ("ndr", 0.0023f),
		new Letter ("ps", 0.0018f),
		new Letter ("dl", 0.00093f),
		new Letter ("nr", 0.00092f),
		new Letter ("nh", 0.00092f),
		new Letter ("nl", 0.00092f),
		new Letter ("tn", 0.00092f),
		new Letter ("pm", 0.00092f),
		new Letter ("tl", 0.00092f),
		new Letter ("xh", 0.00046f),
		new Letter ("mv", 0.00046f),
		new Letter ("ld", 0.00046f),
		new Letter ("mw", 0.00046f),
		new Letter ("br", 0.00046f),
		new Letter ("qn", 0.00046f)
	};

	public static Regex StartsWithVowelRegex = new Regex (@"^[aeiou]");
	public static Regex EndsWithVowelsRegex = new Regex (@"(?>[aeiou]+)(?>[^aeiou]+)(?<vowels>(?>[aeiou]+))$");
	public static Regex EndsWithConsonantsRegex = new Regex (@"(?>[^aeiou]+)$");

//	public static Regex PhrasePartRegex = new Regex (@"\[(?<attr>\w+)(?:\((?<params>(?:\w+,?)+)\))?\](?:\[w+\])*(?<phrase>(((?'Open'\()[^\(\)]*)+((?'Close-Open'\))[^\(\)]*)+)*(?(Open)(?!)))");
	public static Regex PhrasePartRegex = new Regex (@"\[(?<attr>\w+)(?:\((?<params>(?:\w+,?)+)\))?\](?<phrase>(?:\[\w+(?:\((?:\w+,?)+\))?\])*\((?<value>[^\(\)]+)\))");
	public static Regex WordPartRegex = new Regex (@"\[(?<attr>\w+)(?:\((?<params>(?:\w+,?)+)\))?\](?<word>(?>(?:\[\w+(?:\((?:\w+,?)+\))?\])*[\w\'\-]+))");
	public static Regex PhraseIndexRegex = new Regex (@"{(?<index>\d+)}");
	public static Regex ArticleRegex = new Regex (@"^((?<def>the)|(?<indef>(a|an)))$");
	public static Regex PluralSuffixRegex = new Regex (@"^(es|s)$");
	public static Regex VerbNominalizationSuffixRegex = new Regex (@"^(er|r)$");
	public static Regex ConjugationSuffixRegex = new Regex (@"^(ed|d|s)$");

	[XmlAttribute("ArticleProperties")]
	public int ArticlePropertiesInt;
	[XmlAttribute("NounIndicativeProperties")]
	public int NounIndicativePropertiesInt;
	[XmlAttribute("VerbIndicativeProperties")]
	public int VerbIndicativePropertiesInt;

	private GeneralArticleProperties _articleProperties;
	private GeneralNounIndicativeProperties _nounIndicativeProperties;
	private GeneralVerbIndicativeProperties _verbIndicativeProperties;

	[XmlAttribute("ArticleAdjunctionProperties")]
	public int ArticleAdjunctionPropertiesInt;
	[XmlAttribute("NounIndicativeAdjunctionProperties")]
	public int NounIndicativeAdjunctionPropertiesInt;
	[XmlAttribute("VerbIndicativeAdjunctionProperties")]
	public int VerbIndicativeAdjunctionPropertiesInt;
	[XmlAttribute("AdpositionAdjunctionProperties")]
	public int AdpositionAdjunctionPropertiesInt;
	[XmlAttribute("AdjectiveAdjunctionProperties")]
	public int AdjectiveAdjunctionPropertiesInt;
	[XmlAttribute("NounAdjunctionProperties")]
	public int NounAdjunctionPropertiesInt;

	[XmlIgnore]
	public AdjunctionProperties ArticleAdjunctionProperties;
	[XmlIgnore]
	public AdjunctionProperties NounIndicativeAdjunctionProperties;
	[XmlIgnore]
	public AdjunctionProperties VerbIndicativeAdjunctionProperties;
	[XmlIgnore]
	public AdjunctionProperties AdpositionAdjunctionProperties;
	[XmlIgnore]
	public AdjunctionProperties AdjectiveAdjunctionProperties;
	[XmlIgnore]
	public AdjunctionProperties NounAdjunctionProperties;

	public class SyllableSet
	{
		public const int AddSyllableModifier = 8;

		[XmlAttribute("OSALC")]
		public float OnsetChance;

		[XmlAttribute("NSALC")]
		public float NucleusChance;

		[XmlAttribute("CSALC")]
		public float CodaChance;

		public List<string> Syllables = new List<string> ();

		public SyllableSet () {
			
		}

		public string GetRandomSyllable (GetRandomFloatDelegate getRandomFloat) {

			int selCount = Syllables.Count + AddSyllableModifier;

			int randOption = (int)Mathf.Floor (selCount * getRandomFloat ());

			if (randOption < Syllables.Count) {
			
				return Syllables [randOption];
			}

			string syllable = GenerateSyllable (OnsetLetters, OnsetChance, NucleusLetters, NucleusChance, CodaLetters, CodaChance, getRandomFloat);

			if (!Syllables.Contains (syllable)) {
				Syllables.Add (syllable);
			}

			return syllable;
		}
	}

	public SyllableSet ArticleSyllables = new SyllableSet ();
	public SyllableSet DerivativeArticleStartSyllables = new SyllableSet ();
	public SyllableSet DerivativeArticleNextSyllables = new SyllableSet ();

	public SyllableSet NounIndicativeSyllables = new SyllableSet ();
	public SyllableSet DerivativeNounIndicativeStartSyllables = new SyllableSet ();
	public SyllableSet DerivativeNounIndicativeNextSyllables = new SyllableSet ();

	public SyllableSet VerbIndicativeSyllables = new SyllableSet ();
	public SyllableSet DerivativeVerbIndicativeStartSyllables = new SyllableSet ();
	public SyllableSet DerivativeVerbIndicativeNextSyllables = new SyllableSet ();

	public SyllableSet AdpositionStartSyllables = new SyllableSet ();
	public SyllableSet AdpositionNextSyllables = new SyllableSet ();

	public SyllableSet AdjectiveStartSyllables = new SyllableSet ();
	public SyllableSet AdjectiveNextSyllables = new SyllableSet ();

	public SyllableSet NounStartSyllables = new SyllableSet ();
	public SyllableSet NounNextSyllables = new SyllableSet ();

	public SyllableSet VerbStartSyllables = new SyllableSet ();
	public SyllableSet VerbNextSyllables = new SyllableSet ();

	public List<Morpheme> Articles;
	public List<Morpheme> NounIndicatives;
	public List<Morpheme> VerbIndicatives;

	public List<Morpheme> Adpositions = new List<Morpheme> ();
	public List<Morpheme> Adjectives = new List<Morpheme> ();
	public List<Morpheme> Nouns = new List<Morpheme> ();
	public List<Morpheme> Verbs = new List<Morpheme> ();

	private Dictionary<string, Morpheme> _articles;
	private Dictionary<string, Morpheme> _nounIndicatives;
	private Dictionary<string, Morpheme> _verbIndicatives;

	private Dictionary<string, Morpheme> _adpositions = new Dictionary<string, Morpheme> ();
	private HashSet<string> _existingAdpositionMorphemeValues = new HashSet<string> ();

	private Dictionary<string, Morpheme> _adjectives = new Dictionary<string, Morpheme> ();
	private HashSet<string> _existingAdjectiveMorphemeValues = new HashSet<string> ();

	private Dictionary<string, Morpheme> _nouns = new Dictionary<string, Morpheme> ();
	private Dictionary<string, float> _existingNounMorphemeValues = new Dictionary<string, float> ();

	private Dictionary<string, Morpheme> _verbs = new Dictionary<string, Morpheme> ();
	private HashSet<string> _existingVerbMorphemeValues = new HashSet<string> ();

	private const float _initialHomographTolerance = 0.1f;
	private const float _homographToleranceDecayFactor = 0.1f;

	private const float _irregularPluralNounFrequency = 0.025f;

	public Language () {
		
	}

	public Language (long id) {
	
		Id = id;
	}

	public static CharacterGroup GenerateCharacterGroup (Letter[] letterSet, GetRandomFloatDelegate getRandomFloat) {

		float totalWeight = 0;

		foreach (Letter letter in letterSet) {
		
			totalWeight += letter.Weight;
		}

		Letter chossenLetter = CollectionUtility.WeightedSelection (letterSet, totalWeight, () => getRandomFloat ()) as Letter;

		return new CharacterGroup (chossenLetter.Value, getRandomFloat () * chossenLetter.Weight);
	}

	private static float GetLettersTotalWeight (Letter[] letters) {

		float totalWeight = 0;

		foreach (Letter letter in letters) {

			totalWeight += letter.Weight;
		}

		return totalWeight;
	}

	public static string GenerateSyllable (
		Letter[] onsetLetters,
		float onsetChance,
		Letter[] nucleusLetters,
		float nucleusChance,
		Letter[] codaLetters,
		float codaChance,
		GetRandomFloatDelegate getRandomFloat) {

		CollectionUtility.NormalizedValueGeneratorDelegate valueGeneratorDelegate = new CollectionUtility.NormalizedValueGeneratorDelegate (getRandomFloat);

		string onset = (onsetChance > getRandomFloat ()) ? CollectionUtility.WeightedSelection (onsetLetters, GetLettersTotalWeight (onsetLetters), valueGeneratorDelegate) : string.Empty;
		string nucleus = (nucleusChance > getRandomFloat ()) ? CollectionUtility.WeightedSelection (nucleusLetters, GetLettersTotalWeight (nucleusLetters), valueGeneratorDelegate) : string.Empty;
		string coda = (codaChance > getRandomFloat ()) ? CollectionUtility.WeightedSelection (codaLetters, GetLettersTotalWeight (codaLetters), valueGeneratorDelegate) : string.Empty;

		if (nucleus == string.Empty) {
		
			return coda;
		}

		return onset + nucleus + coda;
	}

	public static string GenerateMorpheme (
		SyllableSet syllables,
		GetRandomFloatDelegate getRandomFloat) {

		return GenerateMorpheme (syllables, syllables, 0, getRandomFloat);
	}


	public static string GenerateMorpheme (
		SyllableSet startSyllables, 
		SyllableSet nextSyllables, 
		float addSyllableChanceDecay, 
		GetRandomFloatDelegate getRandomFloat) {

		float addSyllableChance = 2;
		bool first = true;

		string morpheme = "";

		while (getRandomFloat () < addSyllableChance) {

			SyllableSet syllables = nextSyllables;

			if (first) {
				syllables = startSyllables;
				first = false;
			}

//			int syllableIndex = (int)Mathf.Floor (syllables.Count * getRandomFloat ());
//
//			if (syllableIndex == syllables.Count) {
//				throw new System.Exception ("syllable index out of bounds");
//			}
//
//			morpheme += syllables[syllableIndex];

			morpheme = Affix (morpheme, syllables.GetRandomSyllable (getRandomFloat));

			addSyllableChance *= addSyllableChanceDecay;// * addSyllableChanceDecay;
		}

		return morpheme;
	}

	public static string GenerateDerivatedWord (
		string rootWord, 
		float noChangeChance, 
		float replaceChance, 
		SyllableSet syllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		GetRandomFloatDelegate getRandomFloat) {

		return GenerateDerivatedWord (rootWord, noChangeChance, replaceChance, syllables, syllables, derivativeStartSyllables, derivativeNextSyllables, 0.0f, getRandomFloat);
	}

	public static string GenerateDerivatedWord (
		string rootWord, 
		float noChangeChance, 
		float replaceChance, 
		SyllableSet startSyllables, 
		SyllableSet nextSyllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		float addSyllableChanceDecay, 
		GetRandomFloatDelegate getRandomFloat) {

		float randomFloat = getRandomFloat ();

		if (randomFloat < noChangeChance)
			return rootWord;

		if (randomFloat >= (1f - replaceChance)) {
		
			return GenerateMorpheme (startSyllables, nextSyllables, addSyllableChanceDecay, getRandomFloat);
		}

		if (getRandomFloat () < 0.5f) {
		
			return Affix (GenerateMorpheme (derivativeStartSyllables, derivativeNextSyllables, addSyllableChanceDecay, getRandomFloat), rootWord);
		}

		return Affix (rootWord, GenerateMorpheme (derivativeNextSyllables, derivativeNextSyllables, addSyllableChanceDecay, getRandomFloat));
	}

	public static AdjunctionProperties GenerateAdjunctionProperties (
		GetRandomFloatDelegate getRandomFloat, 
		float goesAfterNounChance,
		float isAffixedChance,
		float isLinkedWithDashChance, 
		float isAffixedChance_after,
		float isLinkedWithDashChance_after
	) {

		AdjunctionProperties properties = AdjunctionProperties.None;

		if (getRandomFloat () < goesAfterNounChance) {

			properties |= AdjunctionProperties.GoesAfter;

			float random = getRandomFloat ();

			if (random < isAffixedChance_after) {

				properties |= AdjunctionProperties.IsAffixed;

			} else if (random < (isAffixedChance_after + isLinkedWithDashChance_after)) {

				properties |= AdjunctionProperties.IsLinkedWithDash;
			}

		} else {

			float random = getRandomFloat ();

			if (random < isAffixedChance) {

				properties |= AdjunctionProperties.IsAffixed;

			} else if (random < (isAffixedChance + isLinkedWithDashChance)) {

				properties |= AdjunctionProperties.IsLinkedWithDash;
			}

		}

		return properties;
	}

	public static string AdjunctionPropertiesToString (AdjunctionProperties properties) {

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

		if ((properties & AdjunctionProperties.GoesAfter) == AdjunctionProperties.GoesAfter) {

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

	public static Morpheme GenerateArticle (
		SyllableSet syllables, 
		MorphemeProperties properties, 
		GetRandomFloatDelegate getRandomFloat) {

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = GenerateMorpheme (syllables, getRandomFloat);
		morpheme.Properties = properties;
		morpheme.Type = WordType.Article;

		return morpheme;
	}

	public static Morpheme GenerateDerivatedArticle (
		Morpheme rootArticle, 
		SyllableSet syllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		MorphemeProperties properties, 
		GetRandomFloatDelegate getRandomFloat) {

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = GenerateDerivatedWord (rootArticle.Value, 0.4f, 0.5f, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat);
		morpheme.Properties = rootArticle.Properties | properties;
		morpheme.Type = WordType.Article;

		return morpheme;
	}

	public static void GenerateGenderedArticles (
		Morpheme root,
		SyllableSet syllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		GetRandomFloatDelegate getRandomFloat, 
		out Morpheme masculine, 
		out Morpheme femenine,
		out Morpheme neutral) {

		Morpheme firstVariant = GenerateDerivatedArticle (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);

		Morpheme secondVariant;
		if (getRandomFloat () < 0.5f) {
			secondVariant = GenerateDerivatedArticle (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
		} else {
			secondVariant = GenerateDerivatedArticle (firstVariant, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
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

		femenine.Properties |= MorphemeProperties.Femenine;
		neutral.Properties |= MorphemeProperties.Neutral;
	}

	public static Dictionary<string, Morpheme> GenerateArticles (
		SyllableSet syllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		GeneralArticleProperties generalProperties, 
		GetRandomFloatDelegate getRandomFloat) {

		Dictionary<string, Morpheme> articles = new Dictionary<string, Morpheme> ();

		Morpheme root = GenerateArticle (syllables, MorphemeProperties.None, getRandomFloat);

		Morpheme definite = GenerateDerivatedArticle (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
		Morpheme indefinite = GenerateDerivatedArticle (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Indefinite, getRandomFloat);
		Morpheme uncountable = GenerateDerivatedArticle (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Uncountable, getRandomFloat);

		if ((generalProperties & GeneralArticleProperties.HasDefiniteSingularArticles) == GeneralArticleProperties.HasDefiniteSingularArticles) {

			Morpheme definiteSingular = GenerateDerivatedArticle (definite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);

			Morpheme femenine, masculine, neutral;
			GenerateGenderedArticles (definiteSingular, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.DefiniteSingularFemenine;
			masculine.Meaning = IndicativeType.DefiniteSingularMasculine;
			neutral.Meaning = IndicativeType.DefiniteSingularNeutral;

			articles.Add (IndicativeType.DefiniteSingularFemenine, femenine);
			articles.Add (IndicativeType.DefiniteSingularMasculine, masculine);
			articles.Add (IndicativeType.DefiniteSingularNeutral, neutral);
		}

		if ((generalProperties & GeneralArticleProperties.HasDefinitePluralArticles) == GeneralArticleProperties.HasDefinitePluralArticles) {

			Morpheme definitePlural = GenerateDerivatedArticle (definite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Plural, getRandomFloat);

			Morpheme femenine, masculine, neutral;
			GenerateGenderedArticles (definitePlural, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.DefinitePluralFemenine;
			masculine.Meaning = IndicativeType.DefinitePluralMasculine;
			neutral.Meaning = IndicativeType.DefinitePluralNeutral;

			articles.Add (IndicativeType.DefinitePluralFemenine, femenine);
			articles.Add (IndicativeType.DefinitePluralMasculine, masculine);
			articles.Add (IndicativeType.DefinitePluralNeutral, neutral);
		}

		if ((generalProperties & GeneralArticleProperties.HasIndefiniteSingularArticles) == GeneralArticleProperties.HasIndefiniteSingularArticles) {

			Morpheme indefiniteSingular = GenerateDerivatedArticle (indefinite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);

			Morpheme femenine, masculine, neutral;
			GenerateGenderedArticles (indefiniteSingular, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.IndefiniteSingularFemenine;
			masculine.Meaning = IndicativeType.IndefiniteSingularMasculine;
			neutral.Meaning = IndicativeType.IndefiniteSingularNeutral;

			articles.Add (IndicativeType.IndefiniteSingularFemenine, femenine);
			articles.Add (IndicativeType.IndefiniteSingularMasculine, masculine);
			articles.Add (IndicativeType.IndefiniteSingularNeutral, neutral);
		}

		if ((generalProperties & GeneralArticleProperties.HasIndefinitePluralArticles) == GeneralArticleProperties.HasIndefinitePluralArticles) {

			Morpheme indefinitePlural = GenerateDerivatedArticle (indefinite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Plural, getRandomFloat);

			Morpheme femenine, masculine, neutral;
			GenerateGenderedArticles (indefinitePlural, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.IndefinitePluralFemenine;
			masculine.Meaning = IndicativeType.IndefinitePluralMasculine;
			neutral.Meaning = IndicativeType.IndefinitePluralNeutral;

			articles.Add (IndicativeType.IndefinitePluralFemenine, femenine);
			articles.Add (IndicativeType.IndefinitePluralMasculine, masculine);
			articles.Add (IndicativeType.IndefinitePluralNeutral, neutral);
		}

		if ((generalProperties & GeneralArticleProperties.HasUncountableArticles) == GeneralArticleProperties.HasUncountableArticles) {

			Morpheme femenine, masculine, neutral;
			GenerateGenderedArticles (uncountable, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.UncountableFemenine;
			masculine.Meaning = IndicativeType.UncountableMasculine;
			neutral.Meaning = IndicativeType.UncountableNeutral;

			articles.Add (IndicativeType.UncountableFemenine, femenine);
			articles.Add (IndicativeType.UncountableMasculine, masculine);
			articles.Add (IndicativeType.UncountableNeutral, neutral);
		}

		return articles;
	}

	public static MorphemeProperties GenerateMorphemeProperties (
		GetRandomFloatDelegate getRandomFloat, 
		bool isPlural = false, 
		bool randomGender = false, 
		bool isFemenine = false, 
		bool isNeutral = true,
		bool isPassive = false) {

		MorphemeProperties properties = MorphemeProperties.None;

		if (isPlural) {
			properties |= MorphemeProperties.Plural;
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
			properties |= MorphemeProperties.Femenine;
		}

		if (isNeutral) {
			properties |= MorphemeProperties.Neutral;
		}

		if (isPassive) {
			properties |= MorphemeProperties.Passive;
		}

		return properties;
	}

	public static string WordPropertiesToString (MorphemeProperties properties) {

		if (properties == MorphemeProperties.None)
			return "None";

		string output = "";

		bool multipleProperties = false;

		if ((properties & MorphemeProperties.Femenine) == MorphemeProperties.Femenine) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsFemenine";
			multipleProperties = true;
		}

		if ((properties & MorphemeProperties.Neutral) == MorphemeProperties.Neutral) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsNeutral";
			multipleProperties = true;
		}

		if ((properties & MorphemeProperties.Plural) == MorphemeProperties.Plural) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsPlural";
			multipleProperties = true;
		}

//		if ((properties & MorphemeProperties.Irregular) == MorphemeProperties.Irregular) {
//
//			if (multipleProperties) {
//				output += " | ";
//			}
//
//			output += "IsIrregular";
//			multipleProperties = true;
//		}

		if ((properties & MorphemeProperties.Uncountable) == MorphemeProperties.Uncountable) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsUncountable";
			multipleProperties = true;
		}

		if ((properties & MorphemeProperties.Uncountable) == MorphemeProperties.FirstPerson) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsFirstPerson";
			multipleProperties = true;
		}

		if ((properties & MorphemeProperties.Uncountable) == MorphemeProperties.SecondPerson) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsSecondPerson";
			multipleProperties = true;
		}

		if ((properties & MorphemeProperties.Uncountable) == MorphemeProperties.ThirdPerson) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsThirdPerson";
			multipleProperties = true;
		}

		return output;
	}

	public void GenerateArticleProperties (GetRandomFloatDelegate getRandomFloat) {

		if (getRandomFloat () < 0.40f) {

			_articleProperties |= GeneralArticleProperties.HasDefiniteSingularArticles;
		}

		if (getRandomFloat () < 0.30f) {

			_articleProperties |= GeneralArticleProperties.HasDefinitePluralArticles;
		}

		if (getRandomFloat () < 0.20f) {

			_articleProperties |= GeneralArticleProperties.HasIndefiniteSingularArticles;
		}

		if (getRandomFloat () < 0.15f) {

			_articleProperties |= GeneralArticleProperties.HasIndefinitePluralArticles;
		}

		if (getRandomFloat () < 0.10f) {

			_articleProperties |= GeneralArticleProperties.HasUncountableArticles;
		}
	}

	public void GenerateNounIndicativeProperties (GetRandomFloatDelegate getRandomFloat) {

		if (getRandomFloat () < 0.25f) {

			_nounIndicativeProperties |= GeneralNounIndicativeProperties.HasDefiniteIndicative;
		}

		if (getRandomFloat () < 0.20f) {

			_nounIndicativeProperties |= GeneralNounIndicativeProperties.HasIndefiniteIndicative;
		}

		if (getRandomFloat () < 0.15f) {

			_nounIndicativeProperties |= GeneralNounIndicativeProperties.HasUncountableIndicative;
		}

		if (getRandomFloat () < 0.25f) {

			_nounIndicativeProperties |= GeneralNounIndicativeProperties.HasMasculineIndicative;
		}

		if (getRandomFloat () < 0.25f) {

			_nounIndicativeProperties |= GeneralNounIndicativeProperties.HasNeutralIndicative;
		}

		if (getRandomFloat () < 0.25f) {

			_nounIndicativeProperties |= GeneralNounIndicativeProperties.HasFemenineIndicative;
		}

		if (getRandomFloat () < 0.20f) {

			_nounIndicativeProperties |= GeneralNounIndicativeProperties.HasSingularIndicative;
		}

		if (getRandomFloat () < 0.30f) {

			_nounIndicativeProperties |= GeneralNounIndicativeProperties.HasPluralIndicative;
		}
	}

	public void GenerateVerbIndicativeProperties (GetRandomFloatDelegate getRandomFloat) {

		/// Person indicatives

		if (getRandomFloat () < 0.20f) {

			_verbIndicativeProperties |= GeneralVerbIndicativeProperties.HasFirstPersonIndicative;
		}

		if (getRandomFloat () < 0.20f) {

			_verbIndicativeProperties |= GeneralVerbIndicativeProperties.HasSecondPersonIndicative;
		}

		if (getRandomFloat () < 0.20f) {

			_verbIndicativeProperties |= GeneralVerbIndicativeProperties.HasThirdPersonIndicative;
		}

		/// Count indicatives

		if (getRandomFloat () < 0.30f) {

			_verbIndicativeProperties |= GeneralVerbIndicativeProperties.HasSingularIndicative;
		}

		if (getRandomFloat () < 0.30f) {

			_verbIndicativeProperties |= GeneralVerbIndicativeProperties.HasPluralIndicative;
		}

		/// Active nominalization indicative

		if (getRandomFloat () < 0.75f) {

			_verbIndicativeProperties |= GeneralVerbIndicativeProperties.HasActiveNominalizationIndicative;
		}

		/// Passive nominalization indicative

		if (getRandomFloat () < 0.60f) {

			_verbIndicativeProperties |= GeneralVerbIndicativeProperties.HasPassiveNominalizationIndicative;
		}

		/// Tense indicatives

		if (getRandomFloat () < 0.40f) {

			_verbIndicativeProperties |= GeneralVerbIndicativeProperties.HasPresentTenseIndicative;
		}

		if (getRandomFloat () < 0.35f) {

			_verbIndicativeProperties |= GeneralVerbIndicativeProperties.HasPastTenseIndicative;
		}

		if (getRandomFloat () < 0.35f) {

			_verbIndicativeProperties |= GeneralVerbIndicativeProperties.HasFutureTenseIndicative;
		}

		if (getRandomFloat () < 0.30f) {

			_verbIndicativeProperties |= GeneralVerbIndicativeProperties.HasInfinitiveTenseIndicative;
		}
	}

	public void GenerateArticleAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		ArticleAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat, 0.4f, 0.3f, 0.1f, 0.7f, 0.3f);
	}

	public void GenerateArticleSyllables (GetRandomFloatDelegate getRandomFloat) {
		
		ArticleSyllables.OnsetChance = 0.5f;
		ArticleSyllables.NucleusChance = 1.0f;
		ArticleSyllables.CodaChance = 0.5f;

		DerivativeArticleStartSyllables.OnsetChance = 0.5f;
		DerivativeArticleStartSyllables.NucleusChance = 1.0f;
		DerivativeArticleStartSyllables.CodaChance = 0.5f;

		DerivativeArticleNextSyllables.OnsetChance = 0.5f;
		DerivativeArticleNextSyllables.NucleusChance = 1.0f;
		DerivativeArticleNextSyllables.CodaChance = 0.5f;
	}

	public void GenerateAllArticles (GetRandomFloatDelegate getRandomFloat) {

		_articles = GenerateArticles (ArticleSyllables, DerivativeArticleStartSyllables, DerivativeArticleNextSyllables, _articleProperties, getRandomFloat);

		Articles = new List<Morpheme> (_articles.Count);

		foreach (KeyValuePair<string, Morpheme> pair in _articles) {
		
			Articles.Add (pair.Value);
		}
	}

	public void GenerateNounIndicativeAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		NounIndicativeAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat, 0.7f, 0.5f, 0.2f, 0.9f, 0.1f);
	}

	public void GenerateNounIndicativeSyllables (GetRandomFloatDelegate getRandomFloat) {

		NounIndicativeSyllables.OnsetChance = 0.5f;
		NounIndicativeSyllables.NucleusChance = 1.0f;
		NounIndicativeSyllables.CodaChance = 0.5f;

		DerivativeNounIndicativeStartSyllables.OnsetChance = 0.5f;
		DerivativeNounIndicativeStartSyllables.NucleusChance = 1.0f;
		DerivativeNounIndicativeStartSyllables.CodaChance = 0.5f;

		DerivativeNounIndicativeNextSyllables.OnsetChance = 0.5f;
		DerivativeNounIndicativeNextSyllables.NucleusChance = 1.0f;
		DerivativeNounIndicativeNextSyllables.CodaChance = 0.5f;
	}

	public void GenerateVerbIndicativeAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		VerbIndicativeAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat, 0.7f, 0.5f, 0.2f, 0.9f, 0.1f);
	}

	public void GenerateVerbIndicativeSyllables (GetRandomFloatDelegate getRandomFloat) {

		VerbIndicativeSyllables.OnsetChance = 0.5f;
		VerbIndicativeSyllables.NucleusChance = 1.0f;
		VerbIndicativeSyllables.CodaChance = 0.5f;

		DerivativeVerbIndicativeStartSyllables.OnsetChance = 0.5f;
		DerivativeVerbIndicativeStartSyllables.NucleusChance = 1.0f;
		DerivativeVerbIndicativeStartSyllables.CodaChance = 0.5f;

		DerivativeVerbIndicativeNextSyllables.OnsetChance = 0.5f;
		DerivativeVerbIndicativeNextSyllables.NucleusChance = 1.0f;
		DerivativeVerbIndicativeNextSyllables.CodaChance = 0.5f;
	}

	public static Morpheme GenerateIndicative (
		SyllableSet syllables,  
		GetRandomFloatDelegate getRandomFloat) {

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = GenerateMorpheme (syllables, getRandomFloat);
		morpheme.Properties = MorphemeProperties.None;
		morpheme.Type = WordType.Indicative;

		return morpheme;
	}

	public static Morpheme GenerateIndicative (
		SyllableSet syllables, 
		MorphemeProperties properties, 
		GetRandomFloatDelegate getRandomFloat) {

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = GenerateMorpheme (syllables, getRandomFloat);
		morpheme.Properties = properties;
		morpheme.Type = WordType.Indicative;

		return morpheme;
	}

	public static Morpheme GenerateNullWord (WordType type, MorphemeProperties properties = MorphemeProperties.None) {

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = string.Empty;
		morpheme.Properties = properties;
		morpheme.Type = type;
		morpheme.Meaning = string.Empty;

		return morpheme;
	}

	public static Morpheme CopyMorpheme (Morpheme sourceMorpheme) {

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = sourceMorpheme.Value;
		morpheme.Properties = sourceMorpheme.Properties;
		morpheme.Type = sourceMorpheme.Type;
		morpheme.Meaning = sourceMorpheme.Meaning;

		return morpheme;
	}

	public static Morpheme GenerateDerivatedIndicative (
		Morpheme rootIndicative, 
		SyllableSet syllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		GetRandomFloatDelegate getRandomFloat) {

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = GenerateDerivatedWord (rootIndicative.Value, 0.0f, 0.5f, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat);
		morpheme.Properties = rootIndicative.Properties;
		morpheme.Type = WordType.Indicative;

		return morpheme;
	}

	public static Morpheme GenerateDerivatedIndicative (
		Morpheme rootIndicative, 
		SyllableSet syllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		MorphemeProperties properties, 
		GetRandomFloatDelegate getRandomFloat) {

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = GenerateDerivatedWord (rootIndicative.Value, 0.0f, 0.5f, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat);
		morpheme.Properties = rootIndicative.Properties | properties;
		morpheme.Type = WordType.Indicative;

		return morpheme;
	}

	public static void GenerateGenderedIndicatives (
		Morpheme root,
		SyllableSet syllables,  
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		GeneralNounIndicativeProperties indicativeProperties, 
		GetRandomFloatDelegate getRandomFloat, 
		out Morpheme masculine, 
		out Morpheme femenine,
		out Morpheme neutral) {

		if ((indicativeProperties & GeneralNounIndicativeProperties.HasMasculineIndicative) == GeneralNounIndicativeProperties.HasMasculineIndicative)
			masculine = GenerateDerivatedIndicative (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
		else
			masculine = CopyMorpheme (root);

		if ((indicativeProperties & GeneralNounIndicativeProperties.HasFemenineIndicative) == GeneralNounIndicativeProperties.HasFemenineIndicative)
			femenine = GenerateDerivatedIndicative (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
		else
			femenine = CopyMorpheme (root);

		if ((indicativeProperties & GeneralNounIndicativeProperties.HasNeutralIndicative) == GeneralNounIndicativeProperties.HasNeutralIndicative)
			neutral = GenerateDerivatedIndicative (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
		else
			neutral = CopyMorpheme (root);

		femenine.Properties |= MorphemeProperties.Femenine;
		neutral.Properties |= MorphemeProperties.Neutral;
	}

	public static Dictionary<string, Morpheme> GenerateNounIndicatives (
		SyllableSet syllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		GeneralNounIndicativeProperties indicativeProperties, 
		GetRandomFloatDelegate getRandomFloat) {

		Dictionary<string, Morpheme> indicatives = new Dictionary<string, Morpheme> ();

		Morpheme definite;
		if ((indicativeProperties & GeneralNounIndicativeProperties.HasDefiniteIndicative) == GeneralNounIndicativeProperties.HasDefiniteIndicative)
			definite = GenerateIndicative (syllables, MorphemeProperties.None, getRandomFloat);
		else
			definite = GenerateNullWord (WordType.Indicative);

		Morpheme indefinite;
		if ((indicativeProperties & GeneralNounIndicativeProperties.HasIndefiniteIndicative) == GeneralNounIndicativeProperties.HasIndefiniteIndicative)
			indefinite = GenerateIndicative (syllables, MorphemeProperties.Indefinite, getRandomFloat);
		else
			indefinite = GenerateNullWord (WordType.Indicative, MorphemeProperties.Indefinite);

		Morpheme uncountable;
		if ((indicativeProperties & GeneralNounIndicativeProperties.HasUncountableIndicative) == GeneralNounIndicativeProperties.HasUncountableIndicative)
			uncountable = GenerateIndicative (syllables, MorphemeProperties.Uncountable, getRandomFloat);
		else
			uncountable = GenerateNullWord (WordType.Indicative, MorphemeProperties.Uncountable);

		///

		Morpheme definiteSingular;
		if ((indicativeProperties & GeneralNounIndicativeProperties.HasSingularIndicative) == GeneralNounIndicativeProperties.HasSingularIndicative)
			definiteSingular = GenerateDerivatedIndicative (definite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
		else
			definiteSingular = CopyMorpheme (definite);

		Morpheme femenine, masculine, neutral;
		GenerateGenderedIndicatives (definiteSingular, syllables, derivativeStartSyllables, derivativeNextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.DefiniteSingularFemenine;
		masculine.Meaning = IndicativeType.DefiniteSingularMasculine;
		neutral.Meaning = IndicativeType.DefiniteSingularNeutral;

		indicatives.Add (IndicativeType.DefiniteSingularFemenine, femenine);
		indicatives.Add (IndicativeType.DefiniteSingularMasculine, masculine);
		indicatives.Add (IndicativeType.DefiniteSingularNeutral, neutral);

		///

		Morpheme definitePlural;
		if ((indicativeProperties & GeneralNounIndicativeProperties.HasPluralIndicative) == GeneralNounIndicativeProperties.HasPluralIndicative)
			definitePlural = GenerateDerivatedIndicative (definite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Plural, getRandomFloat);
		else {
			definitePlural = CopyMorpheme (definite);
			definitePlural.Properties |= MorphemeProperties.Plural;
		}

		GenerateGenderedIndicatives (definitePlural, syllables, derivativeStartSyllables, derivativeNextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.DefinitePluralFemenine;
		masculine.Meaning = IndicativeType.DefinitePluralMasculine;
		neutral.Meaning = IndicativeType.DefinitePluralNeutral;

		indicatives.Add (IndicativeType.DefinitePluralFemenine, femenine);
		indicatives.Add (IndicativeType.DefinitePluralMasculine, masculine);
		indicatives.Add (IndicativeType.DefinitePluralNeutral, neutral);

		///

		Morpheme indefiniteSingular;
		if ((indicativeProperties & GeneralNounIndicativeProperties.HasSingularIndicative) == GeneralNounIndicativeProperties.HasSingularIndicative)
			indefiniteSingular = GenerateDerivatedIndicative (indefinite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
		else
			indefiniteSingular = CopyMorpheme (indefinite);

		GenerateGenderedIndicatives (indefiniteSingular, syllables, derivativeStartSyllables, derivativeNextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.IndefiniteSingularFemenine;
		masculine.Meaning = IndicativeType.IndefiniteSingularMasculine;
		neutral.Meaning = IndicativeType.IndefiniteSingularNeutral;

		indicatives.Add (IndicativeType.IndefiniteSingularFemenine, femenine);
		indicatives.Add (IndicativeType.IndefiniteSingularMasculine, masculine);
		indicatives.Add (IndicativeType.IndefiniteSingularNeutral, neutral);

		///

		Morpheme indefinitePlural;
		if ((indicativeProperties & GeneralNounIndicativeProperties.HasPluralIndicative) == GeneralNounIndicativeProperties.HasPluralIndicative)
			indefinitePlural = GenerateDerivatedIndicative (indefinite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Plural, getRandomFloat);
		else {
			indefinitePlural = CopyMorpheme (indefinite);
			indefinitePlural.Properties |= MorphemeProperties.Plural;
		}

		GenerateGenderedIndicatives (indefinitePlural, syllables, derivativeStartSyllables, derivativeNextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.IndefinitePluralFemenine;
		masculine.Meaning = IndicativeType.IndefinitePluralMasculine;
		neutral.Meaning = IndicativeType.IndefinitePluralNeutral;

		indicatives.Add (IndicativeType.IndefinitePluralFemenine, femenine);
		indicatives.Add (IndicativeType.IndefinitePluralMasculine, masculine);
		indicatives.Add (IndicativeType.IndefinitePluralNeutral, neutral);

		///

		GenerateGenderedIndicatives (uncountable, syllables, derivativeStartSyllables, derivativeNextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.UncountableFemenine;
		masculine.Meaning = IndicativeType.UncountableMasculine;
		neutral.Meaning = IndicativeType.UncountableNeutral;

		indicatives.Add (IndicativeType.UncountableFemenine, femenine);
		indicatives.Add (IndicativeType.UncountableMasculine, masculine);
		indicatives.Add (IndicativeType.UncountableNeutral, neutral);

		return indicatives;
	}

	public void GenerateAllNounIndicatives (GetRandomFloatDelegate getRandomFloat) {

		_nounIndicatives = GenerateNounIndicatives (NounIndicativeSyllables, DerivativeNounIndicativeStartSyllables, DerivativeArticleNextSyllables, _nounIndicativeProperties, getRandomFloat);

		NounIndicatives = new List<Morpheme> (_nounIndicatives.Count);

		foreach (KeyValuePair<string, Morpheme> pair in _nounIndicatives) {

			NounIndicatives.Add (pair.Value);
		}
	}

	private static void GenerateVerbPersonIndicatives (
		Dictionary<string, Morpheme> indicatives,
		Morpheme root,
		string rootIndicativeType,
		SyllableSet syllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		GeneralVerbIndicativeProperties indicativeProperties,
		GetRandomFloatDelegate getRandomFloat) {

		/// First person

		Morpheme firstPerson;
		if ((indicativeProperties & GeneralVerbIndicativeProperties.HasFirstPersonIndicative) == GeneralVerbIndicativeProperties.HasFirstPersonIndicative)
			firstPerson = GenerateDerivatedIndicative (root, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat);
		else
			firstPerson = CopyMorpheme (root);

		firstPerson.Properties |= MorphemeProperties.FirstPerson;
		firstPerson.Meaning = rootIndicativeType + IndicativeType.FirstPerson;

		indicatives.Add (firstPerson.Meaning, firstPerson);

		/// Second person

		Morpheme secondPerson;
		if ((indicativeProperties & GeneralVerbIndicativeProperties.HasSecondPersonIndicative) == GeneralVerbIndicativeProperties.HasSecondPersonIndicative)
			secondPerson = GenerateDerivatedIndicative (root, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat);
		else
			secondPerson = CopyMorpheme (root);

		secondPerson.Properties |= MorphemeProperties.SecondPerson;
		secondPerson.Meaning = rootIndicativeType + IndicativeType.SecondPerson;

		indicatives.Add (secondPerson.Meaning, secondPerson);

		/// Third person

		Morpheme thirdPerson;
		if ((indicativeProperties & GeneralVerbIndicativeProperties.HasThirdPersonIndicative) == GeneralVerbIndicativeProperties.HasThirdPersonIndicative)
			thirdPerson = GenerateDerivatedIndicative (root, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat);
		else
			thirdPerson = CopyMorpheme (root);

		thirdPerson.Properties |= MorphemeProperties.ThirdPerson;
		thirdPerson.Meaning = rootIndicativeType + IndicativeType.ThirdPerson;

		indicatives.Add (thirdPerson.Meaning, thirdPerson);
	}

	private static void GenerateVerbCountIndicatives (
		Dictionary<string, Morpheme> indicatives,
		Morpheme root,
		string rootIndicativeType,
		SyllableSet syllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		GeneralVerbIndicativeProperties indicativeProperties, 
		GetRandomFloatDelegate getRandomFloat) {

		Morpheme singular;
		if ((indicativeProperties & GeneralVerbIndicativeProperties.HasSingularIndicative) == GeneralVerbIndicativeProperties.HasSingularIndicative)
			singular = GenerateDerivatedIndicative (root, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat);
		else
			singular = CopyMorpheme (root);

		GenerateVerbPersonIndicatives (
			indicatives, 
			singular, 
			rootIndicativeType + IndicativeType.Singular, 
			syllables, 
			derivativeStartSyllables, 
			derivativeNextSyllables, 
			indicativeProperties, 
			getRandomFloat);

		Morpheme plural;
		if ((indicativeProperties & GeneralVerbIndicativeProperties.HasSingularIndicative) == GeneralVerbIndicativeProperties.HasSingularIndicative)
			plural = GenerateDerivatedIndicative (root, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat);
		else
			plural = CopyMorpheme (root);

		plural.Properties |= MorphemeProperties.Plural;

		GenerateVerbPersonIndicatives (
			indicatives, 
			plural, 
			rootIndicativeType + IndicativeType.Plural, 
			syllables, 
			derivativeStartSyllables, 
			derivativeNextSyllables, 
			indicativeProperties, 
			getRandomFloat);
	}

	public static Dictionary<string, Morpheme> GenerateVerbIndicatives (
		SyllableSet syllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		GeneralVerbIndicativeProperties indicativeProperties, 
		GetRandomFloatDelegate getRandomFloat) {

		Dictionary<string, Morpheme> indicatives = new Dictionary<string, Morpheme> ();

		Morpheme activeNominalization;
		if ((indicativeProperties & GeneralVerbIndicativeProperties.HasActiveNominalizationIndicative) == GeneralVerbIndicativeProperties.HasActiveNominalizationIndicative)
			activeNominalization = GenerateIndicative (syllables, getRandomFloat);
		else
			activeNominalization = GenerateNullWord (WordType.Indicative);

		activeNominalization.Meaning = IndicativeType.ActiveNominalization;

		indicatives.Add (activeNominalization.Meaning, activeNominalization);

		///

		Morpheme passiveNominalization;
		if ((indicativeProperties & GeneralVerbIndicativeProperties.HasPassiveNominalizationIndicative) == GeneralVerbIndicativeProperties.HasPassiveNominalizationIndicative)
			passiveNominalization = GenerateIndicative (syllables, getRandomFloat);
		else
			passiveNominalization = GenerateNullWord (WordType.Indicative);

		passiveNominalization.Properties |= MorphemeProperties.Passive;
		passiveNominalization.Meaning = IndicativeType.PassiveNominalization;

		indicatives.Add (passiveNominalization.Meaning, passiveNominalization);

		///

		Morpheme nullTense = GenerateNullWord (WordType.Indicative);

		GenerateVerbCountIndicatives (indicatives, nullTense, IndicativeType.NullTense, syllables, derivativeStartSyllables, derivativeNextSyllables, indicativeProperties, getRandomFloat);

		///

		Morpheme infinitiveTense;
		if ((indicativeProperties & GeneralVerbIndicativeProperties.HasInfinitiveTenseIndicative) == GeneralVerbIndicativeProperties.HasInfinitiveTenseIndicative)
			infinitiveTense = GenerateIndicative (syllables, getRandomFloat);
		else
			infinitiveTense = GenerateNullWord (WordType.Indicative);

		infinitiveTense.Meaning = IndicativeType.InfinitiveTense;

		indicatives.Add (infinitiveTense.Meaning, infinitiveTense);

		///

		Morpheme presentTense;
		if ((indicativeProperties & GeneralVerbIndicativeProperties.HasPresentTenseIndicative) == GeneralVerbIndicativeProperties.HasPresentTenseIndicative)
			presentTense = GenerateIndicative (syllables, getRandomFloat);
		else
			presentTense = GenerateNullWord (WordType.Indicative);

		GenerateVerbCountIndicatives (indicatives, presentTense, IndicativeType.PresentTense, syllables, derivativeStartSyllables, derivativeNextSyllables, indicativeProperties, getRandomFloat);

		///

		Morpheme pastTense;
		if ((indicativeProperties & GeneralVerbIndicativeProperties.HasPastTenseIndicative) == GeneralVerbIndicativeProperties.HasPastTenseIndicative)
			pastTense = GenerateIndicative (syllables, getRandomFloat);
		else
			pastTense = GenerateNullWord (WordType.Indicative);

		GenerateVerbCountIndicatives (indicatives, pastTense, IndicativeType.PastTense, syllables, derivativeStartSyllables, derivativeNextSyllables, indicativeProperties, getRandomFloat);

		///

		Morpheme futureTense;
		if ((indicativeProperties & GeneralVerbIndicativeProperties.HasFutureTenseIndicative) == GeneralVerbIndicativeProperties.HasFutureTenseIndicative)
			futureTense = GenerateIndicative (syllables, getRandomFloat);
		else
			futureTense = GenerateNullWord (WordType.Indicative);

		GenerateVerbCountIndicatives (indicatives, futureTense, IndicativeType.FutureTense, syllables, derivativeStartSyllables, derivativeNextSyllables, indicativeProperties, getRandomFloat);

		return indicatives;
	}

	public void GenerateAllVerbIndicatives (GetRandomFloatDelegate getRandomFloat) {

		_verbIndicatives = GenerateVerbIndicatives (VerbIndicativeSyllables, DerivativeVerbIndicativeStartSyllables, DerivativeVerbIndicativeNextSyllables, _verbIndicativeProperties, getRandomFloat);

		VerbIndicatives = new List<Morpheme> (_verbIndicatives.Count);

		foreach (KeyValuePair<string, Morpheme> pair in _verbIndicatives) {

			VerbIndicatives.Add (pair.Value);
		}
	}

	public void GenerateAdpositionAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		AdpositionAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat, 0.5f, 0.3f, 0.1f, 0.5f, 0.2f);
	}

	public void GenerateAdpositionSyllables (GetRandomFloatDelegate getRandomFloat) {

		AdpositionStartSyllables.OnsetChance = 0.5f;
		AdpositionStartSyllables.NucleusChance = 1.0f;
		AdpositionStartSyllables.CodaChance = 0.5f;

		AdpositionNextSyllables.OnsetChance = 0.5f;
		AdpositionNextSyllables.NucleusChance = 1.0f;
		AdpositionNextSyllables.CodaChance = 0.5f;
	}

	public Morpheme GenerateAdposition (string relation, GetRandomFloatDelegate getRandomFloat) {

		if (_adpositions.ContainsKey (relation)) {

			return _adpositions [relation];
		}

		string value = GenerateMorpheme (AdpositionStartSyllables, AdpositionNextSyllables, 0.2f, getRandomFloat);

		while (_existingAdpositionMorphemeValues.Contains (value)) {
		
			value = GenerateMorpheme (AdpositionStartSyllables, AdpositionNextSyllables, 0.2f, getRandomFloat);
		}

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = value;
		morpheme.Properties = MorphemeProperties.None;
		morpheme.Type = WordType.Adposition;
		morpheme.Meaning = relation;

		_adpositions.Add (relation, morpheme);
		_existingAdpositionMorphemeValues.Add (morpheme.Value);

		Adpositions.Add (morpheme);

		return morpheme;
	}

	public void GenerateAdjectiveAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		AdjectiveAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat, 0.5f, 0.3f, 0.1f, 0.5f, 0.2f);
	}

	public void GenerateAdjectiveSyllables (GetRandomFloatDelegate getRandomFloat) {

		AdjectiveStartSyllables.OnsetChance = 0.5f;
		AdjectiveStartSyllables.NucleusChance = 1.0f;
		AdjectiveStartSyllables.CodaChance = 0.5f;

		AdjectiveNextSyllables.OnsetChance = 0.5f;
		AdjectiveNextSyllables.NucleusChance = 1.0f;
		AdjectiveNextSyllables.CodaChance = 0.5f;
	}

	public Morpheme GenerateAdjective (string meaning, GetRandomFloatDelegate getRandomFloat) {

		if (_adjectives.ContainsKey (meaning)) {
		
			return _adjectives [meaning];
		}

		string value = GenerateMorpheme (AdjectiveStartSyllables, AdjectiveNextSyllables, 0.25f, getRandomFloat);

		while (_existingAdjectiveMorphemeValues.Contains (value)) {

			value = GenerateMorpheme (AdjectiveStartSyllables, AdjectiveNextSyllables, 0.25f, getRandomFloat);
		}

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = value;
		morpheme.Properties = MorphemeProperties.None;
		morpheme.Type = WordType.Adjective;
		morpheme.Meaning = meaning;

		_adjectives.Add (meaning, morpheme);
		_existingAdjectiveMorphemeValues.Add (morpheme.Value);

		Adjectives.Add (morpheme);

		return morpheme;
	}

	public void GenerateNounAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		NounAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat, 0.5f, 0.3f, 0.1f, 0.5f, 0.2f);
	}

	public void GenerateNounSyllables (GetRandomFloatDelegate getRandomFloat) {

		NounStartSyllables.OnsetChance = 0.5f;
		NounStartSyllables.NucleusChance = 1.0f;
		NounStartSyllables.CodaChance = 0.5f;

		NounNextSyllables.OnsetChance = 0.5f;
		NounNextSyllables.NucleusChance = 1.0f;
		NounNextSyllables.CodaChance = 0.5f;
	}

	public Morpheme GenerateNoun (string meaning, GetRandomFloatDelegate getRandomFloat, bool isPlural, bool randomGender, bool isFemenine = false, bool isNeutral = false) {

		return GenerateNoun (meaning, GenerateMorphemeProperties (getRandomFloat, isPlural, randomGender, isFemenine, isNeutral), getRandomFloat);
	}

	public Morpheme GenerateNoun (string meaning, MorphemeProperties properties, GetRandomFloatDelegate getRandomFloat) {

		if (_nouns.ContainsKey (meaning)) {
		
			return _nouns [meaning];
		}

		string value;

		while (true) {
		
			value = GenerateMorpheme (NounStartSyllables, NounNextSyllables, 0.3f, getRandomFloat);

			if (!_existingNounMorphemeValues.ContainsKey (value))
				break;

			float tolerance = _existingNounMorphemeValues [value];

			if (getRandomFloat () < tolerance) {
			
				_existingNounMorphemeValues [value] = tolerance * _homographToleranceDecayFactor;
				break;
			}
		}

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = value;
		morpheme.Properties = properties;
		morpheme.Type = WordType.Noun;
		morpheme.Meaning = meaning;

		_nouns.Add (meaning, morpheme);

		if (!_existingNounMorphemeValues.ContainsKey (value)) {

			_existingNounMorphemeValues.Add (value, _initialHomographTolerance);
		}

		Nouns.Add (morpheme);

		return morpheme;
	}

	public void GenerateVerbSyllables (GetRandomFloatDelegate getRandomFloat) {

		VerbStartSyllables.OnsetChance = 0.5f;
		VerbStartSyllables.NucleusChance = 1.0f;
		VerbStartSyllables.CodaChance = 0.5f;

		VerbNextSyllables.OnsetChance = 0.5f;
		VerbNextSyllables.NucleusChance = 1.0f;
		VerbNextSyllables.CodaChance = 0.5f;
	}

	public Morpheme GenerateVerb (string meaning, GetRandomFloatDelegate getRandomFloat) {

		if (_verbs.ContainsKey (meaning)) {

			return _verbs [meaning];
		}

		string value = GenerateMorpheme (VerbStartSyllables, VerbNextSyllables, 0.25f, getRandomFloat);

		while (_existingVerbMorphemeValues.Contains (value)) {

			value = GenerateMorpheme (VerbStartSyllables, VerbNextSyllables, 0.25f, getRandomFloat);
		}

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = value;
		morpheme.Properties = MorphemeProperties.None;
		morpheme.Type = WordType.Verb;
		morpheme.Meaning = meaning;

		_verbs.Add (meaning, morpheme);
		_existingVerbMorphemeValues.Add (morpheme.Value);

		Verbs.Add (morpheme);

		return morpheme;
	}

	public Morpheme GenerateNominalizedVerb (string meaning, Morpheme verb, string tense, GetRandomFloatDelegate getRandomFloat, bool isPlural, bool randomGender, bool isFemenine = false, bool isNeutral = false, bool isPassive = false) {

		return GenerateNominalizedVerb (meaning, verb, tense, GenerateMorphemeProperties (getRandomFloat, isPlural, randomGender, isFemenine, isNeutral, isPassive), getRandomFloat);
	}

	public Morpheme GenerateNominalizedVerb (string meaning, Morpheme verb, string tense, MorphemeProperties properties, GetRandomFloatDelegate getRandomFloat) {

		if (_nouns.ContainsKey (meaning)) {

			return _nouns [meaning];
		}

		string value = verb.Value;

		PhraseProperties phraseProperties = MapMorphemeToPhraseProperties (properties);
		phraseProperties |= PhraseProperties.ThirdPerson;

		Morpheme verbIndicative = GetAppropiateVerbIndicative (phraseProperties, tense);

		value = AppendAdjunction (value, verbIndicative.Value, VerbIndicativeAdjunctionProperties);

		Morpheme verbNormalizationIndicative = GetAppropiateVerbNominalizationIndicative (phraseProperties);

		value = AppendAdjunction (value, verbNormalizationIndicative.Value, VerbIndicativeAdjunctionProperties);

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = value;
		morpheme.Properties = properties;
		morpheme.Type = WordType.Noun;
		morpheme.Meaning = meaning;

		_nouns.Add (meaning, morpheme);

		if (!_existingNounMorphemeValues.ContainsKey (value)) {

			_existingNounMorphemeValues.Add (value, _initialHomographTolerance);
		}

		Nouns.Add (morpheme);

		return morpheme;
	}

	public Morpheme GetAppropiateArticle (PhraseProperties phraseProperties) {

		Morpheme article = null;

		if ((phraseProperties & PhraseProperties.Uncountable) == PhraseProperties.Uncountable) {
			
			if ((_articleProperties & GeneralArticleProperties.HasUncountableArticles) == GeneralArticleProperties.HasUncountableArticles) {
				if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

					article = _articles [IndicativeType.UncountableFemenine];

				} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

					article = _articles [IndicativeType.UncountableNeutral];

				} else {
					article = _articles [IndicativeType.UncountableMasculine];
				}
			}

		} else if ((phraseProperties & PhraseProperties.Indefinite) == PhraseProperties.Indefinite) {
				
			if ((phraseProperties & PhraseProperties.Plural) == PhraseProperties.Plural) {

				if ((_articleProperties & GeneralArticleProperties.HasIndefinitePluralArticles) == GeneralArticleProperties.HasIndefinitePluralArticles) {
					if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

						article = _articles [IndicativeType.IndefinitePluralFemenine];

					} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

						article = _articles [IndicativeType.IndefinitePluralNeutral];

					} else {
						article = _articles [IndicativeType.IndefinitePluralMasculine];
					}
				}
			} else {
				if ((_articleProperties & GeneralArticleProperties.HasIndefiniteSingularArticles) == GeneralArticleProperties.HasIndefiniteSingularArticles) {
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

				if ((_articleProperties & GeneralArticleProperties.HasDefinitePluralArticles) == GeneralArticleProperties.HasDefinitePluralArticles) {
					if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

						article = _articles [IndicativeType.DefinitePluralFemenine];

					} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

						article = _articles [IndicativeType.DefinitePluralNeutral];

					} else {
						article = _articles [IndicativeType.DefinitePluralMasculine];
					}
				}
			} else {
				if ((_articleProperties & GeneralArticleProperties.HasDefiniteSingularArticles) == GeneralArticleProperties.HasDefiniteSingularArticles) {
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

	private Phrase BuildAdpositionalPhrase (string relation, Phrase complementPhrase, GetRandomFloatDelegate getRandomFloat) {

		Phrase phrase = new Phrase ();

		Morpheme adposition = GenerateAdposition (relation, getRandomFloat);

		string meaning = relation + " " + complementPhrase.Meaning;

		string text = complementPhrase.Text;

		if ((AdpositionAdjunctionProperties & AdjunctionProperties.GoesAfter) == AdjunctionProperties.GoesAfter) {

			if ((AdpositionAdjunctionProperties & AdjunctionProperties.IsAffixed) == AdjunctionProperties.IsAffixed) {

				if ((AdpositionAdjunctionProperties & AdjunctionProperties.IsLinkedWithDash) == AdjunctionProperties.IsLinkedWithDash) {

					text += "-" + adposition.Value;
				} else {

					text = Affix (text, adposition.Value);
				}
			} else {

				text += " " + adposition.Value;
			}
		} else {
			if ((AdpositionAdjunctionProperties & AdjunctionProperties.IsAffixed) == AdjunctionProperties.IsAffixed) {

				if ((AdpositionAdjunctionProperties & AdjunctionProperties.IsLinkedWithDash) == AdjunctionProperties.IsLinkedWithDash) {

					text = adposition.Value + "-" + text;
				} else {

					text = Affix (adposition.Value, text);
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

	public Morpheme GetAppropiateNounIndicative (PhraseProperties phraseProperties) {

		Morpheme indicative = null;

		if ((phraseProperties & PhraseProperties.Uncountable) == PhraseProperties.Uncountable) {

			if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

				indicative = _nounIndicatives [IndicativeType.UncountableFemenine];

			} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

				indicative = _nounIndicatives [IndicativeType.UncountableNeutral];

			} else {

				indicative = _nounIndicatives [IndicativeType.UncountableMasculine];

			}
		} else if ((phraseProperties & PhraseProperties.Indefinite) == PhraseProperties.Indefinite) {
			if ((phraseProperties & PhraseProperties.Plural) == PhraseProperties.Plural) {

				if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

					indicative = _nounIndicatives [IndicativeType.IndefinitePluralFemenine];

				} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

					indicative = _nounIndicatives [IndicativeType.IndefinitePluralNeutral];

				} else {

					indicative = _nounIndicatives [IndicativeType.IndefinitePluralMasculine];

				}
			} else {

				if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

					indicative = _nounIndicatives [IndicativeType.IndefiniteSingularFemenine];

				} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

					indicative = _nounIndicatives [IndicativeType.IndefiniteSingularNeutral];

				} else {

					indicative = _nounIndicatives [IndicativeType.IndefiniteSingularMasculine];
				}
			}
		} else {
			if ((phraseProperties & PhraseProperties.Plural) == PhraseProperties.Plural) {

				if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

					indicative = _nounIndicatives [IndicativeType.DefinitePluralFemenine];

				} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

					indicative = _nounIndicatives [IndicativeType.DefinitePluralNeutral];

				} else {

					indicative = _nounIndicatives [IndicativeType.DefinitePluralMasculine];

				}
			} else {

				if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

					indicative = _nounIndicatives [IndicativeType.DefiniteSingularFemenine];

				} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

					indicative = _nounIndicatives [IndicativeType.DefiniteSingularNeutral];

				} else {

					indicative = _nounIndicatives [IndicativeType.DefiniteSingularMasculine];
				}
			}
		}

		return indicative;
	}

	public static PhraseProperties MapMorphemeToPhraseProperties (MorphemeProperties morphemeProperties) {
	
		PhraseProperties phraseProperties = PhraseProperties.None;

		if ((morphemeProperties & MorphemeProperties.Plural) == MorphemeProperties.Plural)
			phraseProperties |= PhraseProperties.Plural;

		if ((morphemeProperties & MorphemeProperties.Femenine) == MorphemeProperties.Femenine)
			phraseProperties |= PhraseProperties.Femenine;

		if ((morphemeProperties & MorphemeProperties.Neutral) == MorphemeProperties.Neutral)
			phraseProperties |= PhraseProperties.Neutral;

		if ((morphemeProperties & MorphemeProperties.Indefinite) == MorphemeProperties.Indefinite)
			phraseProperties |= PhraseProperties.Indefinite;

		if ((morphemeProperties & MorphemeProperties.Passive) == MorphemeProperties.Passive)
			phraseProperties |= PhraseProperties.Passive;

		if ((morphemeProperties & MorphemeProperties.Uncountable) == MorphemeProperties.Uncountable)
			phraseProperties |= PhraseProperties.Uncountable;

		if ((morphemeProperties & MorphemeProperties.FirstPerson) == MorphemeProperties.FirstPerson)
			phraseProperties |= PhraseProperties.FirstPerson;

		if ((morphemeProperties & MorphemeProperties.SecondPerson) == MorphemeProperties.SecondPerson)
			phraseProperties |= PhraseProperties.SecondPerson;

		if ((morphemeProperties & MorphemeProperties.ThirdPerson) == MorphemeProperties.ThirdPerson)
			phraseProperties |= PhraseProperties.ThirdPerson;

		return phraseProperties;
	}

	public Morpheme GetAppropiateVerbNominalizationIndicative (PhraseProperties phraseProperties) {

		if ((phraseProperties & PhraseProperties.Passive) == PhraseProperties.Passive)
			return _verbIndicatives[IndicativeType.PassiveNominalization];
		else
			return _verbIndicatives[IndicativeType.ActiveNominalization];
	}

	public Morpheme GetAppropiateVerbIndicative (PhraseProperties phraseProperties, string tense) {

		switch (tense) {

		case VerbTenses.Infinitive:
			return _verbIndicatives [IndicativeType.InfinitiveTense];

		case VerbTenses.Null:
			if ((phraseProperties & PhraseProperties.Plural) == PhraseProperties.Plural) {
				if ((phraseProperties & PhraseProperties.FirstPerson) == PhraseProperties.FirstPerson) {
					
					return _verbIndicatives [IndicativeType.NullTense + IndicativeType.Plural + IndicativeType.FirstPerson];

				} else if ((phraseProperties & PhraseProperties.SecondPerson) == PhraseProperties.SecondPerson) {
					
					return _verbIndicatives [IndicativeType.NullTense + IndicativeType.Plural + IndicativeType.SecondPerson];

				} else if ((phraseProperties & PhraseProperties.ThirdPerson) == PhraseProperties.ThirdPerson) {

					return _verbIndicatives [IndicativeType.NullTense + IndicativeType.Plural + IndicativeType.ThirdPerson];

				}
			} else {
				if ((phraseProperties & PhraseProperties.FirstPerson) == PhraseProperties.FirstPerson) {

					return _verbIndicatives [IndicativeType.NullTense + IndicativeType.Singular + IndicativeType.FirstPerson];

				} else if ((phraseProperties & PhraseProperties.SecondPerson) == PhraseProperties.SecondPerson) {

					return _verbIndicatives [IndicativeType.NullTense + IndicativeType.Singular + IndicativeType.SecondPerson];

				} else if ((phraseProperties & PhraseProperties.ThirdPerson) == PhraseProperties.ThirdPerson) {

					return _verbIndicatives [IndicativeType.NullTense + IndicativeType.Singular + IndicativeType.ThirdPerson];

				}
			}
			break;

		case VerbTenses.Past:
			if ((phraseProperties & PhraseProperties.Plural) == PhraseProperties.Plural) {
				if ((phraseProperties & PhraseProperties.FirstPerson) == PhraseProperties.FirstPerson) {

					return _verbIndicatives [IndicativeType.PastTense + IndicativeType.Plural + IndicativeType.FirstPerson];

				} else if ((phraseProperties & PhraseProperties.SecondPerson) == PhraseProperties.SecondPerson) {

					return _verbIndicatives [IndicativeType.PastTense + IndicativeType.Plural + IndicativeType.SecondPerson];

				} else if ((phraseProperties & PhraseProperties.ThirdPerson) == PhraseProperties.ThirdPerson) {

					return _verbIndicatives [IndicativeType.PastTense + IndicativeType.Plural + IndicativeType.ThirdPerson];

				}
			} else {
				if ((phraseProperties & PhraseProperties.FirstPerson) == PhraseProperties.FirstPerson) {

					return _verbIndicatives [IndicativeType.PastTense + IndicativeType.Singular + IndicativeType.FirstPerson];

				} else if ((phraseProperties & PhraseProperties.SecondPerson) == PhraseProperties.SecondPerson) {

					return _verbIndicatives [IndicativeType.PastTense + IndicativeType.Singular + IndicativeType.SecondPerson];

				} else if ((phraseProperties & PhraseProperties.ThirdPerson) == PhraseProperties.ThirdPerson) {

					return _verbIndicatives [IndicativeType.PastTense + IndicativeType.Singular + IndicativeType.ThirdPerson];

				}
			}
			break;

		case VerbTenses.Present:
			if ((phraseProperties & PhraseProperties.Plural) == PhraseProperties.Plural) {
				if ((phraseProperties & PhraseProperties.FirstPerson) == PhraseProperties.FirstPerson) {

					return _verbIndicatives [IndicativeType.PresentTense + IndicativeType.Plural + IndicativeType.FirstPerson];

				} else if ((phraseProperties & PhraseProperties.SecondPerson) == PhraseProperties.SecondPerson) {

					return _verbIndicatives [IndicativeType.PresentTense + IndicativeType.Plural + IndicativeType.SecondPerson];

				} else if ((phraseProperties & PhraseProperties.ThirdPerson) == PhraseProperties.ThirdPerson) {

					return _verbIndicatives [IndicativeType.PresentTense + IndicativeType.Plural + IndicativeType.ThirdPerson];

				}
			} else {
				if ((phraseProperties & PhraseProperties.FirstPerson) == PhraseProperties.FirstPerson) {

					return _verbIndicatives [IndicativeType.PresentTense + IndicativeType.Singular + IndicativeType.FirstPerson];

				} else if ((phraseProperties & PhraseProperties.SecondPerson) == PhraseProperties.SecondPerson) {

					return _verbIndicatives [IndicativeType.PresentTense + IndicativeType.Singular + IndicativeType.SecondPerson];

				} else if ((phraseProperties & PhraseProperties.ThirdPerson) == PhraseProperties.ThirdPerson) {

					return _verbIndicatives [IndicativeType.PresentTense + IndicativeType.Singular + IndicativeType.ThirdPerson];

				}
			}
			break;

		case VerbTenses.Future:
			if ((phraseProperties & PhraseProperties.Plural) == PhraseProperties.Plural) {
				if ((phraseProperties & PhraseProperties.FirstPerson) == PhraseProperties.FirstPerson) {

					return _verbIndicatives [IndicativeType.FutureTense + IndicativeType.Plural + IndicativeType.FirstPerson];

				} else if ((phraseProperties & PhraseProperties.SecondPerson) == PhraseProperties.SecondPerson) {

					return _verbIndicatives [IndicativeType.FutureTense + IndicativeType.Plural + IndicativeType.SecondPerson];

				} else if ((phraseProperties & PhraseProperties.ThirdPerson) == PhraseProperties.ThirdPerson) {

					return _verbIndicatives [IndicativeType.FutureTense + IndicativeType.Plural + IndicativeType.ThirdPerson];

				}
			} else {
				if ((phraseProperties & PhraseProperties.FirstPerson) == PhraseProperties.FirstPerson) {

					return _verbIndicatives [IndicativeType.FutureTense + IndicativeType.Singular + IndicativeType.FirstPerson];

				} else if ((phraseProperties & PhraseProperties.SecondPerson) == PhraseProperties.SecondPerson) {

					return _verbIndicatives [IndicativeType.FutureTense + IndicativeType.Singular + IndicativeType.SecondPerson];

				} else if ((phraseProperties & PhraseProperties.ThirdPerson) == PhraseProperties.ThirdPerson) {

					return _verbIndicatives [IndicativeType.FutureTense + IndicativeType.Singular + IndicativeType.ThirdPerson];

				}
			}
			break;

		default:
			throw new System.Exception ("Unhandled tense: " + tense);
		}

		throw new System.Exception ("NO proper indicative found...");
	}

	private static string Affix (string word1, string word2) {

		Match onsetMatch = StartsWithVowelRegex.Match (word2);

		if (onsetMatch.Success) {
			Match vowelMatch = EndsWithVowelsRegex.Match (word1);

			if (vowelMatch.Success) {
				word1 += "---"; // lousy hack
				word1 = word1.Replace (vowelMatch.Groups["vowels"].Value + "---", string.Empty);
			}
		} else {
			word1 = EndsWithConsonantsRegex.Replace (word1, string.Empty);
		}

		return word1 + word2;
	}

	public static string AppendAdjunction (string phrase, string adjunction, AdjunctionProperties properties, bool forceAffixed = false) {
		
		if (string.IsNullOrEmpty (adjunction))
			return phrase;
			
		if ((properties & AdjunctionProperties.GoesAfter) == AdjunctionProperties.GoesAfter) {

			if (forceAffixed || ((properties & AdjunctionProperties.IsAffixed) == AdjunctionProperties.IsAffixed)) {
				
				phrase = Affix (phrase, adjunction);

			} else if ((properties & AdjunctionProperties.IsLinkedWithDash) == AdjunctionProperties.IsLinkedWithDash) {

				phrase += "-" + adjunction;

			} else {

				phrase += " " + adjunction;
			}
		} else {
			if (forceAffixed || ((properties & AdjunctionProperties.IsAffixed) == AdjunctionProperties.IsAffixed)) {

				phrase = Affix (adjunction, phrase);

			} else if ((properties & AdjunctionProperties.IsLinkedWithDash) == AdjunctionProperties.IsLinkedWithDash) {

				phrase = adjunction + "-" + phrase;

			} else {

				phrase = adjunction + " " + phrase;
			}
		}

		return phrase;
	}

	public Phrase TranslatePhrase (ParsedPhrase parsedPhrase, GetRandomFloatDelegate getRandomFloat) {

		Phrase translatedPhrase = null;

		if (parsedPhrase.Attributes.ContainsKey (ParsedPhraseAttributeId.PhrasePlusPrepositionalPhrase)) {

			MatchCollection PhraseIndexMatchCollection = PhraseIndexRegex.Matches (parsedPhrase.Value);

			int startPhraseIndex = int.Parse (PhraseIndexMatchCollection [0].Groups ["index"].Value);
			int prepositionalPhraseIndex = int.Parse (PhraseIndexMatchCollection [1].Groups ["index"].Value);

			Phrase startPhrase = TranslatePhrase (parsedPhrase.SubPhrases [startPhraseIndex], getRandomFloat);
			Phrase prepositionalPhrase = TranslatePhrase (parsedPhrase.SubPhrases [prepositionalPhraseIndex], getRandomFloat);

			translatedPhrase = MergePhrases (startPhrase, prepositionalPhrase);

		} else if (parsedPhrase.Attributes.ContainsKey (ParsedPhraseAttributeId.PrepositionalPhrase)) {

			MatchCollection PhraseIndexMatchCollection = PhraseIndexRegex.Matches (parsedPhrase.Value);

			int complementPhraseIndex = int.Parse (PhraseIndexMatchCollection [0].Groups ["index"].Value);

			Phrase complementPhrase = TranslatePhrase (parsedPhrase.SubPhrases [complementPhraseIndex], getRandomFloat);

			///

			string relationWord = parsedPhrase.Value.Replace (PhraseIndexMatchCollection [0].Value, "").Trim (new char[] {' '});

			translatedPhrase = BuildAdpositionalPhrase (relationWord, complementPhrase, getRandomFloat); 

		} else if (parsedPhrase.Attributes.ContainsKey (ParsedPhraseAttributeId.NounPhrase)) {

			translatedPhrase = TranslateNounPhrase (parsedPhrase.Value, getRandomFloat); 
		}

		if (parsedPhrase.Attributes.ContainsKey (ParsedPhraseAttributeId.Noun)) {

			translatedPhrase = Agglutinate (translatedPhrase);
		}


		return translatedPhrase;
	}

	public Phrase TranslatePhrase (string untranslatedPhrase, GetRandomFloatDelegate getRandomFloat) {

		return TranslatePhrase (ParsePhrase (untranslatedPhrase), getRandomFloat);
	}

	private Phrase TranslateNounPhrase (string untranslatedNounPhrase, GetRandomFloatDelegate getRandomFloat) {

		bool absentArticle = true;
		PhraseProperties phraseProperties = PhraseProperties.None;

		Phrase nounPhrase = null;

		List<Phrase> nounAdjunctionPhrases = new List<Phrase> ();
		List<Morpheme> adjectives = new List<Morpheme> ();

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

			ParsedWord parsedPhrasePart = ParseWord (phrasePart);

			if (!parsedPhrasePart.Attributes.ContainsKey (ParsedWordAttributeId.Name) && !parsedPhrasePart.Attributes.ContainsKey (ParsedWordAttributeId.UncountableNoun) && (absentArticle)) {
				phraseProperties |= PhraseProperties.Indefinite;
			}

			if (parsedPhrasePart.Attributes.ContainsKey (ParsedWordAttributeId.NounAdjunct)) {
				
				nounAdjunctionPhrases.Add (TranslateNoun (phrasePart, phraseProperties, getRandomFloat, true));

			} else if (parsedPhrasePart.Attributes.ContainsKey (ParsedWordAttributeId.Adjective)) {
			
				adjectives.Add (GenerateAdjective (parsedPhrasePart.Value, getRandomFloat));

			} else {

				nounPhrase = TranslateNoun (phrasePart, phraseProperties, getRandomFloat);
				phraseProperties = nounPhrase.Properties;
			}
		}

		if (nounPhrase == null) {

			Debug.Break ();
			throw new System.Exception ("nounPhrase can't be null");
		}

		foreach (Phrase nounAdjunctionPhrase in nounAdjunctionPhrases) {

			nounPhrase.Text = AppendAdjunction (nounPhrase.Text, nounAdjunctionPhrase.Text, NounAdjunctionProperties);
		}

		foreach (Morpheme adjective in adjectives) {

			nounPhrase.Text = AppendAdjunction (nounPhrase.Text, adjective.Value, AdjectiveAdjunctionProperties);
		}

		Morpheme article = GetAppropiateArticle (phraseProperties);

		if (article != null) {
			nounPhrase.Text = AppendAdjunction (nounPhrase.Text, article.Value, ArticleAdjunctionProperties);
		}

		nounPhrase.Original = untranslatedNounPhrase;
		nounPhrase.Meaning = ClearConstructCharacters (untranslatedNounPhrase);

		return nounPhrase;
	}

	public Phrase TranslateNoun (string untranslatedNoun, PhraseProperties properties, GetRandomFloatDelegate getRandomFloat, bool nounAdjunction = false) {

		string[] nounParts = untranslatedNoun.Split (new char[] { ':' });

		Morpheme mainNoun = null;

		List<Morpheme> nounComponents = new List<Morpheme> ();

		bool isPlural = false;
		bool hasRandomGender = true;
		bool isFemenineNoun = false;
		bool isNeutralNoun = false;
		bool isUncountableNoun = false;

		for (int i = 0; i < nounParts.Length; i++) {

			Morpheme noun = null;

			string nounPart = nounParts [i];

			ParsedWord parsedWordPart = ParseWord (nounPart);

			hasRandomGender = true;
			isFemenineNoun = false;
			isNeutralNoun = false;
			isUncountableNoun = false;

			if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.FemenineNoun)) {
				hasRandomGender = false;
				isFemenineNoun = true;
			} else if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.MasculineNoun)) {
				hasRandomGender = false;
			} else if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.NeutralNoun)) {
				hasRandomGender = false;
				isNeutralNoun = true;
			} else if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.UncountableNoun)) {
				isUncountableNoun = true;
			}

			if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.IrregularVerb) || 
				parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.RegularVerb) || 
				parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.NominalizedIrregularVerb) || 
				parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.NominalizedRegularVerb)) {

				bool isPassiveNoun = false;

				string verbMeaning;
				string nounMeaning = parsedWordPart.Value;
				string tense = VerbTenses.Null;

				if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.NominalizedIrregularVerb)) {

					verbMeaning = parsedWordPart.Attributes [ParsedWordAttributeId.NominalizedIrregularVerb] [0];

					if (((i + 1) < nounParts.Length) && PluralSuffixRegex.IsMatch (nounParts[i + 1])) {
						isPlural = true;
						i++;
					}

				} else if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.IrregularVerb)) {

					isPassiveNoun = true;
				
					verbMeaning = parsedWordPart.Attributes [ParsedWordAttributeId.IrregularVerb] [0];
					tense = parsedWordPart.Attributes [ParsedWordAttributeId.IrregularVerb] [2];

				} else if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.NominalizedRegularVerb)) {

					verbMeaning = parsedWordPart.Value;
					nounMeaning += nounParts [i+1];

					// skip next wordPart (suffix)
					i++;

					if (((i + 1) < nounParts.Length) && PluralSuffixRegex.IsMatch (nounParts[i + 1])) {
						isPlural = true;
						i++;
					}

				} else {

					isPassiveNoun = true;

					verbMeaning = parsedWordPart.Value;
					tense = parsedWordPart.Attributes [ParsedWordAttributeId.RegularVerb] [1];
					nounMeaning += nounParts [i+1];

					// skip next wordPart (suffix)
					i++;
				}

				Morpheme verb = GenerateVerb (verbMeaning, getRandomFloat);

				noun = GenerateNominalizedVerb (nounMeaning, verb, tense, getRandomFloat, isPlural, hasRandomGender, isFemenineNoun, isNeutralNoun, isPassiveNoun);

			} else {

				string meaning = parsedWordPart.Value;
				if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.IrregularPluralNoun)) {
					meaning = parsedWordPart.Attributes[ParsedWordAttributeId.IrregularPluralNoun][0];
					isPlural = true;
				}

				if ((!isPlural) && ((i + 1) < nounParts.Length) && PluralSuffixRegex.IsMatch (nounParts[i + 1])) {
					isPlural = true;
					i++;
				}
				
				noun = GenerateNoun (meaning, getRandomFloat, false, hasRandomGender, isFemenineNoun, isNeutralNoun);
			}

			isFemenineNoun = ((noun.Properties & MorphemeProperties.Femenine) == MorphemeProperties.Femenine);
			isNeutralNoun = ((noun.Properties & MorphemeProperties.Neutral) == MorphemeProperties.Neutral);
			hasRandomGender = false;

			if (mainNoun != null) {
				nounComponents.Add (mainNoun);
			}

			mainNoun = noun;
		}

		if (isPlural) {
			properties |= PhraseProperties.Plural;
		}

		if (isFemenineNoun)
			properties |= PhraseProperties.Femenine;
		else if (isNeutralNoun)
			properties |= PhraseProperties.Neutral;

		if (isUncountableNoun)
			properties |= PhraseProperties.Uncountable;

		string text = mainNoun.Value;

		foreach (Morpheme nounComponent in nounComponents) {
			text = AppendAdjunction (text, nounComponent.Value, NounAdjunctionProperties, true);
		}

		if (!nounAdjunction) {
			Morpheme nounIndicative = GetAppropiateNounIndicative (properties);

			text = AppendAdjunction (text, nounIndicative.Value, NounIndicativeAdjunctionProperties);
		}

		Phrase phrase = new Phrase ();
		phrase.Text = text;
		phrase.Original = untranslatedNoun;
		phrase.Meaning = ClearConstructCharacters (untranslatedNoun);
		phrase.Properties = properties;

		return phrase;
	}

	public static ParsedPhrase ParsePhrase (string phrase) {

		ParsedPhrase parsedPhrase = null;

		List<ParsedPhrase> parsedPhrases = new List<ParsedPhrase> ();
		int phraseIndex = 0;

		List<string> foundWords = new List<string> ();
		int wordIndex = 0;

		while (true) {
			MatchCollection wordMatchCollection = WordPartRegex.Matches (phrase);

			foreach (Match match in wordMatchCollection) {

				foundWords.Add (match.Value);

				phrase = phrase.Replace (match.Value, "<" + wordIndex + ">");
				wordIndex++;
			}

			Match phraseMatch = PhrasePartRegex.Match (phrase);

			if (!phraseMatch.Success)
				break;

			parsedPhrase = new ParsedPhrase ();

			string phraseValue = phraseMatch.Groups ["value"].Value;
			string subPhrase = "";

			phrase = phrase.Replace (phraseMatch.Value, "{" + phraseIndex + "}");
			phraseIndex++;

			while (phraseMatch.Success) {

				subPhrase = phraseMatch.Groups ["phrase"].Value;

				parsedPhrase.Attributes.Add (phraseMatch.Groups ["attr"].Value, phraseMatch.Groups ["params"].Success ? phraseMatch.Groups ["params"].Value.Split (new char[] {','}) : null);

				phraseMatch = PhrasePartRegex.Match (subPhrase);
			}

			for (int i = 0; i < wordIndex; i++) {
			
				phraseValue = phraseValue.Replace ("<" + i + ">", foundWords [i]);
			}

			parsedPhrase.Value = phraseValue;

			for (int i = 0; i < phraseIndex; i++) {

				if (phraseValue.Contains ("{" + i + "}"))
					parsedPhrase.SubPhrases.Add (i, parsedPhrases[i]);
			}

			parsedPhrases.Add (parsedPhrase);
		}

		return parsedPhrase;
	}

	public static ParsedWord ParseWord (string word) {

		ParsedWord parsedWord = new ParsedWord ();

		while (true) {
			Match match = WordPartRegex.Match (word);

			if (!match.Success)
				break;

			word = word.Replace (match.Value, match.Groups ["word"].Value);

			parsedWord.Attributes.Add (match.Groups ["attr"].Value, match.Groups ["params"].Success ? match.Groups ["params"].Value.Split (new char[] {','}) : null);
		}

		parsedWord.Value = word;

		return parsedWord;
	}

	public static Phrase Agglutinate (Phrase phrase) {

		Phrase agglutinatedPhrase = new Phrase (phrase);

		agglutinatedPhrase.Text = Agglutinate (phrase.Text);

		agglutinatedPhrase.Meaning = phrase.Meaning.Replace (' ', '-');

		return agglutinatedPhrase;
	}

	public static string Agglutinate (string sentence) {

		sentence = sentence.ToLower ();

		string[] words = sentence.Split (new char[] {' ', '-'});

		sentence = "";

		foreach (string word in words) {
			sentence = Affix (sentence, word);
		}

		return sentence;
	}

	public static void TurnIntoProperName (Phrase phrase, bool agglutinate) {

		bool linkWithDashMeaning = agglutinate;

		string newText = TurnIntoProperName (phrase.Text, agglutinate, false);
		string newMeaning = TurnIntoProperName (phrase.Meaning, false, linkWithDashMeaning);

		phrase.Text = newText;
		phrase.Meaning = newMeaning;
	}

	public static string TurnIntoProperName (string sentence, bool agglutinate, bool linkWithDash) {
		
		string[] words = sentence.Split (new char[] {' '});

		string newSentence = string.Empty;

		if (!agglutinate) {
			char link = linkWithDash ? '-' : ' ';

			bool first = true;
			foreach (string word in words) {

				if (first) {

					newSentence = MakeFirstLetterUpper (word);
					first = false;

					continue;
				}

				newSentence += link + MakeFirstLetterUpper (word);
			}
		} else {
		
			newSentence = MakeFirstLetterUpper (Agglutinate (sentence));
		}

		return newSentence;
	}

	public static string MakeFirstLetterUpper (string sentence) {

		if (string.IsNullOrEmpty (sentence)) {
			throw new System.Exception ("Empty sentence");
		}

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

		string newText = MakeFirstLetterUpper (phrase.Text);
		string newMeaning = MakeFirstLetterUpper (phrase.Meaning);

		phrase.Text = newText;
		phrase.Meaning = newMeaning;
	}

	public void Synchronize () {

		ArticlePropertiesInt = (int)_articleProperties;
		NounIndicativePropertiesInt = (int)_nounIndicativeProperties;
		VerbIndicativePropertiesInt = (int)_verbIndicativeProperties;

		ArticleAdjunctionPropertiesInt = (int)ArticleAdjunctionProperties;
		NounIndicativeAdjunctionPropertiesInt = (int)NounIndicativeAdjunctionProperties;
		VerbIndicativeAdjunctionPropertiesInt = (int)VerbIndicativeAdjunctionProperties;
		AdpositionAdjunctionPropertiesInt = (int)AdpositionAdjunctionProperties;
		AdjectiveAdjunctionPropertiesInt = (int)AdjectiveAdjunctionProperties;
		NounAdjunctionPropertiesInt = (int)NounAdjunctionProperties;

		foreach (Morpheme morpheme in Articles) {
			morpheme.Synchronize ();
		}

		foreach (Morpheme morpheme in NounIndicatives) {
			morpheme.Synchronize ();
		}

		foreach (Morpheme morpheme in VerbIndicatives) {
			morpheme.Synchronize ();
		}

		foreach (Morpheme morpheme in Adpositions) {
			morpheme.Synchronize ();
		}

		foreach (Morpheme morpheme in Adjectives) {
			morpheme.Synchronize ();
		}

		foreach (Morpheme morpheme in Nouns) {
			morpheme.Synchronize ();
		}

		foreach (Morpheme morpheme in Verbs) {
			morpheme.Synchronize ();
		}
	}

	public void FinalizeLoad () {

		_articleProperties = (GeneralArticleProperties)ArticlePropertiesInt;
		_nounIndicativeProperties = (GeneralNounIndicativeProperties)NounIndicativePropertiesInt;
		_verbIndicativeProperties = (GeneralVerbIndicativeProperties)VerbIndicativePropertiesInt;

		ArticleAdjunctionProperties = (AdjunctionProperties)ArticleAdjunctionPropertiesInt;
		NounIndicativeAdjunctionProperties = (AdjunctionProperties)NounIndicativeAdjunctionPropertiesInt;
		VerbIndicativeAdjunctionProperties = (AdjunctionProperties)VerbIndicativeAdjunctionPropertiesInt;
		AdpositionAdjunctionProperties = (AdjunctionProperties)AdpositionAdjunctionPropertiesInt;
		AdjectiveAdjunctionProperties = (AdjunctionProperties)AdjectiveAdjunctionPropertiesInt;
		NounAdjunctionProperties = (AdjunctionProperties)NounAdjunctionPropertiesInt;

		_articles = new Dictionary<string, Morpheme> (Articles.Count);
		_nounIndicatives = new Dictionary<string, Morpheme> (NounIndicatives.Count);
		_verbIndicatives = new Dictionary<string, Morpheme> (VerbIndicatives.Count);

		// initialize dictionaries

		foreach (Morpheme word in Articles) {
			_articles.Add (word.Meaning, word);
		}

		foreach (Morpheme word in VerbIndicatives) {
			_verbIndicatives.Add (word.Meaning, word);
		}

		foreach (Morpheme word in NounIndicatives) {
			_nounIndicatives.Add (word.Meaning, word);
		}

		foreach (Morpheme word in Adpositions) {
			_adpositions.Add (word.Meaning, word);
			_existingAdpositionMorphemeValues.Add (word.Value);
		}

		foreach (Morpheme word in Adjectives) {
			_adjectives.Add (word.Meaning, word);
			_existingAdjectiveMorphemeValues.Add (word.Value);
		}

		foreach (Morpheme word in Verbs) {
			_verbs.Add (word.Meaning, word);
			_existingVerbMorphemeValues.Add (word.Value);
		}

		foreach (Morpheme word in Nouns) {
			_nouns.Add (word.Meaning, word);

			if (_existingNounMorphemeValues.ContainsKey (word.Value)) {
			
				float newToleranceFactor = _existingNounMorphemeValues [word.Value] * _homographToleranceDecayFactor;
				_existingNounMorphemeValues [word.Value] = newToleranceFactor;
			} else {

				_existingNounMorphemeValues.Add (word.Value, _initialHomographTolerance);
			}
		}

		// Finish loading morphemes

		foreach (Morpheme morpheme in Articles) {
			morpheme.FinalizeLoad ();
		}

		foreach (Morpheme morpheme in NounIndicatives) {
			morpheme.FinalizeLoad ();
		}

		foreach (Morpheme morpheme in Adpositions) {
			morpheme.FinalizeLoad ();
		}

		foreach (Morpheme morpheme in Adjectives) {
			morpheme.FinalizeLoad ();
		}

		foreach (Morpheme morpheme in Nouns) {
			morpheme.FinalizeLoad ();
		}
	}
}
