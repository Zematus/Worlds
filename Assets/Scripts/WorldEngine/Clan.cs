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

	public const string ClanType = "Clan";

	public const float Split_MinProminenceTransfer = 0.25f;
	public const float Split_ProminenceTransferProportion = 0.75f;

	public const long ClanSplitDateSpanFactorConstant = CellGroup.GenerationSpan * 40;

	[XmlAttribute("CoreMigDate")]
	public long CoreMigrationEventDate;

	[XmlAttribute("SplitDate")]
	public long SplitDecisionEventDate;

	[XmlIgnore]
	public ClanCoreMigrationEvent CoreMigrationEvent;

	[XmlIgnore]
	public ClanSplitDecisionEvent SplitDecisionEvent;

	public Clan () {

	}

	public Clan (Polity polity, CellGroup coreGroup, float prominence, Clan parentClan = null) : base (ClanType, polity, coreGroup, prominence, parentClan) {

	}

	protected override void InitializeInternal ()
	{
		base.InitializeInternal ();

		CoreMigrationEventDate = ClanCoreMigrationEvent.CalculateTriggerDate (this);
		CoreMigrationEvent = new ClanCoreMigrationEvent (this, CoreMigrationEventDate);
		World.InsertEventToHappen (CoreMigrationEvent);

		SplitDecisionEventDate = ClanSplitDecisionEvent.CalculateTriggerDate (this);
		SplitDecisionEvent = new ClanSplitDecisionEvent (this, SplitDecisionEventDate);
		World.InsertEventToHappen (SplitDecisionEvent);
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

		CoreMigrationEvent = new ClanCoreMigrationEvent (this, CoreMigrationEventDate);
		World.InsertEventToHappen (CoreMigrationEvent);

		SplitDecisionEvent = new ClanSplitDecisionEvent (this, SplitDecisionEventDate);
		World.InsertEventToHappen (SplitDecisionEvent);
	}

	protected override void UpdateInternal () {

		if (NewCoreGroup != null) {
			if ((NewCoreGroup != null) && (IsGroupValidCore (NewCoreGroup))) {
				MigrateToNewCoreGroup ();
			}

			NewCoreGroup = null;

			CoreMigrationEventDate = ClanCoreMigrationEvent.CalculateTriggerDate (this);

			CoreMigrationEvent.Reset (CoreMigrationEventDate);

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

		float randomValue = GetNextLocalRandomFloat ((int)(RngOffsets.CLAN_SPLIT + Id));
		float randomFactor = Split_MinProminenceTransfer + (randomValue * Split_ProminenceTransferProportion);

		float oldProminence = Prominence;

		Prominence = oldProminence * randomFactor;

		float newClanProminence = oldProminence * (1f - randomFactor);

		Clan newClan = new Clan (Polity as Tribe, _splitFactionCoreGroup, newClanProminence, this);
		newClan.Initialize (); // We can initialize right away since the containing polity is already initialized

		Polity.AddFaction (newClan);

		Polity.UpdateDominantFaction ();

		World.AddFactionToUpdate (this);
		World.AddFactionToUpdate (newClan);

		World.AddPolityToUpdate (Polity);

		Polity.AddEventMessage (new ClanSplitEventMessage (this, newClan, World.CurrentDate));
	}

	public float CalculateAdministrativeLoad () {

		float socialOrganizationValue = 0;

		CulturalKnowledge socialOrganizationKnowledge = Polity.Culture.GetKnowledge (SocialOrganizationKnowledge.SocialOrganizationKnowledgeId);

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

	public void IncreaseAuthorityPreferenceValue (float percentage) {

		CulturalPreference authorityPreference = Polity.Culture.GetPreference (CulturalPreference.AuthorityPreferenceId);

		if (authorityPreference == null)
			throw new System.Exception ("authorityPreference is null");

		float value = authorityPreference.Value;

		authorityPreference.Value += (1f - value) * percentage;
	}

	public void DecreaseAuthorityPreferenceValue (float percentage) {

		CulturalPreference authorityPreference = Polity.Culture.GetPreference (CulturalPreference.AuthorityPreferenceId);

		if (authorityPreference == null)
			throw new System.Exception ("authorityPreference is null");

		float value = authorityPreference.Value;

		authorityPreference.Value = value * (1f - percentage);
	}

	public float GetAuthorityPreferenceValue () {

		CulturalPreference authorityPreference = Polity.Culture.GetPreference (CulturalPreference.AuthorityPreferenceId);

		if (authorityPreference != null)
			return authorityPreference.Value; 

		return 0;
	}

	public void IncreaseCohesivenessPreferenceValue (float percentage) {

		CulturalPreference cohesivenessPreference = Polity.Culture.GetPreference (CulturalPreference.CohesivenessPreferenceId);

		if (cohesivenessPreference == null)
			throw new System.Exception ("cohesivenessPreference is null");

		float value = cohesivenessPreference.Value;

		cohesivenessPreference.Value += (1f - value) * percentage;
	}

	public void DecreaseCohesivenessPreferenceValue (float percentage) {

		CulturalPreference cohesivenessPreference = Polity.Culture.GetPreference (CulturalPreference.CohesivenessPreferenceId);

		if (cohesivenessPreference == null)
			throw new System.Exception ("cohesivenessPreference is null");

		float value = cohesivenessPreference.Value;

		cohesivenessPreference.Value = value * (1f - percentage);
	}

	public float GetCohesivenessPreferenceValue () {

		CulturalPreference cohesivenessPreference = Polity.Culture.GetPreference (CulturalPreference.CohesivenessPreferenceId);

		if (cohesivenessPreference != null)
			return cohesivenessPreference.Value; 

		return 0;
	}
}

public class PreventClanSplitEventMessage : FactionEventMessage {

	[XmlAttribute]
	public long AgentId;

	public PreventClanSplitEventMessage () {

	}

	public PreventClanSplitEventMessage (Faction faction, Agent agent, long date) : base (faction, WorldEvent.PreventClanSplitEventId, date) {

		faction.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
	}

	protected override string GenerateMessage ()
	{
		Agent leader = World.GetMemorableAgent (AgentId);

		return leader.Name.Text + " has prevented clan " +  Faction.Name.Text + " from splitting";
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
		Faction newClan = World.GetFaction (NewClanId);

		return "A new clan, " + newClan.Name.Text + ", has split from clan " +  Faction.Name.Text;
	}
}

public class ClanSplitDecision : FactionDecision {

	public const float MinPreferencePercentChange = 0.10f;
	public const float MaxPreferencePercentChange = 0.20f;

	private Clan _clan;

	private bool _cantPrevent = false;
	private bool _preferSplit = true;

	private CellGroup _newCoreGroup;

	public ClanSplitDecision (Clan clan, CellGroup newCoreGroup) : base (clan) {

		_clan = clan;

		Description = "(autofail) Several family groups belonging to clan <b>" + clan.Name.Text + "</b> no longer feel to be connected to the rest of the clan. " +
			"Unfortunately, <b>" + clan.CurrentLeader.Name.Text + "</b> can't do anything about it under the current circumstances...";

		_cantPrevent = true;

		_newCoreGroup = newCoreGroup;
	}

	public ClanSplitDecision (Clan clan, CellGroup newCoreGroup, bool preferSplit) : base (clan) {

		_clan = clan;

		Description = "(" + preferSplit + ") Several family groups belonging to clan <b>" + clan.Name.Text + "</b> no longer feel to be connected to the rest of the clan. " +
			"Should the clan leader, <b>" + clan.CurrentLeader.Name.Text + "</b>, try to keep them from splitting apart?";

		_preferSplit = preferSplit;

		_newCoreGroup = newCoreGroup;
	}

	private void PreventSplit () {

		float charismaFactor = _clan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _clan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		int rngOffset = RngOffsets.CLAN_SPLITTING_EVENT_MODIFY_ATTRIBUTE;
		
		float randomFactor = _clan.GetNextLocalRandomFloat (rngOffset++);
		float authorityPreferencePercentChange = (MaxPreferencePercentChange - MinPreferencePercentChange) * randomFactor + MinPreferencePercentChange;
		authorityPreferencePercentChange /= attributesFactor;

		randomFactor = _clan.GetNextLocalRandomFloat (rngOffset++);
		float cohesivenessPreferencePercentChange = (MaxPreferencePercentChange - MinPreferencePercentChange) * randomFactor + MinPreferencePercentChange;
		cohesivenessPreferencePercentChange *= attributesFactor;

		_clan.DecreaseAuthorityPreferenceValue (authorityPreferencePercentChange);
		_clan.IncreaseCohesivenessPreferenceValue (cohesivenessPreferencePercentChange);

		// Should reduce respect for authority and increase cohesiveness
		Faction.Polity.AddEventMessage (new PreventClanSplitEventMessage (Faction, Faction.CurrentLeader, Faction.World.CurrentDate));
	}

	private void AllowSplit () {

		// Should increase respect for authority and reduce cohesiveness
		Faction.SetToSplit (_newCoreGroup);
	}

	public override Option[] GetOptions () {

		if (_cantPrevent) {
			
			return new Option[] {
				new Option ("Oh well...", AllowSplit),
			};
		}

		return new Option[] {
			new Option ("Allow clan to split in two...", AllowSplit),
			new Option ("Prevent clan from splitting...", PreventSplit)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_preferSplit)
			AllowSplit ();
		else
			PreventSplit ();
	}
}

public class ClanCoreMigrationEvent : FactionEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 500;

	private CellGroup _targetGroup;

	public ClanCoreMigrationEvent () {

		DoNotSerialize = true;
	}

	public ClanCoreMigrationEvent (Clan clan, long triggerDate) : base (clan, triggerDate, ClanCoreMigrationEventId) {

		DoNotSerialize = true;
	}

	public static long CalculateTriggerDate (Clan clan) {

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.CLAN_CORE_MIGRATION_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant;

		long targetDate = (long)(clan.World.CurrentDate + dateSpan) + CellGroup.GenerationSpan;

		return targetDate;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

//		#if DEBUG
//		if (Faction.Polity.Territory.IsSelected) {
//			bool debug = true;
//		}
//		#endif

		Clan clan = Faction as Clan;

		_targetGroup = clan.GetCoreGroupMigrationTarget ();

		if (_targetGroup == null)
			return false;

		return Faction.ShouldMigrateFactionCore (Faction.CoreGroup, _targetGroup);
	}

	public override void Trigger () {

		Faction.PrepareNewCoreGroup (_targetGroup);

		World.AddGroupToUpdate (Faction.CoreGroup);
		World.AddGroupToUpdate (_targetGroup);

		World.AddFactionToUpdate (Faction);

		World.AddPolityToUpdate (Faction.Polity);
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Faction != null) && (Faction.StillPresent)) {

			Clan clan = Faction as Clan;

			clan.CoreMigrationEvent = this;

			clan.CoreMigrationEventDate = CalculateTriggerDate (clan);

			Reset (clan.CoreMigrationEventDate);

			World.InsertEventToHappen (this);
		}
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		Clan clan = Faction as Clan;

		clan.CoreMigrationEvent = this;
	}
}

public class ClanSplitDecisionEvent : FactionEvent {
	
	public const float MinProminenceTrigger = 0.3f;
	public const float MinCoreDistance = 1000f;
	public const float MinCoreInfluenceValue = 0.5f;

	public const int MaxAdministrativeLoad = 4000000;
	public const int MinAdministrativeLoad = 400000;

	private Clan _clan;

	private CellGroup _splitFactionCoreGroup;

	private float _chanceOfSplitting;

	public ClanSplitDecisionEvent () {

		DoNotSerialize = true;
	}

	public ClanSplitDecisionEvent (Clan clan, long triggerDate) : base (clan, triggerDate, ClanSplitEventId) {

		_clan = clan;

		DoNotSerialize = true;
	}

	public static long CalculateTriggerDate (Clan clan) {

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.CLAN_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float administrativeLoad = clan.CalculateAdministrativeLoad ();

		float loadFactor = 1;

		if (administrativeLoad != Mathf.Infinity) {

			float modAdminLoad = Mathf.Max (0, administrativeLoad - MinAdministrativeLoad);
			float modHalfFactorAdminLoad = MaxAdministrativeLoad - MinAdministrativeLoad;

			loadFactor = modHalfFactorAdminLoad / (modAdminLoad + modHalfFactorAdminLoad);
		}

		float cohesivenessPreferenceValue = clan.GetCohesivenessPreferenceValue ();

		float dateSpan = (1 - randomFactor) *  Clan.ClanSplitDateSpanFactorConstant * loadFactor * cohesivenessPreferenceValue;

		long triggerDateSpan = (long)dateSpan + CellGroup.GenerationSpan;

		if (triggerDateSpan < 0) {
			#if DEBUG
			Debug.LogWarning ("updateSpan less than 0: " + triggerDateSpan);
			#endif

			triggerDateSpan = CellGroup.MaxUpdateSpan;
		}

//		#if DEBUG
//		if (clan.Name.Text == "Nuse-zis") {
//			Debug.Log ("Clan \"" + clan.Name.Text + "\" splitting event triggerDate span: " + Manager.GetTimeSpanString (triggerDateSpan));
//		}
//		#endif

		return clan.World.CurrentDate + triggerDateSpan;
	}

	public float GetGroupWeight (CellGroup group) {

		if (group == _clan.CoreGroup)
			return 0;

		PolityInfluence pi = group.GetPolityInfluence (_clan.Polity);

		if (group.HighestPolityInfluence != pi)
			return 0;

		if (!Clan.CanBeClanCore (group))
			return 0;

		float coreDistance = pi.FactionCoreDistance - MinCoreDistance;

		if (coreDistance <= 0)
			return 0;

		float coreDistanceFactor = MinCoreDistance / (MinCoreDistance + coreDistance);

		float minCoreInfluenceValue = MinCoreInfluenceValue * coreDistanceFactor;

		float value = pi.Value - minCoreInfluenceValue;

		if (value <= 0)
			return 0;

		float weight = pi.Value;

		if (weight < 0)
			return float.MaxValue;

		return weight;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ()) {

//			#if DEBUG
//			if (Clan.Name.Text == "Nuse-zis") {
//				Debug.Log ("Clan \"" + Clan.Name.Text + "\" can't trigger split...");
//			}
//			#endif

			return false;
		}

		if (_clan.Prominence < MinProminenceTrigger) {

//			#if DEBUG
//			if (Clan.Name.Text == "Nuse-zis") {
//				Debug.Log ("Clan \"" + Clan.Name.Text + "\" has not enough prominence: " + Clan.Prominence);
//			}
//			#endif
			
			return false;
		}

		int rngOffset = (int)(RngOffsets.CLAN_SPLITTING_EVENT_CAN_TRIGGER + Id);

		_splitFactionCoreGroup = _clan.Polity.GetRandomGroup (rngOffset++, GetGroupWeight, true);

		if (_splitFactionCoreGroup == null) {

//			#if DEBUG
//			if (Clan.Name.Text == "Nuse-zis") {
//				Debug.Log ("Clan \"" + Clan.Name.Text + "\" has _splitFactionCoreGroup == null");
//			}
//			#endif
			
			return false;
		}

		_chanceOfSplitting = CalcChanceOfSplitting ();

		if (_chanceOfSplitting <= 0) {

//			#if DEBUG
//			if (Clan.Name.Text == "Nuse-zis") {
//				Debug.Log ("Clan \"" + Clan.Name.Text + "\" has _chanceOfSplitting <= zero");
//			}
//			#endif

			return false;
		}

		return true;
	}

	public float CalcChanceOfSplitting () {

		float administrativeLoad = _clan.CalculateAdministrativeLoad ();

		if (administrativeLoad == Mathf.Infinity)
			return 1;

		float cohesivenessPreferenceValue = _clan.GetCohesivenessPreferenceValue ();

		if (cohesivenessPreferenceValue <= 0)
			return 1;

		float authorityPreferenceValue = _clan.GetAuthorityPreferenceValue ();

		if (authorityPreferenceValue <= 0)
			return 1;

		float modMinAdministrativeLoad = MinAdministrativeLoad * cohesivenessPreferenceValue;
		float modMaxAdministrativeLoad = MaxAdministrativeLoad * _clan.CurrentLeader.Wisdom * _clan.CurrentLeader.Charisma * authorityPreferenceValue * cohesivenessPreferenceValue;

		float chance = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);

//		#if DEBUG
////		if (Clan.Name.Text == "Nuse-zis") {
//		Debug.Log ("Clan \"" + Clan.Name.Text + "\" administrative load: " + administrativeLoad + ", chance of splitting: " + chance);
////		}
//		#endif

		return Mathf.Clamp01 (chance);
	}

	public override void Trigger () {

		bool preferSplit = _clan.GetNextLocalRandomFloat (RngOffsets.CLAN_SPLITTING_EVENT_PREFER_SPLIT) < _chanceOfSplitting;

		if (_clan.Polity.IsUnderPlayerFocus || _clan.IsUnderPlayerGuidance) {

			Decision splitDecision;

			if (_chanceOfSplitting >= 1) {
				splitDecision = new ClanSplitDecision (_clan, _splitFactionCoreGroup); // Player can't prevent splitting from happening
			} else {
				splitDecision = new ClanSplitDecision (_clan, _splitFactionCoreGroup, preferSplit); // Give player options
			}

			if (_clan.IsUnderPlayerGuidance) {

				World.AddDecisionToResolve (splitDecision);

			} else {

				splitDecision.ExecutePreferredOption ();
			}

		} else if (preferSplit) {

			_clan.SetToSplit (_splitFactionCoreGroup);
		}

		World.AddFactionToUpdate (Faction);
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		_clan.SplitDecisionEvent = this;
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Faction != null) && (Faction.StillPresent)) {

			Clan clan = Faction as Clan;

			clan.SplitDecisionEvent = this;

			clan.SplitDecisionEventDate = CalculateTriggerDate (clan);

			Reset (clan.SplitDecisionEventDate);

			World.InsertEventToHappen (this);
		}
	}
}

