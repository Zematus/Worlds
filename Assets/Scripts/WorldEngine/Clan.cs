using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

// Clan Leadership:
// -- Authority factors:
// ---- Agent Charisma
// ---- Agent Wisdom
// ---- Agent Timespan as Leader * Clan's Authority

// -- Leader Authority has an effect on the chances of the tribe splitting: Greater authority = less chance of splitting
// -- Clan Cohesiveness has also an effect on the chances of the tribe splitting: Greater cohesiveness = less chance of splitting
// -- Preventing a clan from splitting will reduce the clan's respect for authority but increases the overall clan cohesiveness

public class Clan : Faction {

	public const int LeadershipAvgSpan = 20 * World.YearLength;
	public const int MinClanLeaderStartAge = 16 * World.YearLength;
	public const int MaxClanLeaderStartAge = 50 * World.YearLength;

	public const int MinSocialOrganizationValue = 400;

	public const int MinCoreMigrationPopulation = 500;
	public const float MinCoreMigrationPolityInfluence = 0.3f;

	public const float AvgClanSplitRelationshipValue = 0.5f;
	public const float ClanSplitRelationshipValueSpread = 0.1f;
	public const float ClanSplitRelationshipValueCharismaFactor = 50f;

	public const string ClanType = "Clan";

	[XmlAttribute("CoreMigDate")]
	public long CoreMigrationEventDate;

	[XmlAttribute("CSplitDate")]
	public long ClanSplitDecisionEventDate;

	[XmlAttribute("TSplitDate")]
	public long TribeSplitDecisionEventDate;

	[XmlAttribute("CoreMigOrigTribeId")]
	public long CoreMigrationEventOriginalTribeId;

	[XmlAttribute("CSplitOrigTribeId")]
	public long ClanSplitDecisionOriginalTribeId;

	[XmlAttribute("TSplitOrigTribeId")]
	public long TribeSplitDecisionOriginalTribeId;

	[XmlIgnore]
	public ClanCoreMigrationEvent CoreMigrationEvent;

	[XmlIgnore]
	public ClanSplitDecisionEvent ClanSplitDecisionEvent;

	[XmlIgnore]
	public TribeSplitDecisionEvent TribeSplitDecisionEvent;

	public Clan () {

	}

	public Clan (Polity polity, CellGroup coreGroup, float prominence, Clan parentClan = null) : base (ClanType, polity, coreGroup, prominence, parentClan) {

	}

	protected override void InitializeInternal ()
	{
		base.InitializeInternal ();

		CoreMigrationEventDate = ClanCoreMigrationEvent.CalculateTriggerDate (this);
		CoreMigrationEvent = new ClanCoreMigrationEvent (this, CoreMigrationEventDate);
		CoreMigrationEventOriginalTribeId = CoreMigrationEvent.OriginalPolityId;
		World.InsertEventToHappen (CoreMigrationEvent);

		ClanSplitDecisionEventDate = ClanSplitDecisionEvent.CalculateTriggerDate (this);
		ClanSplitDecisionEvent = new ClanSplitDecisionEvent (this, ClanSplitDecisionEventDate);
		ClanSplitDecisionOriginalTribeId = ClanSplitDecisionEvent.OriginalPolityId;
		World.InsertEventToHappen (ClanSplitDecisionEvent);

		TribeSplitDecisionEventDate = TribeSplitDecisionEvent.CalculateTriggerDate (this);
		TribeSplitDecisionEvent = new TribeSplitDecisionEvent (this, TribeSplitDecisionEventDate);
		TribeSplitDecisionOriginalTribeId = TribeSplitDecisionEvent.OriginalPolityId;
		World.InsertEventToHappen (TribeSplitDecisionEvent);
	}

	public CellGroup GetCoreGroupMigrationTarget () {

		Direction migrationDirection = CoreGroup.GenerateCoreMigrationDirection ();

		if (migrationDirection == Direction.Null) {
			return null;
		}

		return CoreGroup.Neighbors [migrationDirection];
	}

	public override void FinalizeLoad () {
	
		base.FinalizeLoad ();

		CoreMigrationEvent = new ClanCoreMigrationEvent (this, CoreMigrationEventOriginalTribeId, CoreMigrationEventDate);
		World.InsertEventToHappen (CoreMigrationEvent);

		ClanSplitDecisionEvent = new ClanSplitDecisionEvent (this, ClanSplitDecisionOriginalTribeId, ClanSplitDecisionEventDate);
		World.InsertEventToHappen (ClanSplitDecisionEvent);

		TribeSplitDecisionEvent = new TribeSplitDecisionEvent (this, TribeSplitDecisionOriginalTribeId, TribeSplitDecisionEventDate);
		World.InsertEventToHappen (TribeSplitDecisionEvent);
	}

	protected override void UpdateInternal () {

		if (NewCoreGroup != null) {
			if ((NewCoreGroup != null) && (IsGroupValidCore (NewCoreGroup))) {
				MigrateToNewCoreGroup ();
			}

			NewCoreGroup = null;

			CoreMigrationEventDate = ClanCoreMigrationEvent.CalculateTriggerDate (this);

			CoreMigrationEvent.Reset (CoreMigrationEventDate);
			CoreMigrationEventOriginalTribeId = CoreMigrationEvent.OriginalPolityId;

			World.InsertEventToHappen (CoreMigrationEvent);
		}
	}

	protected override void GenerateName (Faction parentFaction) {

		int rngOffset = RngOffsets.CLAN_GENERATE_NAME + (int)Polity.Id;

		if (parentFaction != null)
			rngOffset += (int)parentFaction.Id;

		GetRandomIntDelegate getRandomInt = (int maxValue) => Polity.GetNextLocalRandomInt (rngOffset++, maxValue);
		Language.GetRandomFloatDelegate getRandomFloat = () => Polity.GetNextLocalRandomFloat (rngOffset++);

		Language language = Polity.Culture.Language;
		Region region = CoreGroup.Cell.Region;

		string untranslatedName = "";
		Language.Phrase namePhrase = null;

		if (region.Elements.Count <= 0) {
			throw new System.Exception ("No elements to choose name from");
		}

		IEnumerable<string> possibleAdjectives = null;

		List<Element> remainingElements = new List<Element> (region.Elements);

		bool addMoreWords = true;

		bool isPrimaryNoun = true;
		float extraWordChance = 0.2f;

		List<Element> usedElements = new List<Element> ();

		while (addMoreWords) {

			addMoreWords = false;

			bool hasRemainingElements = remainingElements.Count > 0;

			if ((!hasRemainingElements) && (usedElements.Count <= 0)) {

				throw new System.Exception ("No elements to use for name");
			}

			Element element = null;

			if (hasRemainingElements) {
				element = remainingElements.RandomSelectAndRemove (getRandomInt);

				usedElements.Add (element);

			} else {
				element = usedElements.RandomSelect (getRandomInt);
			}

			if (isPrimaryNoun) {
				untranslatedName = element.SingularName;
				isPrimaryNoun = false;

				possibleAdjectives = element.Adjectives;

			} else {
				bool first = true;
				foreach (Element usedElement in usedElements) {
					if (first) {
						untranslatedName = usedElement.SingularName;
						first = false;
					} else {
						untranslatedName = usedElement.SingularName + ":" + untranslatedName;
					}
				}
			}

			string adjective = possibleAdjectives.RandomSelect (getRandomInt, 2 * usedElements.Count);

			if (!string.IsNullOrEmpty (adjective)) {
				untranslatedName = "[adj]" + adjective + " " + untranslatedName;
			}

			addMoreWords = extraWordChance > getRandomFloat ();

			if (!addMoreWords) {
				
				foreach (Faction faction in Polity.GetFactions ()) {

					if (Language.ClearConstructCharacters(untranslatedName) == faction.Name.Meaning) {
						addMoreWords = true;
						break;
					}
				}
			}

			extraWordChance /= 2f;
		}

		untranslatedName = "[Proper][NP](" + untranslatedName + ")";

		namePhrase = language.TranslatePhrase (untranslatedName);

		Name = new Name (namePhrase, untranslatedName, language, World);

//		#if DEBUG
//		Debug.Log ("Clan #" + Id + " name: " + Name);
//		#endif
	}

	protected override Agent RequestCurrentLeader ()
	{
		return RequestCurrentLeader (LeadershipAvgSpan, MinClanLeaderStartAge, MaxClanLeaderStartAge, RngOffsets.CLAN_LEADER_GEN_OFFSET);
	}

	protected override Agent RequestNewLeader ()
	{
		return RequestNewLeader (LeadershipAvgSpan, MinClanLeaderStartAge, MaxClanLeaderStartAge, RngOffsets.CLAN_LEADER_GEN_OFFSET);
	}

	public static bool CanBeClanCore (CellGroup group)
	{
		CulturalKnowledge knowledge = group.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (knowledge == null)
			return false;

		if (knowledge.Value < MinSocialOrganizationValue)
			return false;

		return true;
	}

	public bool IsGroupValidCore (CellGroup group)
	{
		if (!CanBeClanCore(group))
			return false;

		CulturalDiscovery discovery = group.Culture.GetDiscovery (TribalismDiscovery.TribalismDiscoveryId);

		if (discovery == null)
			return false;

		PolityInfluence pi = group.GetPolityInfluence (Polity);

		if (pi == null)
			return false;

		if (pi.Value < MinCoreMigrationPolityInfluence)
			return false;

		if (group.Population < MinCoreMigrationPopulation)
			return false;

		return true;
	}

	public override bool ShouldMigrateFactionCore (CellGroup sourceGroup, CellGroup targetGroup) {

		if (!CanBeClanCore (targetGroup))
			return false;

		PolityInfluence piTarget = targetGroup.GetPolityInfluence (Polity);

		if (piTarget != null) {
			int targetGroupPopulation = targetGroup.Population;
			float targetGroupInfluence = piTarget.Value;

			return ShouldMigrateFactionCore (sourceGroup, targetGroup.Cell, targetGroupInfluence, targetGroupPopulation);
		}

		return false;
	}

	public override bool ShouldMigrateFactionCore (CellGroup sourceGroup, TerrainCell targetCell, float targetInfluence, int targetPopulation) {

		float targetInfluenceFactor = Mathf.Max (0, targetInfluence - MinCoreMigrationPolityInfluence);

		if (targetInfluenceFactor <= 0)
			return false;

		float targetPopulationFactor = Mathf.Max (0, targetPopulation - MinCoreMigrationPopulation);

		if (targetPopulationFactor <= 0)
			return false;

		int sourcePopulation = sourceGroup.Population;

		PolityInfluence pi = sourceGroup.GetPolityInfluence (Polity);

		if (pi == null) {
			Debug.LogError ("Unable to find Polity with Id: " + Polity.Id);
		}

		float sourceInfluence = pi.Value;

		float sourceInfluenceFactor = Mathf.Max (0, sourceInfluence - MinCoreMigrationPolityInfluence);
		float sourcePopulationFactor = Mathf.Max (0, sourcePopulation - MinCoreMigrationPopulation);

		float sourceFactor = sourceInfluenceFactor * sourcePopulationFactor;

		if (sourceFactor <= 0)
			return true;

		float targetFactor = targetInfluenceFactor * targetPopulationFactor;

		float migrateCoreFactor = sourceFactor / (sourceFactor + targetFactor);

		float randomValue = sourceGroup.GetNextLocalRandomFloat (RngOffsets.MIGRATING_GROUP_MOVE_FACTION_CORE + (int)Id);

		if (randomValue > migrateCoreFactor)
			return true;

		return false;
	}

	public override void Split () {

//		#if DEBUG
//		if (Polity.Territory.IsSelected) {
//			bool debug = true;
//		}
//		#endif

		int randomOffset = (int)(RngOffsets.CLAN_SPLIT + Id);

		float randomValue = GetNextLocalRandomFloat (randomOffset++);
		float splitFactionProminence = _splitFactionMinProminence + (randomValue * (_splitFactionMaxProminence - _splitFactionMinProminence));

		Prominence -= splitFactionProminence;

		Clan newClan = new Clan (Polity as Tribe, _splitFactionCoreGroup, splitFactionProminence, this);
		newClan.Initialize (); // We can initialize right away since the containing polity is already initialized

		// set relationship with parent clan

		float parentClanRelationshipValue = AvgClanSplitRelationshipValue + (CurrentLeader.Charisma - 10) / ClanSplitRelationshipValueCharismaFactor;

		randomValue = GetNextLocalRandomFloat (randomOffset++);
		float relationshipValue = parentClanRelationshipValue + (ClanSplitRelationshipValueSpread * (2f * randomValue - 1f));

		SetRelationship (this, newClan, relationshipValue);

		// set relationship with rest of factions in polity

		float avgNewClanRelationshipValue = AvgClanSplitRelationshipValue + (newClan.CurrentLeader.Charisma - 10) / ClanSplitRelationshipValueCharismaFactor;

		foreach (Faction faction in Polity.GetFactions (true)) {

			if (faction == this)
				continue;
		
			randomValue = GetNextLocalRandomFloat (randomOffset++);
			relationshipValue = avgNewClanRelationshipValue + (ClanSplitRelationshipValueSpread * (2f * randomValue - 1f));

			SetRelationship (faction, newClan, relationshipValue);
		}

		// finalize

		Polity.AddFaction (newClan);

		Polity.UpdateDominantFaction ();

		World.AddFactionToUpdate (this);
		World.AddFactionToUpdate (newClan);

		World.AddPolityToUpdate (Polity);

		Polity.AddEventMessage (new ClanSplitEventMessage (this, newClan, World.CurrentDate));
	}

	public float CalculateAdministrativeLoad () {

		float socialOrganizationValue = 0;

		CulturalKnowledge socialOrganizationKnowledge = Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

		if (socialOrganizationKnowledge != null)
			socialOrganizationValue = socialOrganizationKnowledge.Value;

		if (socialOrganizationValue < 0) {

			return Mathf.Infinity;
		}

		float administrativeLoad = Polity.TotalAdministrativeCost * Prominence / socialOrganizationValue;

		administrativeLoad = Mathf.Pow (administrativeLoad, 2);

		if (administrativeLoad < 0) {

			Debug.LogWarning ("administrativeLoad less than 0: " + administrativeLoad);

			return Mathf.Infinity;
		}

		return administrativeLoad;
	}
}
