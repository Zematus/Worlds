using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Clan : Faction {

	public Clan () {

	}

	public Clan (CellGroup group, Polity polity, float prominence) : base (group, polity, prominence) {

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

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 10000;

	public const int MuSocialOrganizationValue = SocialOrganizationKnowledge.MuValueForClanSplitting;

	public const int MuAdministrativeLoadValue = 10000;

	public const string EventSetFlag = "ClanSplitEvent_Set";

	public ClanSplitEvent () {

	}

	public ClanSplitEvent (Clan clan, int triggerDate) : base (clan, triggerDate, ClanSplitEventId) {

		clan.SetFlag (EventSetFlag);
	}

	public static float CalculateAdministrativeLoadFactor (Clan clan) {

		float socialOrganizationValue = 0;

		CellGroup clanGroup = clan.Group;

		CulturalKnowledge socialOrganizationKnowledge = clanGroup.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge != null)
			socialOrganizationValue = socialOrganizationKnowledge.Value;

		float socialOrganizationFactor = socialOrganizationValue / (float)(MuSocialOrganizationValue + socialOrganizationValue);
		socialOrganizationFactor = Mathf.Clamp01 (socialOrganizationFactor) + 0.001f;

		float administrativeLoad = clan.Polity.TotalAdministrativeCost * clan.Prominence;

		return administrativeLoad / socialOrganizationFactor;
	}

	public static int CalculateTriggerDate (Clan clan) {

		float administrativeLoadFactor = CalculateAdministrativeLoadFactor (clan);

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.CLAN_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = randomFactor * randomFactor;

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / (administrativeLoadFactor + 0.001f);

		int targetDate = (int)(clan.World.CurrentDate + dateSpan);

		if (targetDate <= clan.World.CurrentDate)
			targetDate = int.MinValue;

		return targetDate;
	}

	public static bool CanBeAssignedTo (Faction faction) {

		if (faction.IsFlagSet (EventSetFlag))
			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		float administrativeLoadFactor = CalculateAdministrativeLoadFactor (Faction as Clan);

		administrativeLoadFactor = administrativeLoadFactor / (0.001f + administrativeLoadFactor + MuAdministrativeLoadValue);

		float triggerValue = Faction.GetNextLocalRandomFloat (RngOffsets.EVENT_CAN_TRIGGER + (int)Id);

		if (triggerValue > administrativeLoadFactor)
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

		float randomValue = Faction.GetNextLocalRandomFloat (RngOffsets.EVENT_CAN_TRIGGER + 1 + (int)Id);
		float randomFactor = 0.25f + randomValue * 0.5f;

		float oldProminence = Faction.Prominence;

		Faction.Prominence = oldProminence * randomFactor;

		float newClanProminence = oldProminence * (1f - randomFactor);

		Polity.AddFaction (new Clan (targetGroup, tribe, newClanProminence));

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
