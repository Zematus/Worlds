using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Agent : ISynchronizable {

	public const int MaxLifespan = 113; // Prime number to hide birthdate cycle artifacts

	[XmlAttribute]
	public long Id;

	[XmlAttribute("Birth")]
	public int BirthDate;

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

		GenerateName ();
	}

	public void Destroy () {

		StillPresent = false;
	}

	private void GenerateName () {

		int rngOffset = RngOffsets.AGENT_GENERATE_NAME + (int)Group.Id;

		GetRandomIntDelegate getRandomInt = (int maxValue) => Group.GetLocalRandomInt (BirthDate, rngOffset++, maxValue);
		Language.GetRandomFloatDelegate getRandomFloat = () => Group.GetLocalRandomFloat (BirthDate, rngOffset++);

		Language language = Group.Culture.Language;
		Region region = Group.Cell.Region;

		string untranslatedName = "";
		Language.NounPhrase namePhrase = null;

		if (region.Elements.Count <= 0) {

			throw new System.Exception ("No elements to choose name from");
		}

		List<RegionElement> remainingElements = new List<RegionElement> (region.Elements);

		bool addMoreWords = true;

		bool isPrimaryWord = true;
		float extraWordChance = 0.2f;

		while (addMoreWords) {

			addMoreWords = false;

			int index = getRandomInt (remainingElements.Count);

			RegionElement element = remainingElements [index];

			remainingElements.RemoveAt (index);

			if (isPrimaryWord) {

				untranslatedName = element.Name;
				isPrimaryWord = false;

			} else {

				untranslatedName = "[nad]" + element.Name + " " + untranslatedName;
			}

			namePhrase = language.TranslateNounPhrase (untranslatedName, getRandomFloat);

			bool canAddMoreWords = remainingElements.Count > 0;

			if (canAddMoreWords) {

				addMoreWords = extraWordChance > getRandomFloat ();
			}

			if (addMoreWords && !canAddMoreWords) {

				throw new System.Exception ("Ran out of words to add");
			}

			extraWordChance /= 2f;
		}

		Name = new Name (namePhrase, untranslatedName, language, World);
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
