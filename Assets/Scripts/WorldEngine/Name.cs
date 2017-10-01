using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Linq;

public static class NamingTools {

	public static Regex OptionalWordPartRegex = new Regex (@"\{(?<word>.+?)\}");
	public static Regex NameRankRegex = new Regex (@"\[\[(?<rank>.+?)\]\](?<word>.+?)");
}

public class Name : ISynchronizable {

	[XmlAttribute]
	public long LanguageId;

	public Language.NounPhrase Value;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public Language Language;

	public Name () {
		
	}

	public Name (Language.NounPhrase value, string meaning, Language language, World world) {

		World = world;

		LanguageId = language.Id;
		Language = language;

		Value = value;

		Language.TurnIntoProperName (Value);
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
