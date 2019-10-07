using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

public class Letter : CollectionUtility.ElementWeightPair<string>
{
    public Letter(string letter, float weight) : base(letter, weight)
    {
    }
}

public class SyllableSet
{
    public const int MaxNumberOfSyllables = 500;

    // based on frequency of consonants across languages. source: http://phoible.org/
    private static readonly Letter[] OnsetLetters = new Letter[] {
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
    private static readonly Letter[] NucleusLetters = new Letter[] {
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
    private static readonly Letter[] CodaLetters = new Letter[] {
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

    [XmlAttribute("OSALC")]
    public float OnsetChance;

    [XmlAttribute("NSALC")]
    public float NucleusChance;

    [XmlAttribute("CSALC")]
    public float CodaChance;

    private Dictionary<int, string> _syllables = new Dictionary<int, string>();

    public SyllableSet()
    {
    }

    public string GetRandomSyllable(GetRandomFloatDelegate getRandomFloat)
    {
        float randValue = getRandomFloat();
        randValue *= randValue; // Emulate a Zipf's Distribution
        int randOption = (int)Mathf.Floor(MaxNumberOfSyllables * randValue);

        if (_syllables.ContainsKey(randOption))
        {
            return _syllables[randOption];
        }
        else
        {
            string syllable = GenerateSyllable(OnsetLetters, OnsetChance, NucleusLetters, NucleusChance, CodaLetters, CodaChance, getRandomFloat);

            _syllables.Add(randOption, syllable);

            return syllable;
        }
    }

    private static float GetLettersTotalWeight(Letter[] letters)
    {
        float totalWeight = 0;

        foreach (Letter letter in letters)
        {
            totalWeight += letter.Weight;
        }

        return totalWeight;
    }

    private static string GenerateSyllable(
        Letter[] onsetLetters,
        float onsetChance,
        Letter[] nucleusLetters,
        float nucleusChance,
        Letter[] codaLetters,
        float codaChance,
        GetRandomFloatDelegate getRandomFloat)
    {
        string onset = (onsetChance > getRandomFloat()) ? CollectionUtility.WeightedSelection(onsetLetters, GetLettersTotalWeight(onsetLetters), getRandomFloat()) : string.Empty;
        string nucleus = (nucleusChance > getRandomFloat()) ? CollectionUtility.WeightedSelection(nucleusLetters, GetLettersTotalWeight(nucleusLetters), getRandomFloat()) : string.Empty;
        string coda = (codaChance > getRandomFloat()) ? CollectionUtility.WeightedSelection(codaLetters, GetLettersTotalWeight(codaLetters), getRandomFloat()) : string.Empty;

        if (nucleus == string.Empty)
        {
            return coda;
        }

        return onset + nucleus + coda;
    }
}
