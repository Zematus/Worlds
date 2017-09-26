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

		public virtual void Synchronize () {

		}

		public virtual void FinalizeLoad () {
			
		}
	}

	public class NounPhrase : Phrase {

		[XmlAttribute("Properties")]
		public int PropertiesInt;

		[XmlIgnore]
		public PhraseProperties Properties;

		public override void Synchronize () {

			PropertiesInt = (int)Properties;
		}

		public override void FinalizeLoad () {

			Properties = (PhraseProperties)PropertiesInt;
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
		public Dictionary<string, string> Attributes = new Dictionary<string, string> ();
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

	public static class ParsedWordAttributeId {

		public const string NounPluralIndicative = "npl";
		public const string FemenineNoun = "fn";
		public const string MasculineNoun = "mn";
		public const string NeutralNoun = "nn";
		public const string UncountableNoun = "un";
		public const string RegularVerbDerivedNoun = "rvn";
		public const string IrregularVerbDerivedNoun = "ivn";
		public const string IrregularPluralNoun = "ipn";
		public const string NounAdjunct = "nad";
		public const string Adjective = "adj";
		public const string RegularVerb = "rvb";
		public const string IrregularVerb = "ivb";
//		public const string Preposition = "pre";
//		public const string Import = "import";
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
		HasUncountableIndicative = 0x080,

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
		HasIndefinitePluralNeutralIndicative = 0x054,
		HasUncountableMasculineIndicative = 0x081,
		HasUncountableFemenineIndicative = 0x082,
		HasUncountableNeutralIndicative = 0x084
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

	public enum MorphemeProperties
	{
		None = 0x00,
		Plural = 0x01,
		Indefinite = 0x02,
		Femenine = 0x04,
		Neutral = 0x08,
		Irregular = 0x10,
		Uncountable = 0x20,

		IsNotMasculine = 0x0c
	}

	public enum PhraseProperties
	{
		None = 0x00,
		Plural = 0x01,
		Indefinite = 0x02,
		Uncountable = 0x04,
		Femenine = 0x08,
		Neutral = 0x10
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
		public const string UncountableMasculine = "UncountableMasculine";
		public const string UncountableFemenine = "UncountableFemenine";
		public const string UncountableNeutral = "UncountableNeutral";
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

//	public static char[] OnsetLetters = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
//
//	public static char[] NucleusLetters = new char[] { 'a', 'e', 'i', 'o', 'u' };
//
//	public static char[] CodaLetters = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z', '\'' };

	public static Regex WordPartRegex = new Regex (@"\[(?<attr>\w+)(?:\((?<param>\w+)\))?\](?:\[w+\])*(?<word>[\w\'\-]*)");
	public static Regex ArticleRegex = new Regex (@"^((?<def>the)|(?<indef>(a|an)))$");
	public static Regex PluralIndicativeRegex = new Regex (@"^(es|s)$");

//	public GeneralGenderProperties GenderProperties;

	[XmlAttribute("ArticleProperties")]
	public int ArticlePropertiesInt;
	[XmlAttribute("IndicativeProperties")]
	public int IndicativePropertiesInt;

	private GeneralArticleProperties _articleProperties;
	private GeneralIndicativeProperties _indicativeProperties;

//	public GeneralTypeProperties ArticleTypeProperties;
//	public GeneralTypeProperties AdpositionTypeProperties;

	[XmlAttribute("ArticleAdjunctionProperties")]
	public int ArticleAdjunctionPropertiesInt;
	[XmlAttribute("IndicativeAdjunctionProperties")]
	public int IndicativeAdjunctionPropertiesInt;
	[XmlAttribute("AdpositionAdjunctionProperties")]
	public int AdpositionAdjunctionPropertiesInt;
	[XmlAttribute("AdjectiveAdjunctionProperties")]
	public int AdjectiveAdjunctionPropertiesInt;
	[XmlAttribute("NounAdjunctionProperties")]
	public int NounAdjunctionPropertiesInt;

	[XmlIgnore]
	public AdjunctionProperties ArticleAdjunctionProperties;
	[XmlIgnore]
	public AdjunctionProperties IndicativeAdjunctionProperties;
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
//		[XmlAttribute("OALCD")]
//		public float OnsetAddLetterChanceDecay;
		[XmlAttribute("OGC")]
		public int OnsetGroupCount;

		[XmlAttribute("NSALC")]
		public float NucleusChance;
//		[XmlAttribute("NALCD")]
//		public float NucleusAddLetterChanceDecay;
		[XmlAttribute("NGC")]
		public int NucleusGroupCount;

		[XmlAttribute("CSALC")]
		public float CodaChance;
//		[XmlAttribute("CALCD")]
//		public float CodaAddLetterChanceDecay;
		[XmlAttribute("CGC")]
		public int CodaGroupCount;

		public CharacterGroup[] OnsetGroups;
		public CharacterGroup[] NucleusGroups;
		public CharacterGroup[] CodaGroups;

		public List<string> Syllables = new List<string> ();

		public SyllableSet () {
			
		}

		public void GenerateCharacterGroups (GetRandomFloatDelegate getRandomFloat) {

//			OnsetGroups = Language.GenerateCharacterGroups (OnsetLetters, OnsetStartAddLetterChance, OnsetAddLetterChanceDecay, getRandomFloat, OnsetGroupCount);
//			NucleusGroups = Language.GenerateCharacterGroups (NucleusLetters, NucleusStartAddLetterChance, NucleusAddLetterChanceDecay, getRandomFloat, NucleusGroupCount);
//			CodaGroups = Language.GenerateCharacterGroups (CodaLetters, CodaStartAddLetterChance, CodaAddLetterChanceDecay, getRandomFloat, CodaGroupCount);
			OnsetGroups = Language.GenerateCharacterGroups (OnsetLetters, getRandomFloat, OnsetGroupCount);
			NucleusGroups = Language.GenerateCharacterGroups (NucleusLetters, getRandomFloat, NucleusGroupCount);
			CodaGroups = Language.GenerateCharacterGroups (CodaLetters, getRandomFloat, CodaGroupCount);
		}

		public string GetRandomSyllable (GetRandomFloatDelegate getRandomFloat) {

			int selCount = Syllables.Count + AddSyllableModifier;

			int randOption = (int)Mathf.Floor (selCount * getRandomFloat ());

			if (randOption < Syllables.Count) {
			
				return Syllables [randOption];
			}

			string syllable = GenerateSyllable (OnsetGroups, OnsetChance, NucleusGroups, NucleusChance, CodaGroups, CodaChance, getRandomFloat);

			if (!Syllables.Contains (syllable)) {
				Syllables.Add (syllable);
			}

			return syllable;
		}
	}

	public SyllableSet ArticleSyllables = new SyllableSet ();
//	public SyllableSet ArticleNextSyllables = new SyllableSet ();
	public SyllableSet DerivativeArticleStartSyllables = new SyllableSet ();
	public SyllableSet DerivativeArticleNextSyllables = new SyllableSet ();

	public SyllableSet IndicativeStartSyllables = new SyllableSet ();
	public SyllableSet IndicativeNextSyllables = new SyllableSet ();

	public SyllableSet AdpositionStartSyllables = new SyllableSet ();
	public SyllableSet AdpositionNextSyllables = new SyllableSet ();

	public SyllableSet AdjectiveStartSyllables = new SyllableSet ();
	public SyllableSet AdjectiveNextSyllables = new SyllableSet ();

	public SyllableSet NounStartSyllables = new SyllableSet ();
	public SyllableSet NounNextSyllables = new SyllableSet ();

	public List<Morpheme> Articles;
	public List<Morpheme> Indicatives;
	public List<Morpheme> Adpositions = new List<Morpheme> ();
	public List<Morpheme> Adjectives = new List<Morpheme> ();
	public List<Morpheme> Nouns = new List<Morpheme> ();

	private Dictionary<string, Morpheme> _articles;
	private Dictionary<string, Morpheme> _indicatives;
	private Dictionary<string, Morpheme> _adpositions = new Dictionary<string, Morpheme> ();
	private Dictionary<string, Morpheme> _adjectives = new Dictionary<string, Morpheme> ();
	private Dictionary<string, Morpheme> _nouns = new Dictionary<string, Morpheme> ();

	public Language () {
		
	}

	public Language (long id) {
	
		Id = id;
	}

//	public static CharacterGroup GenerateCharacterGroup (char[] characterSet, float startAddLetterChance, float addLetterChanceDecay, GetRandomFloatDelegate getRandomFloat) {
//
//		float addLetterChance = startAddLetterChance;
//
//		string characters = "";
//
//		while (getRandomFloat () < addLetterChance) {
//
//			int charIndex = (int)Mathf.Floor(characterSet.Length * getRandomFloat ());
//
//			characters += characterSet [charIndex];
//
//			addLetterChance *= addLetterChanceDecay;
//		}
//
//		return new CharacterGroup (characters, getRandomFloat ());
//	}
//
//	public static CharacterGroup[] GenerateCharacterGroups (char[] characterSet, float startAddLetterChance, float addLetterChanceDecay, GetRandomFloatDelegate getRandomFloat, int count) {
//
//		CharacterGroup[] characterGroups = new CharacterGroup[count];
//
//		for (int i = 0; i < count; i++) {
//
//			CharacterGroup characterGroup = GenerateCharacterGroup (characterSet, startAddLetterChance, addLetterChanceDecay, getRandomFloat);
//
////			for (int j = 0; j < i; j++) {
////				if (characterGroups [j].Value == characterGroup.Value) {
////					characterGroup = GenerateCharacterGroup (characterSet, startAddLetterChance, addLetterChanceDecay, getRandomFloat);
////					j = 0;
////				}
////			}
//
//			characterGroups [i] = characterGroup;
//		}
//
//		return characterGroups;
//	}

	public static CharacterGroup GenerateCharacterGroup (Letter[] letterSet, GetRandomFloatDelegate getRandomFloat) {

		float totalWeight = 0;

		foreach (Letter letter in letterSet) {
		
			totalWeight += letter.Weight;
		}

		string chossenLetterValue = CollectionUtility.WeightedSelection (letterSet, totalWeight, () => getRandomFloat ());

		return new CharacterGroup (chossenLetterValue, getRandomFloat ());
	}

	public static CharacterGroup[] GenerateCharacterGroups (Letter[] letterSet, GetRandomFloatDelegate getRandomFloat, int count) {

		CharacterGroup[] characterGroups = new CharacterGroup[count];

		for (int i = 0; i < count; i++) {

			CharacterGroup characterGroup = GenerateCharacterGroup (letterSet, getRandomFloat);

//			for (int j = 0; j < i; j++) {
//				if (characterGroups [j].Value == characterGroup.Value) {
//					characterGroup = GenerateCharacterGroup (characterSet, startAddLetterChance, addLetterChanceDecay, getRandomFloat);
//					j = 0;
//				}
//			}

			characterGroups [i] = characterGroup;
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

	public static string GenerateSyllable (
		CharacterGroup[] onsetGroups,
		float onsetChance,
		CharacterGroup[] nucleusGroups,
		float nucleusChance,
		CharacterGroup[] codaGroups,
		float codaChance,
		GetRandomFloatDelegate getRandomFloat) {

		CollectionUtility.NormalizedValueGeneratorDelegate valueGeneratorDelegate = new CollectionUtility.NormalizedValueGeneratorDelegate (getRandomFloat);

		string onset = (onsetChance > getRandomFloat ()) ? CollectionUtility.WeightedSelection (onsetGroups, GetCharacterGroupsTotalWeight (onsetGroups), valueGeneratorDelegate) : string.Empty;
		string nucleus = (nucleusChance > getRandomFloat ()) ? CollectionUtility.WeightedSelection (nucleusGroups, GetCharacterGroupsTotalWeight (nucleusGroups), valueGeneratorDelegate) : string.Empty;
		string coda = (codaChance > getRandomFloat ()) ? CollectionUtility.WeightedSelection (codaGroups, GetCharacterGroupsTotalWeight (codaGroups), valueGeneratorDelegate) : string.Empty;

		if (nucleus == string.Empty) {
		
			return coda;
		}

		return onset + nucleus + coda;
	}

//	public static List<string> GenerateSyllables (CharacterGroup[] onsetGroups, CharacterGroup[] nucleusGroups, CharacterGroup[] codaGroups, GetRandomFloatDelegate getRandomFloat, int maxCount) {
//
//		HashSet<string> genSyllables = new HashSet<string> ();
//
//		int index = 0;
//		int genCount = 0;
//
//		while (index < maxCount) {
//
//			string syllable = GenerateSyllable (onsetGroups, nucleusGroups, codaGroups, getRandomFloat);
//
//			if (genSyllables.Add (syllable))
//				genCount++;
//
//			index++;
//		}
//
//		List<string> syllables = new List<string> (genCount);
//
//		genSyllables.CopyTo (syllables);
//
//		return syllables;
//	}

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

		float addSyllableChance = 1;
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

			morpheme += syllables.GetRandomSyllable (getRandomFloat);

			addSyllableChance *= addSyllableChanceDecay * addSyllableChanceDecay;
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
		
			return GenerateMorpheme (derivativeStartSyllables, derivativeNextSyllables, addSyllableChanceDecay, getRandomFloat) + rootWord;
		}

		return rootWord + GenerateMorpheme (derivativeNextSyllables, derivativeNextSyllables, addSyllableChanceDecay, getRandomFloat);
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

	public static Morpheme GenerateArticle (
		SyllableSet startSyllables, 
//		SyllableSet nextSyllables, 
		MorphemeProperties properties, 
		GetRandomFloatDelegate getRandomFloat) {

		Morpheme morpheme = new Morpheme ();
//		morpheme.Value = GenerateMorpheme (startSyllables, nextSyllables, 0.0f, getRandomFloat);
		morpheme.Value = GenerateMorpheme (startSyllables, getRandomFloat);
		morpheme.Properties = properties;
		morpheme.Type = WordType.Article;

		return morpheme;
	}

	public static Morpheme GenerateDerivatedArticle (
		Morpheme rootArticle, 
		SyllableSet syllables, 
//		SyllableSet nextSyllables, 
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
//		SyllableSet nextSyllables,
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		GetRandomFloatDelegate getRandomFloat, 
		out Morpheme masculine, 
		out Morpheme femenine,
		out Morpheme neutral) {

//		Morpheme firstVariant = GenerateDerivatedArticle (root, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
		Morpheme firstVariant = GenerateDerivatedArticle (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);

		Morpheme secondVariant;
		if (getRandomFloat () < 0.5f) {
//			secondVariant = GenerateDerivatedArticle (root, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
			secondVariant = GenerateDerivatedArticle (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
		} else {
//			secondVariant = GenerateDerivatedArticle (firstVariant, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
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
//		SyllableSet nextSyllables, 
		SyllableSet derivativeStartSyllables, 
		SyllableSet derivativeNextSyllables, 
		GeneralArticleProperties generalProperties, 
		GetRandomFloatDelegate getRandomFloat) {

		Dictionary<string, Morpheme> articles = new Dictionary<string, Morpheme> ();

//		Morpheme root = GenerateArticle (startSyllables, nextSyllables, MorphemeProperties.None, getRandomFloat);
		Morpheme root = GenerateArticle (syllables, MorphemeProperties.None, getRandomFloat);

//		Morpheme definite = GenerateDerivatedArticle (root, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
//		Morpheme indefinite = GenerateDerivatedArticle (root, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Indefinite, getRandomFloat);
//		Morpheme uncountable = GenerateDerivatedArticle (root, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Uncountable, getRandomFloat);
		Morpheme definite = GenerateDerivatedArticle (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
		Morpheme indefinite = GenerateDerivatedArticle (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Indefinite, getRandomFloat);
		Morpheme uncountable = GenerateDerivatedArticle (root, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Uncountable, getRandomFloat);

		if ((generalProperties & GeneralArticleProperties.HasDefiniteSingularArticles) == GeneralArticleProperties.HasDefiniteSingularArticles) {

//			Morpheme definiteSingular = GenerateDerivatedArticle (definite, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
			Morpheme definiteSingular = GenerateDerivatedArticle (definite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);

			Morpheme femenine, masculine, neutral;
//			GenerateGenderedArticles (definiteSingular, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			GenerateGenderedArticles (definiteSingular, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.DefiniteSingularFemenine;
			masculine.Meaning = IndicativeType.DefiniteSingularMasculine;
			neutral.Meaning = IndicativeType.DefiniteSingularNeutral;

			articles.Add (IndicativeType.DefiniteSingularFemenine, femenine);
			articles.Add (IndicativeType.DefiniteSingularMasculine, masculine);
			articles.Add (IndicativeType.DefiniteSingularNeutral, neutral);
		}

		if ((generalProperties & GeneralArticleProperties.HasDefinitePluralArticles) == GeneralArticleProperties.HasDefinitePluralArticles) {

//			Morpheme definitePlural = GenerateDerivatedArticle (definite, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Plural, getRandomFloat);
			Morpheme definitePlural = GenerateDerivatedArticle (definite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Plural, getRandomFloat);

			Morpheme femenine, masculine, neutral;
//			GenerateGenderedArticles (definitePlural, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			GenerateGenderedArticles (definitePlural, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.DefinitePluralFemenine;
			masculine.Meaning = IndicativeType.DefinitePluralMasculine;
			neutral.Meaning = IndicativeType.DefinitePluralNeutral;

			articles.Add (IndicativeType.DefinitePluralFemenine, femenine);
			articles.Add (IndicativeType.DefinitePluralMasculine, masculine);
			articles.Add (IndicativeType.DefinitePluralNeutral, neutral);
		}

		if ((generalProperties & GeneralArticleProperties.HasIndefiniteSingularArticles) == GeneralArticleProperties.HasIndefiniteSingularArticles) {

//			Morpheme indefiniteSingular = GenerateDerivatedArticle (indefinite, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);
			Morpheme indefiniteSingular = GenerateDerivatedArticle (indefinite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.None, getRandomFloat);

			Morpheme femenine, masculine, neutral;
//			GenerateGenderedArticles (indefiniteSingular, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			GenerateGenderedArticles (indefiniteSingular, syllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
			femenine.Meaning = IndicativeType.IndefiniteSingularFemenine;
			masculine.Meaning = IndicativeType.IndefiniteSingularMasculine;
			neutral.Meaning = IndicativeType.IndefiniteSingularNeutral;

			articles.Add (IndicativeType.IndefiniteSingularFemenine, femenine);
			articles.Add (IndicativeType.IndefiniteSingularMasculine, masculine);
			articles.Add (IndicativeType.IndefiniteSingularNeutral, neutral);
		}

		if ((generalProperties & GeneralArticleProperties.HasIndefinitePluralArticles) == GeneralArticleProperties.HasIndefinitePluralArticles) {

//			Morpheme indefinitePlural = GenerateDerivatedArticle (indefinite, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Plural, getRandomFloat);
			Morpheme indefinitePlural = GenerateDerivatedArticle (indefinite, syllables, derivativeStartSyllables, derivativeNextSyllables, MorphemeProperties.Plural, getRandomFloat);

			Morpheme femenine, masculine, neutral;
//			GenerateGenderedArticles (indefinitePlural, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
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
//			GenerateGenderedArticles (uncountable, syllables, nextSyllables, derivativeStartSyllables, derivativeNextSyllables, getRandomFloat, out masculine, out femenine, out neutral);
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

	public static MorphemeProperties GenerateWordProperties (
		GetRandomFloatDelegate getRandomFloat, 
		bool isPlural, 
		bool randomGender = false, 
		bool isFemenine = false, 
		bool isNeutral = false, 
		bool canBeIrregular = false) {

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

		float irregularChance = getRandomFloat ();

		if ((canBeIrregular) && (irregularChance < 0.05f)) {
			properties |= MorphemeProperties.Irregular;
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

		if ((properties & MorphemeProperties.Irregular) == MorphemeProperties.Irregular) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsIrregular";
			multipleProperties = true;
		}

		if ((properties & MorphemeProperties.Uncountable) == MorphemeProperties.Uncountable) {

			if (multipleProperties) {
				output += " | ";
			}

			output += "IsUncountable";
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

	public void GenerateIndicativeProperties (GetRandomFloatDelegate getRandomFloat) {

		if (getRandomFloat () < 0.15f) {

			_indicativeProperties |= GeneralIndicativeProperties.HasDefiniteIndicative;
		}

		if (getRandomFloat () < 0.10f) {

			_indicativeProperties |= GeneralIndicativeProperties.HasIndefiniteIndicative;
		}

		if (getRandomFloat () < 0.05f) {

			_indicativeProperties |= GeneralIndicativeProperties.HasUncountableIndicative;
		}

		if (getRandomFloat () < 0.15f) {

			_indicativeProperties |= GeneralIndicativeProperties.HasMasculineIndicative;
		}

		if (getRandomFloat () < 0.15f) {

			_indicativeProperties |= GeneralIndicativeProperties.HasNeutralIndicative;
		}

		if (getRandomFloat () < 0.15f) {

			_indicativeProperties |= GeneralIndicativeProperties.HasFemenineIndicative;
		}

		if (getRandomFloat () < 0.10f) {

			_indicativeProperties |= GeneralIndicativeProperties.HasSingularIndicative;
		}

		if (getRandomFloat () < 0.20f) {

			_indicativeProperties |= GeneralIndicativeProperties.HasPluralIndicative;
		}
	}

	public void GenerateArticleAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		ArticleAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat);
	}

	public void GenerateArticleSyllables (GetRandomFloatDelegate getRandomFloat) {

		ArticleSyllables.OnsetChance = 0.5f;
		ArticleSyllables.OnsetGroupCount = 10;

		ArticleSyllables.NucleusChance = 1.0f;
		ArticleSyllables.NucleusGroupCount = 5;

		ArticleSyllables.CodaChance = 0.5f;
		ArticleSyllables.CodaGroupCount = 4;

		ArticleSyllables.GenerateCharacterGroups (getRandomFloat);

//		ArticleNextSyllables.OnsetStartAddLetterChance = 0.7f;
//		ArticleNextSyllables.OnsetGroupCount = 10;
//
//		ArticleNextSyllables.NucleusStartAddLetterChance = 1.0f;
//		ArticleNextSyllables.NucleusGroupCount = 5;
//
//		ArticleNextSyllables.CodaStartAddLetterChance = 0.25f;
//		ArticleNextSyllables.CodaGroupCount = 4;
//
//		ArticleNextSyllables.GenerateCharacterGroups (getRandomFloat);

		DerivativeArticleStartSyllables.OnsetChance = 0.5f;
		DerivativeArticleStartSyllables.OnsetGroupCount = 10;

		DerivativeArticleStartSyllables.NucleusChance = 1.0f;
		DerivativeArticleStartSyllables.NucleusGroupCount = 5;

		DerivativeArticleStartSyllables.CodaChance = 0.05f;
		DerivativeArticleStartSyllables.CodaGroupCount = 4;

		DerivativeArticleStartSyllables.GenerateCharacterGroups (getRandomFloat);

		DerivativeArticleNextSyllables.OnsetChance = 0.05f;
		DerivativeArticleNextSyllables.OnsetGroupCount = 10;

		DerivativeArticleNextSyllables.NucleusChance = 1.0f;
		DerivativeArticleNextSyllables.NucleusGroupCount = 5;

		DerivativeArticleNextSyllables.CodaChance = 0.5f;
		DerivativeArticleNextSyllables.CodaGroupCount = 4;

		DerivativeArticleNextSyllables.GenerateCharacterGroups (getRandomFloat);
	}

	public void GenerateAllArticles (GetRandomFloatDelegate getRandomFloat) {

//		_articles = GenerateArticles (ArticleStartSyllables, ArticleNextSyllables, DerivativeArticleStartSyllables, DerivativeArticleNextSyllables, _articleProperties, getRandomFloat);
		_articles = GenerateArticles (ArticleSyllables, DerivativeArticleStartSyllables, DerivativeArticleNextSyllables, _articleProperties, getRandomFloat);

		Articles = new List<Morpheme> (_articles.Count);

		foreach (KeyValuePair<string, Morpheme> pair in _articles) {
		
			Articles.Add (pair.Value);
		}
	}

	public void GenerateIndicativeAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		IndicativeAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat);
	}

	public void GenerateIndicativeSyllables (GetRandomFloatDelegate getRandomFloat) {

		IndicativeStartSyllables.OnsetChance = 0.7f;
		IndicativeStartSyllables.OnsetGroupCount = 10;

		IndicativeStartSyllables.NucleusChance = 1.0f;
		IndicativeStartSyllables.NucleusGroupCount = 5;

		IndicativeStartSyllables.CodaChance = 0.25f;
		IndicativeStartSyllables.CodaGroupCount = 4;

		IndicativeStartSyllables.GenerateCharacterGroups (getRandomFloat);

		IndicativeNextSyllables.OnsetChance = 0.7f;
		IndicativeNextSyllables.OnsetGroupCount = 10;

		IndicativeNextSyllables.NucleusChance = 1.0f;
		IndicativeNextSyllables.NucleusGroupCount = 5;

		IndicativeNextSyllables.CodaChance = 0.25f;
		IndicativeNextSyllables.CodaGroupCount = 4;

		IndicativeNextSyllables.GenerateCharacterGroups (getRandomFloat);
	}

	public static Morpheme GenerateIndicative (SyllableSet startSyllables, SyllableSet nextSyllables, MorphemeProperties properties, GetRandomFloatDelegate getRandomFloat) {

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = GenerateMorpheme (startSyllables, nextSyllables, 0.0f, getRandomFloat);
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
		SyllableSet startSyllables, 
		SyllableSet nextSyllables, 
		MorphemeProperties properties, 
		GetRandomFloatDelegate getRandomFloat) {

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = GenerateDerivatedWord (rootIndicative.Value, 0.0f, 0.5f, startSyllables, nextSyllables, startSyllables, nextSyllables, 0.0f, getRandomFloat);
		morpheme.Properties = rootIndicative.Properties | properties;
		morpheme.Type = WordType.Indicative;

		return morpheme;
	}

	public static void GenerateGenderedIndicatives (
		Morpheme root,
		SyllableSet startSyllables, 
		SyllableSet nextSyllables,
		GeneralIndicativeProperties indicativeProperties, 
		GetRandomFloatDelegate getRandomFloat, 
		out Morpheme masculine, 
		out Morpheme femenine,
		out Morpheme neutral) {

		if ((indicativeProperties & GeneralIndicativeProperties.HasMasculineIndicative) == GeneralIndicativeProperties.HasMasculineIndicative)
			masculine = GenerateDerivatedIndicative (root, startSyllables, nextSyllables, MorphemeProperties.None, getRandomFloat);
		else
			masculine = CopyMorpheme (root);

		if ((indicativeProperties & GeneralIndicativeProperties.HasFemenineIndicative) == GeneralIndicativeProperties.HasFemenineIndicative)
			femenine = GenerateDerivatedIndicative (root, startSyllables, nextSyllables, MorphemeProperties.None, getRandomFloat);
		else
			femenine = CopyMorpheme (root);

		if ((indicativeProperties & GeneralIndicativeProperties.HasNeutralIndicative) == GeneralIndicativeProperties.HasNeutralIndicative)
			neutral = GenerateDerivatedIndicative (root, startSyllables, nextSyllables, MorphemeProperties.None, getRandomFloat);
		else
			neutral = CopyMorpheme (root);

		femenine.Properties |= MorphemeProperties.Femenine;
		neutral.Properties |= MorphemeProperties.Neutral;
	}

	public static Dictionary<string, Morpheme> GenerateIndicatives (
		SyllableSet startSyllables, 
		SyllableSet nextSyllables, 
		GeneralIndicativeProperties indicativeProperties, 
		GetRandomFloatDelegate getRandomFloat) {

		Dictionary<string, Morpheme> indicatives = new Dictionary<string, Morpheme> ();

		Morpheme definite;
		if ((indicativeProperties & GeneralIndicativeProperties.HasDefiniteIndicative) == GeneralIndicativeProperties.HasDefiniteIndicative)
			definite = GenerateIndicative (startSyllables, nextSyllables, MorphemeProperties.None, getRandomFloat);
		else
			definite = GenerateNullWord (WordType.Indicative);

		Morpheme indefinite;
		if ((indicativeProperties & GeneralIndicativeProperties.HasIndefiniteIndicative) == GeneralIndicativeProperties.HasIndefiniteIndicative)
			indefinite = GenerateIndicative (startSyllables, nextSyllables, MorphemeProperties.Indefinite, getRandomFloat);
		else
			indefinite = GenerateNullWord (WordType.Indicative, MorphemeProperties.Indefinite);

		Morpheme uncountable;
		if ((indicativeProperties & GeneralIndicativeProperties.HasUncountableIndicative) == GeneralIndicativeProperties.HasUncountableIndicative)
			uncountable = GenerateIndicative (startSyllables, nextSyllables, MorphemeProperties.Uncountable, getRandomFloat);
		else
			uncountable = GenerateNullWord (WordType.Indicative, MorphemeProperties.Uncountable);

		///

		Morpheme definiteSingular;
		if ((indicativeProperties & GeneralIndicativeProperties.HasSingularIndicative) == GeneralIndicativeProperties.HasSingularIndicative)
			definiteSingular = GenerateDerivatedIndicative (definite, startSyllables, nextSyllables, MorphemeProperties.None, getRandomFloat);
		else
			definiteSingular = CopyMorpheme (definite);

		Morpheme femenine, masculine, neutral;
		GenerateGenderedIndicatives (definiteSingular, startSyllables, nextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.DefiniteSingularFemenine;
		masculine.Meaning = IndicativeType.DefiniteSingularMasculine;
		neutral.Meaning = IndicativeType.DefiniteSingularNeutral;

		indicatives.Add (IndicativeType.DefiniteSingularFemenine, femenine);
		indicatives.Add (IndicativeType.DefiniteSingularMasculine, masculine);
		indicatives.Add (IndicativeType.DefiniteSingularNeutral, neutral);

		///

		Morpheme definitePlural;
		if ((indicativeProperties & GeneralIndicativeProperties.HasPluralIndicative) == GeneralIndicativeProperties.HasPluralIndicative)
			definitePlural = GenerateDerivatedIndicative (definite, startSyllables, nextSyllables, MorphemeProperties.Plural, getRandomFloat);
		else {
			definitePlural = CopyMorpheme (definite);
			definitePlural.Properties |= MorphemeProperties.Plural;
		}

		GenerateGenderedIndicatives (definitePlural, startSyllables, nextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.DefinitePluralFemenine;
		masculine.Meaning = IndicativeType.DefinitePluralMasculine;
		neutral.Meaning = IndicativeType.DefinitePluralNeutral;

		indicatives.Add (IndicativeType.DefinitePluralFemenine, femenine);
		indicatives.Add (IndicativeType.DefinitePluralMasculine, masculine);
		indicatives.Add (IndicativeType.DefinitePluralNeutral, neutral);

		///

		Morpheme indefiniteSingular;
		if ((indicativeProperties & GeneralIndicativeProperties.HasSingularIndicative) == GeneralIndicativeProperties.HasSingularIndicative)
			indefiniteSingular = GenerateDerivatedIndicative (indefinite, startSyllables, nextSyllables, MorphemeProperties.None, getRandomFloat);
		else
			indefiniteSingular = CopyMorpheme (indefinite);

		GenerateGenderedIndicatives (indefiniteSingular, startSyllables, nextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.IndefiniteSingularFemenine;
		masculine.Meaning = IndicativeType.IndefiniteSingularMasculine;
		neutral.Meaning = IndicativeType.IndefiniteSingularNeutral;

		indicatives.Add (IndicativeType.IndefiniteSingularFemenine, femenine);
		indicatives.Add (IndicativeType.IndefiniteSingularMasculine, masculine);
		indicatives.Add (IndicativeType.IndefiniteSingularNeutral, neutral);

		///

		Morpheme indefinitePlural;
		if ((indicativeProperties & GeneralIndicativeProperties.HasPluralIndicative) == GeneralIndicativeProperties.HasPluralIndicative)
			indefinitePlural = GenerateDerivatedIndicative (indefinite, startSyllables, nextSyllables, MorphemeProperties.Plural, getRandomFloat);
		else {
			indefinitePlural = CopyMorpheme (indefinite);
			indefinitePlural.Properties |= MorphemeProperties.Plural;
		}

		GenerateGenderedIndicatives (indefinitePlural, startSyllables, nextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.IndefinitePluralFemenine;
		masculine.Meaning = IndicativeType.IndefinitePluralMasculine;
		neutral.Meaning = IndicativeType.IndefinitePluralNeutral;

		indicatives.Add (IndicativeType.IndefinitePluralFemenine, femenine);
		indicatives.Add (IndicativeType.IndefinitePluralMasculine, masculine);
		indicatives.Add (IndicativeType.IndefinitePluralNeutral, neutral);

		///

		GenerateGenderedIndicatives (uncountable, startSyllables, nextSyllables, indicativeProperties, getRandomFloat, out masculine, out femenine, out neutral);
		femenine.Meaning = IndicativeType.UncountableFemenine;
		masculine.Meaning = IndicativeType.UncountableMasculine;
		neutral.Meaning = IndicativeType.UncountableNeutral;

		indicatives.Add (IndicativeType.UncountableFemenine, femenine);
		indicatives.Add (IndicativeType.UncountableMasculine, masculine);
		indicatives.Add (IndicativeType.UncountableNeutral, neutral);

		return indicatives;
	}

	public void GenerateAllIndicatives (GetRandomFloatDelegate getRandomFloat) {

		_indicatives = GenerateIndicatives (IndicativeStartSyllables, IndicativeNextSyllables, _indicativeProperties, getRandomFloat);

		Indicatives = new List<Morpheme> (_indicatives.Count);

		foreach (KeyValuePair<string, Morpheme> pair in _indicatives) {

			Indicatives.Add (pair.Value);
		}
	}

	public void GenerateAdpositionAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		AdpositionAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat);
	}

	public void GenerateAdpositionSyllables (GetRandomFloatDelegate getRandomFloat) {
//
//		CharacterGroup[] onsetGroups = GenerateCharacterGroups (OnsetLetters, 0.7f, 0.1f, getRandomFloat, 20);
//		CharacterGroup[] nucleusGroups = GenerateCharacterGroups (NucleusLetters, 1.0f, 0.35f, getRandomFloat, 10);
//		CharacterGroup[] codaGroups = GenerateCharacterGroups (CodaLetters, 0.25f, 0.1f, getRandomFloat, 8);
//
//		AdpositionStartSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 100);
//		AdpositionNextSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 100);

		AdpositionStartSyllables.OnsetChance = 0.7f;
//		AdpositionStartSyllables.OnsetAddLetterChanceDecay = 0.1f;
		AdpositionStartSyllables.OnsetGroupCount = 20;

		AdpositionStartSyllables.NucleusChance = 1.0f;
//		AdpositionStartSyllables.NucleusAddLetterChanceDecay = 0.35f;
		AdpositionStartSyllables.NucleusGroupCount = 10;

		AdpositionStartSyllables.CodaChance = 0.25f;
//		AdpositionStartSyllables.CodaAddLetterChanceDecay = 0.1f;
		AdpositionStartSyllables.CodaGroupCount = 8;

		AdpositionStartSyllables.GenerateCharacterGroups (getRandomFloat);

		AdpositionNextSyllables.OnsetChance = 0.7f;
//		AdpositionNextSyllables.OnsetAddLetterChanceDecay = 0.1f;
		AdpositionNextSyllables.OnsetGroupCount = 20;

		AdpositionNextSyllables.NucleusChance = 1.0f;
//		AdpositionNextSyllables.NucleusAddLetterChanceDecay = 0.35f;
		AdpositionNextSyllables.NucleusGroupCount = 10;

		AdpositionNextSyllables.CodaChance = 0.25f;
//		AdpositionNextSyllables.CodaAddLetterChanceDecay = 0.1f;
		AdpositionNextSyllables.CodaGroupCount = 8;

		AdpositionNextSyllables.GenerateCharacterGroups (getRandomFloat);
	}

	public void GenerateAdposition (string relation, GetRandomFloatDelegate getRandomFloat) {

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = GenerateMorpheme (AdpositionStartSyllables, AdpositionNextSyllables, 0.5f, getRandomFloat);
		morpheme.Properties = MorphemeProperties.None;
		morpheme.Type = WordType.Adposition;
		morpheme.Meaning = relation;

		_adpositions.Add (relation, morpheme);

		Adpositions.Add (morpheme);
	}

	public void GenerateAdjectiveAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		AdjectiveAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat);
	}

	public void GenerateAdjectiveSyllables (GetRandomFloatDelegate getRandomFloat) {
//
//		CharacterGroup[] onsetGroups = GenerateCharacterGroups (OnsetLetters, 0.7f, 0.1f, getRandomFloat, 20);
//		CharacterGroup[] nucleusGroups = GenerateCharacterGroups (NucleusLetters, 1.0f, 0.35f, getRandomFloat, 10);
//		CharacterGroup[] codaGroups = GenerateCharacterGroups (CodaLetters, 0.25f, 0.1f, getRandomFloat, 8);
//
//		AdjectiveStartSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 100);
//		AdjectiveNextSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 100);

		AdjectiveStartSyllables.OnsetChance = 0.7f;
//		AdjectiveStartSyllables.OnsetAddLetterChanceDecay = 0.1f;
		AdjectiveStartSyllables.OnsetGroupCount = 20;

		AdjectiveStartSyllables.NucleusChance = 1.0f;
//		AdjectiveStartSyllables.NucleusAddLetterChanceDecay = 0.35f;
		AdjectiveStartSyllables.NucleusGroupCount = 10;

		AdjectiveStartSyllables.CodaChance = 0.25f;
//		AdjectiveStartSyllables.CodaAddLetterChanceDecay = 0.1f;
		AdjectiveStartSyllables.CodaGroupCount = 8;

		AdjectiveStartSyllables.GenerateCharacterGroups (getRandomFloat);

		AdjectiveNextSyllables.OnsetChance = 0.7f;
//		AdjectiveNextSyllables.OnsetAddLetterChanceDecay = 0.1f;
		AdjectiveNextSyllables.OnsetGroupCount = 20;

		AdjectiveNextSyllables.NucleusChance = 1.0f;
//		AdjectiveNextSyllables.NucleusAddLetterChanceDecay = 0.35f;
		AdjectiveNextSyllables.NucleusGroupCount = 10;

		AdjectiveNextSyllables.CodaChance = 0.25f;
//		AdjectiveNextSyllables.CodaAddLetterChanceDecay = 0.1f;
		AdjectiveNextSyllables.CodaGroupCount = 8;

		AdjectiveNextSyllables.GenerateCharacterGroups (getRandomFloat);
	}

	public Morpheme GenerateAdjective (string meaning, GetRandomFloatDelegate getRandomFloat) {

		if (_adjectives.ContainsKey (meaning)) {
		
			return _adjectives [meaning];
		}

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = GenerateMorpheme (AdjectiveStartSyllables, AdjectiveNextSyllables, 0.5f, getRandomFloat);
		morpheme.Properties = MorphemeProperties.None;
		morpheme.Type = WordType.Adjective;
		morpheme.Meaning = meaning;

		_adjectives.Add (meaning, morpheme);

		Adjectives.Add (morpheme);

		return morpheme;
	}

	public void GenerateNounAdjunctionProperties (GetRandomFloatDelegate getRandomFloat) {

		NounAdjunctionProperties = GenerateAdjunctionProperties (getRandomFloat);
	}

	public void GenerateNounSyllables (GetRandomFloatDelegate getRandomFloat) {
//
//		CharacterGroup[] onsetGroups = GenerateCharacterGroups (OnsetLetters, 0.7f, 0.25f, getRandomFloat, 20);
//		CharacterGroup[] nucleusGroups = GenerateCharacterGroups (NucleusLetters, 1.0f, 0.35f, getRandomFloat, 10);
//		CharacterGroup[] codaGroups = GenerateCharacterGroups (CodaLetters, 0.5f, 0.25f, getRandomFloat, 8);
//
//		NounStartSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 200);
//		NounNextSyllables = GenerateSyllables (onsetGroups, nucleusGroups, codaGroups, getRandomFloat, 200);

		NounStartSyllables.OnsetChance = 0.7f;
//		NounStartSyllables.OnsetAddLetterChanceDecay = 0.25f;
		NounStartSyllables.OnsetGroupCount = 20;

		NounStartSyllables.NucleusChance = 1.0f;
//		NounStartSyllables.NucleusAddLetterChanceDecay = 0.35f;
		NounStartSyllables.NucleusGroupCount = 10;

		NounStartSyllables.CodaChance = 0.5f;
//		NounStartSyllables.CodaAddLetterChanceDecay = 0.25f;
		NounStartSyllables.CodaGroupCount = 8;

		NounStartSyllables.GenerateCharacterGroups (getRandomFloat);

		NounNextSyllables.OnsetChance = 0.7f;
//		NounNextSyllables.OnsetAddLetterChanceDecay = 0.25f;
		NounNextSyllables.OnsetGroupCount = 20;

		NounNextSyllables.NucleusChance = 1.0f;
//		NounNextSyllables.NucleusAddLetterChanceDecay = 0.35f;
		NounNextSyllables.NucleusGroupCount = 10;

		NounNextSyllables.CodaChance = 0.5f;
//		NounNextSyllables.CodaAddLetterChanceDecay = 0.25f;
		NounNextSyllables.CodaGroupCount = 8;

		NounNextSyllables.GenerateCharacterGroups (getRandomFloat);
	}

	public Morpheme GenerateNoun (string meaning, GetRandomFloatDelegate getRandomFloat, bool isPlural, bool randomGender, bool isFemenine = false, bool isNeutral = false, bool canBeIrregular = true) {

		return GenerateNoun (meaning, GenerateWordProperties (getRandomFloat, isPlural, randomGender, isFemenine, isNeutral, canBeIrregular), getRandomFloat);
	}

	public Morpheme GenerateNoun (string meaning, MorphemeProperties properties, GetRandomFloatDelegate getRandomFloat) {

		if (_nouns.ContainsKey (meaning)) {
		
			return _nouns [meaning];
		}

		Morpheme morpheme = new Morpheme ();
		morpheme.Value = GenerateMorpheme (NounStartSyllables, NounNextSyllables, 0.5f, getRandomFloat);
		morpheme.Properties = properties;
		morpheme.Type = WordType.Noun;
		morpheme.Meaning = meaning;

		_nouns.Add (meaning, morpheme);

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

	public Phrase BuildAdpositionalPhrase (string relation, Phrase complementPhrase) {

		Phrase phrase = new Phrase ();

		Morpheme adposition = null;

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

	public Morpheme GetAppropiateIndicative (PhraseProperties phraseProperties) {

		Morpheme indicative = null;

		if ((phraseProperties & PhraseProperties.Uncountable) == PhraseProperties.Uncountable) {
			
			if ((phraseProperties & PhraseProperties.Femenine) == PhraseProperties.Femenine) {

				indicative = _indicatives [IndicativeType.UncountableFemenine];

			} else if ((phraseProperties & PhraseProperties.Neutral) == PhraseProperties.Neutral) {

				indicative = _indicatives [IndicativeType.UncountableNeutral];

			} else {

				indicative = _indicatives [IndicativeType.UncountableMasculine];

			}
		} else if ((phraseProperties & PhraseProperties.Indefinite) == PhraseProperties.Indefinite) {
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

			if (!parsedPhrasePart.Attributes.ContainsKey (ParsedWordAttributeId.UncountableNoun) && (absentArticle)) {
				phraseProperties |= PhraseProperties.Indefinite;
			}

			if (parsedPhrasePart.Attributes.ContainsKey (ParsedWordAttributeId.NounAdjunct)) {
				
				nounAdjunctionPhrases.Add (TranslateNoun (phrasePart, phraseProperties, getRandomFloat));

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

		foreach (NounPhrase nounAdjunctionPhrase in nounAdjunctionPhrases) {

			nounPhrase.Text = AddAdjunctionToNounPhrase (nounPhrase.Text, nounAdjunctionPhrase.Text, NounAdjunctionProperties);
		}

		foreach (Morpheme adjective in adjectives) {

			nounPhrase.Text = AddAdjunctionToNounPhrase (nounPhrase.Text, adjective.Value, AdjectiveAdjunctionProperties);
		}

		Morpheme article = GetAppropiateArticle (phraseProperties);

		if (article != null) {
			nounPhrase.Text = AddAdjunctionToNounPhrase (nounPhrase.Text, article.Value, ArticleAdjunctionProperties);
		}

		nounPhrase.Original = untranslatedNounPhrase;
		nounPhrase.Meaning = ClearConstructCharacters (untranslatedNounPhrase);

		return nounPhrase;
	}

	public NounPhrase TranslateNoun (string untranslatedNoun, PhraseProperties properties, GetRandomFloatDelegate getRandomFloat) {

		string[] nounParts = untranslatedNoun.Split (new char[] { ':' });

		Morpheme mainNoun = null;

		List<Morpheme> nounComponents = new List<Morpheme> ();

		bool isPlural = false;
		bool hasRandomGender = true;
		bool isFemenineNoun = false;
		bool isNeutralNoun = false;
		bool isUncountableNoun = false;

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

				Morpheme noun = null;

				string singularNoun = parsedWordPart.Value;
				if (parsedWordPart.Attributes.ContainsKey (ParsedWordAttributeId.IrregularPluralNoun)) {
					singularNoun = parsedWordPart.Attributes[ParsedWordAttributeId.IrregularPluralNoun];
					isPlural = true;
				}
				
				noun = GenerateNoun (singularNoun, getRandomFloat, false, hasRandomGender, isFemenineNoun, isNeutralNoun);

				isFemenineNoun = ((noun.Properties & MorphemeProperties.Femenine) == MorphemeProperties.Femenine);
				isNeutralNoun = ((noun.Properties & MorphemeProperties.Neutral) == MorphemeProperties.Neutral);
				hasRandomGender = false;

				if (mainNoun != null) {
					nounComponents.Add (mainNoun);
				}

				mainNoun = noun;
			}
		}

		if (isPlural)
			properties |= PhraseProperties.Plural;

		if (isFemenineNoun)
			properties |= PhraseProperties.Femenine;
		else if (isNeutralNoun)
			properties |= PhraseProperties.Neutral;

		if (isUncountableNoun)
			properties |= PhraseProperties.Uncountable;

		string text = mainNoun.Value;

		foreach (Morpheme nounComponent in nounComponents) {
			text = AddAdjunctionToNounPhrase (text, nounComponent.Value, NounAdjunctionProperties, true);
		}

		Morpheme indicative = GetAppropiateIndicative (properties);

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

		ArticlePropertiesInt = (int)_articleProperties;
		IndicativePropertiesInt = (int)_indicativeProperties;

		ArticleAdjunctionPropertiesInt = (int)ArticleAdjunctionProperties;
		IndicativeAdjunctionPropertiesInt = (int)IndicativeAdjunctionProperties;
		AdpositionAdjunctionPropertiesInt = (int)AdpositionAdjunctionProperties;
		AdjectiveAdjunctionPropertiesInt = (int)AdjectiveAdjunctionProperties;
		NounAdjunctionPropertiesInt = (int)NounAdjunctionProperties;

		foreach (Morpheme morpheme in Articles) {
			morpheme.Synchronize ();
		}

		foreach (Morpheme morpheme in Indicatives) {
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
	}

	public void FinalizeLoad () {

		_articleProperties = (GeneralArticleProperties)ArticlePropertiesInt;
		_indicativeProperties = (GeneralIndicativeProperties)IndicativePropertiesInt;

		ArticleAdjunctionProperties = (AdjunctionProperties)ArticleAdjunctionPropertiesInt;
		IndicativeAdjunctionProperties = (AdjunctionProperties)IndicativeAdjunctionPropertiesInt;
		AdpositionAdjunctionProperties = (AdjunctionProperties)AdpositionAdjunctionPropertiesInt;
		AdjectiveAdjunctionProperties = (AdjunctionProperties)AdjectiveAdjunctionPropertiesInt;
		NounAdjunctionProperties = (AdjunctionProperties)NounAdjunctionPropertiesInt;

		_articles = new Dictionary<string, Morpheme> (Articles.Count);
		_indicatives = new Dictionary<string, Morpheme> (Indicatives.Count);

		// initialize dictionaries

		foreach (Morpheme word in Articles) {
			_articles.Add (word.Meaning, word);
		}

		foreach (Morpheme word in Indicatives) {
			_indicatives.Add (word.Meaning, word);
		}

		foreach (Morpheme word in Adpositions) {
			_adpositions.Add (word.Meaning, word);
		}

		foreach (Morpheme word in Adjectives) {
			_adjectives.Add (word.Meaning, word);
		}

		foreach (Morpheme word in Nouns) {
			_nouns.Add (word.Meaning, word);
		}

		// Finish loading morphemes

		foreach (Morpheme morpheme in Articles) {
			morpheme.FinalizeLoad ();
		}

		foreach (Morpheme morpheme in Indicatives) {
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
