using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Clan : Faction {

	public const float NoCoreMigrationFactor = 0.0f;

	public const string ClanType = "Clan";

	[XmlAttribute("SpltDate")]
	public int ClanSplitEventDate;

	[XmlIgnore]
	public ClanSplitEvent ClanSplitEvent;

	public Clan () {

	}

	public Clan (Polity polity, CellGroup coreGroup, float prominence, Clan parentClan = null) : base (ClanType, polity, coreGroup, prominence, parentClan) {

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

	public override bool ShouldMigrateFactionCore (CellGroup sourceGroup, CellGroup targetGroup) {

		PolityInfluence piTarget = targetGroup.GetPolityInfluence (Polity);

		if (piTarget != null) {
			int targetGroupPopulation = targetGroup.Population;
			float targetGroupInfluence = piTarget.Value;

			return ShouldMigrateFactionCore (sourceGroup, targetGroup.Cell, targetGroupInfluence, targetGroupPopulation);
		}

		return false;
	}

	public override bool ShouldMigrateFactionCore (CellGroup sourceGroup, TerrainCell targetCell, float targetInfluence, int targetPopulation) {

		int sourcePopulation = sourceGroup.Population;

		PolityInfluence pi = sourceGroup.GetPolityInfluence (Polity);

		if (pi == null) {
			Debug.LogError ("Unable to find Polity with Id: " + Polity.Id);
		}

		float sourceGroupInfluence = pi.Value;

		float influenceFactor = sourceGroupInfluence / (sourceGroupInfluence + targetInfluence);
		float populationFactor = sourcePopulation / (sourcePopulation + targetPopulation);

		float migrateCoreFactor = Clan.NoCoreMigrationFactor + (influenceFactor * populationFactor) * (1 - Clan.NoCoreMigrationFactor);

		float randomValue = sourceGroup.GetNextLocalRandomFloat (RngOffsets.MIGRATING_GROUP_MOVE_FACTION_CORE + (int)Id);

		if (randomValue >= migrateCoreFactor)
			return true;

		return false;
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

	public const float MinCoreInfluenceValue = 0.3f;

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

		int rngOffset = RngOffsets.EVENT_CAN_TRIGGER + (int)Id;

		float triggerValue = Faction.GetNextLocalRandomFloat (rngOffset++);

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

		Clan newClan = new Clan (polity as Tribe, Faction.CoreGroup, newClanProminence, Faction as Clan);

		polity.AddFaction (newClan);

		World.AddFactionToUpdate (Faction);
		World.AddFactionToUpdate (newClan);

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

public class ClanCoreMigrationEvent : FactionEvent {

	[XmlAttribute]
	public long TargetGroupId;

	[XmlIgnore]
	public CellGroup TargetGroup;

	public const int DateSpanFactorConstant = CellGroup.GenerationTime * 200;

	//	public const string EventSetFlag = "ClanSplitEvent_Set";

	public ClanCoreMigrationEvent () {

		DoNotSerialize = true;
	}

	public ClanCoreMigrationEvent (Clan clan, CellGroup targetGroup, int triggerDate) : base (clan, triggerDate, ClanCoreMigrationEventId) {

		TargetGroup = targetGroup;

		TargetGroupId = TargetGroup.Id;

		//		clan.SetFlag (EventSetFlag);

		DoNotSerialize = true;
	}

	public static int CalculateTriggerDate (Clan clan) {

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.CLAN_CORE_MIGRATION_EVENT_CALCULATE_TRIGGER_DATE);
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

		return Faction.ShouldMigrateFactionCore (Faction.CoreGroup, TargetGroup);
	}

	public override void Trigger () {

		Faction.SetCoreGroup (TargetGroup);

		World.AddFactionToUpdate (Faction);

		World.AddPolityToUpdate (Faction.Polity);
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
