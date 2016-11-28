using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Clan : Faction {

	public Clan () {

	}

	public Clan (CellGroup group, Polity polity) : base (group, polity) {

	}

	public override void UpdateInternal () {
		
	}

	public override void GenerateName () {

		int rngOffset = RngOffsets.CLAN_GENERATE_NAME + (int)Polity.Id;

		GetRandomIntDelegate getRandomInt = (int maxValue) => Group.GetNextLocalRandomInt (rngOffset++, maxValue);
		Language.GetRandomFloatDelegate getRandomFloat = () => Group.GetNextLocalRandomFloat (rngOffset++);

		Language language = Polity.Culture.Language;
		Region region = Group.Cell.Region;

		string untranslatedName = "";
		Language.NounPhrase namePhrase = null;

		if (region.Elements.Count <= 0) {

			throw new System.Exception ("No elements to choose name from");
		}

		List<RegionElement> remainingElements = new List<RegionElement> (region.Elements);

		bool addMoreWords = true;

		bool isPrimaryWord = true;
		float extraWordChange = 0.2f;

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
			
				addMoreWords = extraWordChange > getRandomFloat ();
			}

			if ((!canAddMoreWords) || (!addMoreWords)) {
				
				foreach (Faction faction in Polity.GetFactions ()) {

					if (namePhrase.Text == faction.Name.Text) {
						addMoreWords = true;
						break;
					}
				}
			}

			if (addMoreWords && !canAddMoreWords) {
			
				throw new System.Exception ("Ran out of words to add");
			}

			extraWordChange /= 2f;
		}

		Name = new Name (namePhrase, untranslatedName, language, World);
	}
}

public class ClanSplitEvent : FactionEvent {

	public const string EventSetFlag = "ClanSplitEvent_Set";

	public ClanSplitEvent () {

	}

	public ClanSplitEvent (Faction faction, int triggerDate) : base (faction, triggerDate, ClanSplitEventId) {

		Faction.SetFlag (EventSetFlag);
	}

	public static bool CanBeAssignedTo (Faction faction) {

		if (faction.IsFlagSet (EventSetFlag))
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		return true;
	}

	public override void Trigger () {

		Tribe tribe = Polity as Tribe;

		CellGroup targetGroup = tribe.GetRandomWeightedInfluencedGroup (RngOffsets.EVENT_TRIGGER + (int)Id);

		#if DEBUG
		if (targetGroup == null) {
			throw new System.Exception ("target group is null");
		}
		#endif

		Polity.AddFaction (new Clan (targetGroup, tribe));

		World.AddPolityToUpdate (Polity);
	}

	protected override void DestroyInternal ()
	{
		if (Faction != null) {
			Faction.UnsetFlag (EventSetFlag);
		}

		base.DestroyInternal ();
	}
}
