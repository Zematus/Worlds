using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

public class Agent : ISynchronizable {

	public const int MaxLifespan = 113; // Prime number to hide birthdate cycle artifacts

	[XmlAttribute]
	public long Id;

	[XmlAttribute("Birth")]
	public int BirthDate;

	[XmlAttribute]
	public bool IsFemale;

	[XmlAttribute("GrpId")]
	public long GroupId;

	[XmlAttribute("StilPres")]
	public bool StillPresent = true;

	public Name Name = null;

	[XmlIgnore]
	public World World;

	[XmlIgnore]
	public CellGroup Group;

	public Agent () {

	}

	public Agent (CellGroup birthGroup, int birthDate) {

		World = birthGroup.World;

		GroupId = birthGroup.Id;
		Group = birthGroup;

		BirthDate = birthDate;

		int idOffset = 0;

		Id = GenerateUniqueIdentifier (birthDate, offset: idOffset);

		GenerateBio ();

		GenerateName ();
	}

	public void Destroy () {

		StillPresent = false;
	}

	public long GenerateUniqueIdentifier (int date, long oom = 1, long offset = 0) {

		return Group.GenerateUniqueIdentifier (date, oom, offset);
	}

	private void GenerateBio () {

		int rngOffset = RngOffsets.AGENT_GENERATE_BIO + (int)Group.Id;

//		GetRandomIntDelegate getRandomInt = (int maxValue) => Group.GetLocalRandomInt (BirthDate, rngOffset++, maxValue);
		Language.GetRandomFloatDelegate getRandomFloat = () => Group.GetLocalRandomFloat (BirthDate, rngOffset++);

		IsFemale = getRandomFloat () > 0.5f;
	}

	private void GenerateName () {

		int rngOffset = RngOffsets.AGENT_GENERATE_NAME + (int)Group.Id;

		GetRandomIntDelegate getRandomInt = (int maxValue) => Group.GetLocalRandomInt (BirthDate, rngOffset++, maxValue);
		Language.GetRandomFloatDelegate getRandomFloat = () => Group.GetLocalRandomFloat (BirthDate, rngOffset++);

		Language language = Group.Culture.Language;
		Region region = Group.Cell.Region;

		string untranslatedName = "";
		Language.Phrase namePhrase = null;

		if (region.Elements.Count <= 0) {

			throw new System.Exception ("No elements to choose name from");
		}

		List<Element> remainingElements = new List<Element> (region.Elements.Where (e => e.Associations.Length > 0));

		if (remainingElements.Count <= 0) {

			throw new System.Exception ("No elements to choose name from");
		}

		Element element = remainingElements.RandomSelectAndRemove (getRandomInt);

		string adjective = element.Adjectives.RandomSelect (getRandomInt, 15);

		if (!string.IsNullOrEmpty (adjective)) {
			adjective = "[adj]" + adjective + " ";
		}

		Association association = element.Associations.RandomSelect (getRandomInt);

		string nounGender = (IsFemale) ? "[fn]" : "[mn]";

		string subjectNoun = "[name]" + nounGender + association.Noun;

		if (association.IsAdjunction) {
			untranslatedName = "[Proper][NP](" + adjective + "[nad]" + element.SingularName + " " + subjectNoun + ")";

		} else {

			string article = "";

			if ((association.Form == AssociationForms.DefiniteSingular) ||
			    (association.Form == AssociationForms.DefinitePlural) ||
			    (association.Form == AssociationForms.NameSingular)) {
			
				article = "the ";
			}

			string uncountableProp = (association.Form == AssociationForms.Uncountable) ? "[un]" : "";

			string elementNoun = element.SingularName;

			if ((association.Form == AssociationForms.DefinitePlural) ||
				(association.Form == AssociationForms.IndefinitePlural)) {

				elementNoun = element.PluralName;
			}

			elementNoun = article + adjective + uncountableProp + elementNoun;

			untranslatedName = "[PpPP]([Proper][NP](" + subjectNoun + ") [PP](" + association.Relation + " [Proper][NP](" + elementNoun + ")))";
		}

		namePhrase = language.TranslatePhrase (untranslatedName, getRandomFloat);

		Name = new Name (namePhrase, untranslatedName, language, World);

//		#if DEBUG
//		Debug.Log ("Leader #" + Id + " name: " + Name);
//		#endif
	}

	public virtual void Synchronize () {

		Name.Synchronize ();
	}

	public virtual void FinalizeLoad () {

		Name.World = World;
		Name.FinalizeLoad ();

		Group = World.GetGroup (GroupId);

		if (Group == null) {
			throw new System.Exception ("Missing Group with Id " + GroupId);
		}
	}
}
