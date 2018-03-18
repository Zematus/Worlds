using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Linq;

public class Variation {

	public string Tags;

	public string Text;

	public Variation (string text) {

		Text = text;
		Tags = string.Empty;
	}

	public Variation (string text, string tags) {

		Text = text;
		Tags = tags;
	}
}

public static class NamingTools {

	public static Regex OptionalWordPartRegex = new Regex (@"\{(?:\<(?<tags>\w+)\>)?(?<word>.+?)\}");
	public static Regex NameRankRegex = new Regex (@"\[\[(?<rank>.+?)\]\](?<word>.+?)");

	public static Variation[] GenerateNounVariations (string[] variants) {

		List<Variation> variations = new List<Variation> ();

		foreach (string variant in variants) {

			GenerateNounVariations (variant, variations);
		}

		return variations.ToArray ();
	}

	public static void GenerateNounVariations (string variant, List<Variation> variations) {

		Match match = NamingTools.OptionalWordPartRegex.Match (variant);

		if (!match.Success) {

			variations.Add (new Variation (variant));
			return;
		}

		string v1Str = variant.Replace (match.Value, string.Empty);
		string v2Str = variant.Replace (match.Value, match.Groups ["word"].Value);

		Variation v1 = new Variation (v1Str);
		Variation v2 = new Variation (v2Str);

		if (match.Groups ["tags"].Success) {
			if (string.IsNullOrEmpty(v2.Tags))
				v2.Tags = match.Groups ["tags"].Value;
			else
				v2.Tags += "," + match.Groups ["tags"].Value;
		}

		GenerateNounVariations (v1, variations);
		GenerateNounVariations (v2, variations);
	}

	public static void GenerateNounVariations (Variation variation, List<Variation> variations) {

		Match match = NamingTools.OptionalWordPartRegex.Match (variation.Text);

		if (!match.Success) {

			variations.Add (variation);
			return;
		}

		string v1Str = variation.Text.Replace (match.Value, string.Empty);
		string v2Str = variation.Text.Replace (match.Value, match.Groups ["word"].Value);

		Variation v1 = new Variation (v1Str, variation.Tags);
		Variation v2 = new Variation (v2Str, variation.Tags);

		if (match.Groups ["tags"].Success) {
			if (string.IsNullOrEmpty(v2.Tags))
				v2.Tags = match.Groups ["tags"].Value;
			else
				v2.Tags += "," + match.Groups ["tags"].Value;
		}

		GenerateNounVariations (v1, variations);
		GenerateNounVariations (v2, variations);
	}
}

public class Name : ISynchronizable {

	[XmlAttribute]
	public long LanguageId;

	public Language.Phrase Value;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public Language Language;

	public Name () {
		
	}

	public Name (Language.Phrase value, string meaning, Language language, World world) {

		World = world;

		LanguageId = language.Id;
		Language = language;

		Value = value;

//		Language.TurnIntoProperName (Value, false);
	}

	public string BoldText {
		get { return "<b>" + Value.Text + "</b>"; }
	}

	public string Text {
		get { return Value.Text; }
	}

	public string Meaning {
		get { return Value.Meaning; }
	}

	public void Synchronize () {

		Value.Synchronize ();
	}

	public void FinalizeLoad () {

		Language = World.GetLanguage (LanguageId);

		if (Language == null) {
		
			throw new System.Exception ("Language can't be null");
		}

		Value.FinalizeLoad ();
	}

	public override string ToString () {
	
		return Value.Text + " (" + Value.Meaning + ")";
	}
}
