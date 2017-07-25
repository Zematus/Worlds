using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Clan : Faction {

	public const string ClanType = "Clan";

	[XmlAttribute("SpltDate")]
	public int ClanSplitEventDate;

	[XmlIgnore]
	public ClanSplitEvent ClanSplitEvent;

	public Clan () {

	}

	public Clan (Polity polity, float prominence, Clan parentClan = null) : base (ClanType, polity, prominence, parentClan) {

		if (ClanSplitEvent.CanBeAssignedTo (this)) {

			ClanSplitEventDate = ClanSplitEvent.CalculateTriggerDate (this);

			ClanSplitEvent = new ClanSplitEvent (this, ClanSplitEventDate);

			World.InsertEventToHappen (ClanSplitEvent);
		}

		string logMessage = "New clan '" + Name + "' spawned in Polity '" + Polity.Name + "'";

		if (parentClan != null) {
		
			logMessage += " from clan '" + parentClan.Name + "'";
		}

		logMessage += " in year " + World.CurrentDate + ", starting prominence: " + prominence;

		if (parentClan != null) {
			
//			Debug.Log (logMessage);
		}
	}

	public override void FinalizeLoad () {
	
		base.FinalizeLoad ();

		if (ClanSplitEvent.CanBeAssignedTo (this)) {

			ClanSplitEvent = new ClanSplitEvent (this, ClanSplitEventDate);

			World.InsertEventToHappen (ClanSplitEvent);
		}
	}

	protected override void UpdateInternal () {
		
	}

	protected override void GenerateName (Faction parentFaction) {

		int rngOffset = RngOffsets.CLAN_GENERATE_NAME + (int)Polity.Id;

		if (parentFaction != null)
			rngOffset += (int)parentFaction.Id;

		GetRandomIntDelegate getRandomInt = (int maxValue) => Polity.GetNextLocalRandomInt (rngOffset++, maxValue);
		Language.GetRandomFloatDelegate getRandomFloat = () => Polity.GetNextLocalRandomFloat (rngOffset++);

		Language language = Polity.Culture.Language;
		Region region = Polity.CoreGroup.Cell.Region;

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

public class ClanSplitEventMessage : FactionEventMessage {

	[XmlAttribute]
	public long NewClanId;

	public ClanSplitEventMessage () {

	}

	public ClanSplitEventMessage (Faction faction, Clan newClan, long date) : base (faction, WorldEvent.ClanSplitEventId, date) {

		NewClanId = newClan.Id;
	}

	protected override string GenerateMessage ()
	{
		Faction newClan = Polity.GetFaction (NewClanId);

		return "A new clan, " + newClan.Name.Text + ", has split from clan " +  Faction.Name.Text;
	}
}

public class ClanSplitEvent : FactionEvent {

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 2000;

	public const int MuAdministrativeLoadValue = 500000;

	public const float MinProminenceTrigger = 0.3f;
	public const float MinProminenceTransfer = 0.25f;
	public const float ProminenceTransferProportion = 0.75f;

//	public const string EventSetFlag = "ClanSplitEvent_Set";

	public ClanSplitEvent () {

		DoNotSerialize = true;
	}

	public ClanSplitEvent (Clan clan, int triggerDate) : base (clan, triggerDate, ClanSplitEventId) {

//		clan.SetFlag (EventSetFlag);

		DoNotSerialize = true;
	}

	private static float CalculateClanAdministrativeLoadFactor (Clan clan) {

		float socialOrganizationValue = 0;

		CulturalKnowledge socialOrganizationKnowledge = clan.Polity.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge != null)
			socialOrganizationValue = socialOrganizationKnowledge.Value;

		if (socialOrganizationValue <= 0) {
		
			return float.MaxValue;
		}

		float administrativeLoad = clan.Polity.TotalAdministrativeCost * clan.Prominence;

		return administrativeLoad / socialOrganizationValue;
	}

	public static int CalculateTriggerDate (Clan clan) {

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.CLAN_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant;

		int targetDate = (int)(clan.World.CurrentDate + dateSpan);

		if (targetDate <= clan.World.CurrentDate)
			targetDate = int.MinValue;

		return targetDate;
	}

	public static bool CanBeAssignedTo (Clan clan) {

//		if (clan.IsFlagSet (EventSetFlag))
//			return false;

		return true;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		#if DEBUG
		if (Faction.Polity.Territory.IsSelected) {
			bool debug = true;
		}
		#endif

		if (Faction.Prominence < MinProminenceTrigger)
			return false;

		float administrativeLoadFactor = CalculateClanAdministrativeLoadFactor (Faction as Clan);
		administrativeLoadFactor = Mathf.Pow (administrativeLoadFactor, 2);

		if (administrativeLoadFactor < 0)
			return true;

		float splitValue = administrativeLoadFactor / (administrativeLoadFactor + MuAdministrativeLoadValue);

		float triggerValue = Faction.GetNextLocalRandomFloat (RngOffsets.EVENT_CAN_TRIGGER + (int)Id);

		if (triggerValue > splitValue)
			return false;

		return true;
	}

	public override void Trigger () {

		float randomValue = Faction.GetNextLocalRandomFloat (RngOffsets.EVENT_TRIGGER + 1 + (int)Id);
		float randomFactor = MinProminenceTransfer + (randomValue * ProminenceTransferProportion);

		float oldProminence = Faction.Prominence;

		Faction.Prominence = oldProminence * randomFactor;

		float newClanProminence = oldProminence * (1f - randomFactor);

		Polity polity = Faction.Polity;

		Clan newClan = new Clan (polity as Tribe, newClanProminence, Faction as Clan);

		polity.AddFaction (newClan);

		World.AddPolityToUpdate (polity);

		polity.AddEventMessage (new ClanSplitEventMessage (Faction, newClan, TriggerDate));
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

//		if (Faction != null) {
//			Faction.UnsetFlag (EventSetFlag);
//		}

		if ((Faction != null) && (Faction.StillPresent)) {

			Clan clan = Faction as Clan;

			if (CanBeAssignedTo (clan)) {

				clan.ClanSplitEvent = this;

				clan.ClanSplitEventDate = CalculateTriggerDate (clan);

				Reset (clan.ClanSplitEventDate);

				World.InsertEventToHappen (this);
			}
		}
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Clan clan = Faction as Clan;

		clan.ClanSplitEvent = this;
	}
}
