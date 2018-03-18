using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

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

		return leader.Name.BoldText + " has prevented clan " +  Faction.Name.BoldText + " from splitting";
	}
}

public class ClanSplitEventMessage : FactionEventMessage {

	[XmlAttribute]
	public long OldClanId;

	public ClanSplitEventMessage () {

	}

	public ClanSplitEventMessage (Clan oldClan, Clan newClan, long date) : base (newClan, WorldEvent.ClanSplitEventId, date) {

		OldClanId = oldClan.Id;
	}

	protected override string GenerateMessage ()
	{
		Faction oldClan = World.GetFaction (OldClanId);

		return "A new clan, " + Faction.Name.BoldText + ", has split from clan " +  oldClan.Name.BoldText;
	}
}

public class ClanSplitDecision : FactionDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float SplitClanMinProminence = 0.25f;
	public const float SplitClanMaxProminence = 0.50f;

	private Clan _clan;

	private bool _cantPrevent = false;
	private bool _preferSplit = true;

	private CellGroup _newCoreGroup;

	private static string GenerateDescriptionIntro (Clan clan) {

		return "Several minor bands within clan " + clan.Name.BoldText + " have become too distant and seem to be tryin to form their own clan. ";
	}

	public ClanSplitDecision (Clan clan, CellGroup newCoreGroup) : base (clan) {

		_clan = clan;

		Description = GenerateDescriptionIntro (clan) +
			"Unfortunately, " + clan.CurrentLeader.Name.BoldText + " can't do anything about it under the current circumstances...";

		_cantPrevent = true;

		_newCoreGroup = newCoreGroup;
	}

	public ClanSplitDecision (Clan clan, CellGroup newCoreGroup, bool preferSplit) : base (clan) {

		_clan = clan;

		Description = GenerateDescriptionIntro (clan) +
			"Should the clan leader, " + clan.CurrentLeader.Name.BoldText + ", try to reach out to them to keep them from splitting into their own clan?";

		_preferSplit = preferSplit;

		_newCoreGroup = newCoreGroup;
	}

	private string GeneratePreventSplitResultMessage () {

		float charismaFactor = _clan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _clan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPreferencePercentChange = BaseMinPreferencePercentChange / attributesFactor;
		float maxPreferencePercentChange = BaseMaxPreferencePercentChange / attributesFactor;

		float prefValue = _clan.GetAuthorityPreferenceValue ();

		float minPrefChange = MathUtility.DecreaseByPercent (prefValue, minPreferencePercentChange);
		float maxPrefChange = MathUtility.DecreaseByPercent (prefValue, maxPreferencePercentChange);

		string authorityPreferenceChangeStr = "\t• Clan " + _clan.Name.BoldText + ": authority preference (" + prefValue.ToString ("0.00")
			+ ") decreases to: " + minPrefChange.ToString ("0.00") + " - " + maxPrefChange.ToString ("0.00");

		minPreferencePercentChange = BaseMinPreferencePercentChange * attributesFactor;
		maxPreferencePercentChange = BaseMaxPreferencePercentChange * attributesFactor;

		prefValue = _clan.GetCohesivenessPreferenceValue ();

		minPrefChange = MathUtility.IncreaseByPercent (prefValue, minPreferencePercentChange);
		maxPrefChange = MathUtility.IncreaseByPercent (prefValue, maxPreferencePercentChange);

		string cohesivenessPreferenceChangeStr = "\t• Clan " + _clan.Name.BoldText + ": cohesiveness preference (" + prefValue.ToString ("0.00")
			+ ") increases to: " + minPrefChange.ToString ("0.00") + " - " + maxPrefChange.ToString ("0.00");

		return authorityPreferenceChangeStr + "\n" + cohesivenessPreferenceChangeStr;
	}

	public static void LeaderPreventsSplit (Clan clan) {

		float charismaFactor = clan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = clan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		int rngOffset = RngOffsets.CLAN_SPLITTING_EVENT_LEADER_PREVENTS_MODIFY_ATTRIBUTE;

		float randomFactor = clan.GetNextLocalRandomFloat (rngOffset++);
		float authorityPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
		authorityPreferencePercentChange /= attributesFactor;

		randomFactor = clan.GetNextLocalRandomFloat (rngOffset++);
		float cohesivenessPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
		cohesivenessPreferencePercentChange *= attributesFactor;

		clan.DecreaseAuthorityPreferenceValue (authorityPreferencePercentChange);
		clan.IncreaseCohesivenessPreferenceValue (cohesivenessPreferencePercentChange);

		clan.World.AddFactionToUpdate (clan);

		// Should reduce respect for authority and increase cohesiveness
		clan.Polity.AddEventMessage (new PreventClanSplitEventMessage (clan, clan.CurrentLeader, clan.World.CurrentDate));
	}

	private void PreventSplit () {
		
		LeaderPreventsSplit (_clan);
	}

	private string GenerateAllowSplitResultMessage () {

		float minProminence;
		float maxProminence;

		CalculateMinMaxProminence (_clan, out minProminence, out maxProminence);

		string message;

		float clanProminence = _clan.Prominence;
		float minNewClanProminence = clanProminence - minProminence;
		float maxNewClanProminence = clanProminence - maxProminence;

		message = "\t• Clan " + _clan.Name.BoldText + ": prominence (" + clanProminence.ToString ("P") 
			+ ") decreases to " + minNewClanProminence.ToString ("P") + " - " + maxNewClanProminence.ToString ("P");
		message += "\n\t• A new clan with prominence " + minProminence.ToString ("P") + " - " + maxProminence.ToString ("P") + " splits from " + _clan.Name.BoldText;

		return message;
	}

	private void AllowSplit () {

		LeaderAllowsSplit (_clan, _newCoreGroup);
	}

	public override Option[] GetOptions () {

		if (_cantPrevent) {
			
			return new Option[] {
				new Option ("Oh well...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
			};
		}

		return new Option[] {
			new Option ("Allow clan to split in two...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
			new Option ("Prevent clan from splitting...", "Effects:\n" + GeneratePreventSplitResultMessage (), PreventSplit)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_preferSplit)
			AllowSplit ();
		else
			PreventSplit ();
	}

	public static void LeaderAllowsSplit (Clan clan, CellGroup newClanCoreGroup) {

		float minProminence;
		float maxProminence;

		CalculateMinMaxProminence (clan, out minProminence, out maxProminence);

		clan.SetToSplit (newClanCoreGroup, minProminence, maxProminence);
	}

	public static void CalculateMinMaxProminence (Clan clan, out float minProminence, out float maxProminence) {

		float charismaFactor = clan.CurrentLeader.Charisma / 10f;
		float cultureModifier = 1 + (charismaFactor * clan.GetCohesivenessPreferenceValue ());

		minProminence = clan.Prominence * SplitClanMinProminence / cultureModifier;
		maxProminence = clan.Prominence * SplitClanMaxProminence / cultureModifier;
	}
}

public class ClanCoreMigrationEvent : FactionEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 500;

	private CellGroup _targetGroup;

	public ClanCoreMigrationEvent () {

		DoNotSerialize = true;
	}

	public ClanCoreMigrationEvent (Clan clan, long originalTribeId, long triggerDate) : base (clan, originalTribeId, triggerDate, ClanCoreMigrationEventId) {

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
			clan.CoreMigrationEventOriginalTribeId = OriginalPolityId;

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

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 40;
	
	public const float MinProminenceTrigger = 0.3f;
	public const float MinCoreDistance = 1000f;
	public const float MinCoreInfluenceValue = 0.5f;

	public const int MaxAdministrativeLoad = 1000000;
	public const int MinAdministrativeLoad = 200000;

	public const float MaxAdministrativeLoadChanceFactor = 0.05f;

	private Clan _clan;

	private CellGroup _newClanCoreGroup;

	private float _chanceOfSplitting;

	public ClanSplitDecisionEvent () {

		DoNotSerialize = true;
	}

	public ClanSplitDecisionEvent (Clan clan, long originalTribeId, long triggerDate) : base (clan, originalTribeId, triggerDate, ClanSplitEventId) {

		_clan = clan;

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

		float cohesivenessPrefFactor = 2 * cohesivenessPreferenceValue;
		cohesivenessPrefFactor = Mathf.Pow (cohesivenessPrefFactor, 4);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant * loadFactor * cohesivenessPrefFactor;

		long triggerDateSpan = (long)dateSpan + CellGroup.GenerationSpan;

		if (triggerDateSpan < 0) {
			#if DEBUG
			Debug.LogWarning ("updateSpan less than 0: " + triggerDateSpan);
			#endif

			triggerDateSpan = CellGroup.MaxUpdateSpan;
		}

//		#if DEBUG
//		if (clan.Name.BoldedText == "Nuse-zis") {
//			Debug.Log ("Clan \"" + clan.Name.BoldedText + "\" splitting event triggerDate span: " + Manager.GetTimeSpanString (triggerDateSpan));
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

			return false;
		}

		if (_clan.Prominence < MinProminenceTrigger) {
			
			return false;
		}

		int rngOffset = (int)(RngOffsets.EVENT_CAN_TRIGGER + Id);

		_newClanCoreGroup = _clan.Polity.GetRandomGroup (rngOffset++, GetGroupWeight, true);

		if (_newClanCoreGroup == null) {
			
			return false;
		}

		_chanceOfSplitting = CalculateChanceOfSplitting ();

		if (_chanceOfSplitting <= 0) {

			return false;
		}

		return true;
	}

	public float CalculateChanceOfSplitting () {

		float administrativeLoad = _clan.CalculateAdministrativeLoad ();

		if (administrativeLoad == Mathf.Infinity)
			return 1;

		float cohesivenessPreferenceValue = _clan.GetCohesivenessPreferenceValue ();

		if (cohesivenessPreferenceValue <= 0)
			return 1;

		float cohesivenessPrefFactor = 2 * cohesivenessPreferenceValue;
		cohesivenessPrefFactor = Mathf.Pow (cohesivenessPrefFactor, 4);

		float authorityPreferenceValue = _clan.GetAuthorityPreferenceValue ();

		if (authorityPreferenceValue <= 0)
			return 1;

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

		float diffLimitsAdministrativeLoad = MaxAdministrativeLoad - MinAdministrativeLoad;

		float modMinAdministrativeLoad = MinAdministrativeLoad * cohesivenessPrefFactor;
		float modMaxAdministrativeLoad = modMinAdministrativeLoad + (diffLimitsAdministrativeLoad * _clan.CurrentLeader.Wisdom * _clan.CurrentLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

		float chance = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);

		return Mathf.Clamp01 (chance);
	}

	public override void Trigger () {

		bool preferSplit = _clan.GetNextLocalRandomFloat (RngOffsets.CLAN_SPLITTING_EVENT_PREFER_SPLIT) < _chanceOfSplitting;

		if (_clan.Polity.IsUnderPlayerFocus || _clan.IsUnderPlayerGuidance) {

			Decision splitDecision;

			if (_chanceOfSplitting >= 1) {
				splitDecision = new ClanSplitDecision (_clan, _newClanCoreGroup); // Player can't prevent splitting from happening
			} else {
				splitDecision = new ClanSplitDecision (_clan, _newClanCoreGroup, preferSplit); // Give player options
			}

			if (_clan.IsUnderPlayerGuidance) {

				World.AddDecisionToResolve (splitDecision);

			} else {

				splitDecision.ExecutePreferredOption ();
			}

		} else if (preferSplit) {

			ClanSplitDecision.LeaderAllowsSplit (_clan, _newClanCoreGroup);

		} else {

			ClanSplitDecision.LeaderPreventsSplit (_clan);
		}

		World.AddFactionToUpdate (Faction);
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		_clan = Faction as Clan;

		_clan.ClanSplitDecisionEvent = this;
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Faction != null) && (Faction.StillPresent)) {

			Clan clan = Faction as Clan;

			clan.ClanSplitDecisionEvent = this;

			clan.ClanSplitDecisionEventDate = CalculateTriggerDate (clan);

			Reset (clan.ClanSplitDecisionEventDate);
			clan.ClanSplitDecisionOriginalTribeId = OriginalPolityId;

			World.InsertEventToHappen (this);
		}
	}
}

public class TribeSplitDecisionEvent : FactionEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 40;

	public const int MaxAdministrativeLoad = 1000000;
	public const int MinAdministrativeLoad = 200000;

	public const float MaxAdministrativeLoadChanceFactor = 0.05f;

	public const int TerminalAdministrativeLoadValue = 500000;

	public const float MinCoreInfluenceValue = 0.3f;

	public const float MinCoreDistance = 200f;

	private Clan _clan;

	private Tribe _tribe;

	private float _chanceOfSplitting;

	public TribeSplitDecisionEvent () {

		DoNotSerialize = true;
	}

	public TribeSplitDecisionEvent (Clan clan, long originalTribeId, long triggerDate) : base (clan, originalTribeId, triggerDate, TribeSplitEventId) {

		_clan = clan;
		_tribe = World.GetPolity (originalTribeId) as Tribe;

		DoNotSerialize = true;
	}

	public TribeSplitDecisionEvent (Clan clan, long triggerDate) : base (clan, triggerDate, TribeSplitEventId) {

		_clan = clan;
		_tribe = clan.Polity as Tribe;

		DoNotSerialize = true;
	}

	public static long CalculateTriggerDate (Clan clan) {

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.TRIBE_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		Clan dominantClan = clan.Polity.DominantFaction as Clan;

		float administrativeLoad = dominantClan.CalculateAdministrativeLoad ();

		float loadFactor = 1;

		if (administrativeLoad != Mathf.Infinity) {

			float modAdminLoad = Mathf.Max (0, administrativeLoad - MinAdministrativeLoad);
			float modHalfFactorAdminLoad = MaxAdministrativeLoad - MinAdministrativeLoad;

			loadFactor = modHalfFactorAdminLoad / (modAdminLoad + modHalfFactorAdminLoad);
		}

		float cohesivenessPreferenceValue = clan.GetCohesivenessPreferenceValue ();

		float cohesivenessPrefFactor = 2 * cohesivenessPreferenceValue;
		cohesivenessPrefFactor = Mathf.Pow (cohesivenessPrefFactor, 4);

		float dateSpan = (1 - randomFactor) *  DateSpanFactorConstant * loadFactor * cohesivenessPrefFactor;

		long triggerDateSpan = (long)dateSpan + CellGroup.GenerationSpan;

		if (triggerDateSpan < 0) {
			#if DEBUG
			Debug.LogWarning ("updateSpan less than 0: " + triggerDateSpan);
			#endif

			triggerDateSpan = CellGroup.MaxUpdateSpan;
		}

		return clan.World.CurrentDate + triggerDateSpan;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		if (_clan.Polity != OriginalPolity)
			return false;

		if (_clan.IsDominant)
			return false;

		CellGroup clanCoreGroup = _clan.CoreGroup;

		PolityInfluence polityInfluence = clanCoreGroup.GetPolityInfluence (OriginalPolity);

		if (clanCoreGroup.HighestPolityInfluence != polityInfluence)
			return false;

		float prominence = _clan.Prominence;

//		if (prominence < MinProminenceTrigger) {
//
//			return false;
//		}

		float polityCoreDistance = (polityInfluence.PolityCoreDistance * prominence) - MinCoreDistance;

		if (polityCoreDistance <= 0)
			return false;

		_chanceOfSplitting = CalculateChanceOfSplitting ();

		if (_clan.IsUnderPlayerGuidance && _chanceOfSplitting < 0.5f) {
		
			return false;
		}

		if (_chanceOfSplitting <= 0) {

			return false;
		}

		return true;
	}

	public float CalculateChanceOfSplitting () {

		Clan dominantClan = _clan.Polity.DominantFaction as Clan;

		float administrativeLoad = dominantClan.CalculateAdministrativeLoad ();

		if (administrativeLoad == Mathf.Infinity)
			return 1;

		float cohesivenessPreferenceValue = _clan.GetCohesivenessPreferenceValue ();

		if (cohesivenessPreferenceValue <= 0)
			return 1;

		float cohesivenessPrefFactor = 2 * cohesivenessPreferenceValue;
		cohesivenessPrefFactor = Mathf.Pow (cohesivenessPrefFactor, 4);

		float authorityPreferenceValue = _clan.GetAuthorityPreferenceValue ();

		if (authorityPreferenceValue <= 0)
			return 1;

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

		float relationshipFactor = 2 * _clan.GetRelationshipValue (dominantClan);
		relationshipFactor = Mathf.Pow (relationshipFactor, 4);

		float diffLimitsAdministrativeLoad = MaxAdministrativeLoad - MinAdministrativeLoad;

		float modMinAdministrativeLoad = MinAdministrativeLoad * cohesivenessPrefFactor * relationshipFactor;
		float modMaxAdministrativeLoad = modMinAdministrativeLoad + 
			(diffLimitsAdministrativeLoad * dominantClan.CurrentLeader.Wisdom * dominantClan.CurrentLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

		float chance = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);

		return Mathf.Clamp01 (chance);
	}

	public override void Trigger () {

		bool preferSplit = _clan.GetNextLocalRandomFloat (RngOffsets.CLAN_SPLITTING_EVENT_PREFER_SPLIT) < _chanceOfSplitting;

		if (_clan.Polity.IsUnderPlayerFocus || _clan.IsUnderPlayerGuidance) {

			Decision splitDecision;

			if (_chanceOfSplitting >= 1) {
				splitDecision = new ClanSplitFromTribeDecision (_tribe, _clan); // Player can't prevent splitting from happening
			} else {
				splitDecision = new ClanSplitFromTribeDecision (_tribe, _clan, preferSplit); // Give player options
			}

			if (_clan.IsUnderPlayerGuidance) {

				World.AddDecisionToResolve (splitDecision);

			} else {

				splitDecision.ExecutePreferredOption ();
			}

		} else if (preferSplit) {

			ClanSplitFromTribeDecision.LeaderAllowsSplit (_clan, _tribe);

		} else {

			ClanSplitFromTribeDecision.LeaderPreventsSplit (_clan, _tribe);
		}

		World.AddFactionToUpdate (Faction);
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Faction != null) && (Faction.StillPresent)) {
			
			_clan.TribeSplitDecisionEvent = this;

			_clan.TribeSplitDecisionEventDate = CalculateTriggerDate (_clan);
			_clan.TribeSplitDecisionOriginalTribeId = OriginalPolityId;

			Reset (_clan.TribeSplitDecisionEventDate);

			World.InsertEventToHappen (this);
		}
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		_clan = Faction as Clan;

		_clan.TribeSplitDecisionEvent = this;
	}
}

public class SplitClanPreventTribeSplitEventMessage : FactionEventMessage {

	[XmlAttribute]
	public long AgentId;

	[XmlAttribute]
	public long TribeId;

	public SplitClanPreventTribeSplitEventMessage () {

	}

	public SplitClanPreventTribeSplitEventMessage (Faction faction, Tribe tribe, Agent agent, long date) : base (faction, WorldEvent.SplitingClanPreventTribeSplitEventId, date) {

		faction.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		TribeId = tribe.Id;
	}

	protected override string GenerateMessage ()
	{
		Agent leader = World.GetMemorableAgent (AgentId);
		Tribe tribe = World.GetPolity (TribeId) as Tribe;

		return leader.Name.BoldText + " has prevented clan " +  Faction.Name.BoldText + " from leaving the " + tribe.Name.BoldText + " Tribe";
	}
}

public class TribeSplitEventMessage : FactionEventMessage {

	[XmlAttribute]
	public long TribeId;

	[XmlAttribute]
	public long NewTribeId;

	public TribeSplitEventMessage () {

	}

	public TribeSplitEventMessage (Clan splitClan, Tribe tribe, Tribe newTribe, long date) : base (splitClan, WorldEvent.TribeSplitEventId, date) {

		TribeId = tribe.Id;
		NewTribeId = newTribe.Id;
	}

	protected override string GenerateMessage ()
	{
		Polity tribe = World.GetPolity (TribeId);
		Polity newTribe = World.GetPolity (NewTribeId);

		return "A new tribe, " + newTribe.Name.BoldText + ", formed by clan " + Faction.Name.BoldText + ", has split from the " +  tribe.Name.BoldText + " Tribe";
	}
}

public class ClanSplitFromTribeDecision : PolityDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	public const float BaseMinProminencePercentChange = 0.05f;
	public const float BaseMaxProminencePercentChange = 0.15f;

	private Tribe _tribe;

	private bool _cantPrevent = false;
	private bool _preferSplit = true;

	private Clan _triggerClan;

	private static string GenerateDescriptionIntro (Tribe tribe, Clan triggerClan) {
		
//		Agent clanLeader = triggerClan.CurrentLeader;

		return 
			"The pressures of distance and strained relationships has made most of the populance under Clan " + triggerClan.Name.BoldText + " to feel that " +
			"they are no longer part of the " + tribe.Name.BoldText + " Tribe and wish for the Clan to become their own tribe.";
	}

	public ClanSplitFromTribeDecision (Tribe tribe, Clan triggerClan) : base (tribe) {

		_tribe = tribe;

		Description = GenerateDescriptionIntro (tribe, triggerClan) +
			"Unfortunately, the pressure is too high for the clan leader, " + triggerClan.CurrentLeader.Name.BoldText + ", to do anything other than to acquiesce " +
			"to demands of " + triggerClan.CurrentLeader.PossessiveNoun + " people...";

		_cantPrevent = true;

		_triggerClan = triggerClan;
	}

	public ClanSplitFromTribeDecision (Tribe tribe, Clan triggerClan, bool preferSplit) : base (tribe) {

		_tribe = tribe;

		Description = GenerateDescriptionIntro (tribe, triggerClan) +
			"Should the clan leader, " + triggerClan.CurrentLeader.Name.BoldText + ", follow the wish of " + triggerClan.CurrentLeader.PossessiveNoun + " people " +
			"and try to create a tribe of their own?";

		_preferSplit = preferSplit;

		_triggerClan = triggerClan;
	}

	private string GeneratePreventSplitResultEffectsString_AuthorityPreference () {

		float charismaFactor = _triggerClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _triggerClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinPreferencePercentChange / attributesFactor;
		float maxPercentChange = BaseMaxPreferencePercentChange / attributesFactor;

		float originalValue = _triggerClan.GetAuthorityPreferenceValue ();

		float minValChange = MathUtility.DecreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.DecreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _triggerClan.Name.BoldText + ": authority preference (" + originalValue.ToString ("0.00") + ") decreases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	private string GeneratePreventSplitResultEffectsString_CohesivenessPreference () {

		float charismaFactor = _triggerClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _triggerClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinPreferencePercentChange * attributesFactor;
		float maxPercentChange = BaseMaxPreferencePercentChange * attributesFactor;

		float originalValue = _triggerClan.GetCohesivenessPreferenceValue ();

		float minValChange = MathUtility.IncreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _triggerClan.Name.BoldText + ": cohesiveness preference (" + originalValue.ToString ("0.00") + ") increases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	private void GeneratePreventSplitResultEffectsString_Prominence (out string effectTriggerClan, out string effectDominantClan) {

		float charismaFactor = _triggerClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _triggerClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinProminencePercentChange / attributesFactor;
		float maxPercentChange = BaseMaxProminencePercentChange / attributesFactor;

		float oldProminenceValue = _triggerClan.Prominence;

		float minValChange = oldProminenceValue * (1f - minPercentChange);
		float maxValChange = oldProminenceValue * (1f - maxPercentChange);

		Clan dominantClan = _tribe.DominantFaction as Clan;

		float oldDominantProminenceValue = dominantClan.Prominence;

		float minValChangeDominant = oldDominantProminenceValue + oldProminenceValue - maxValChange;
		float maxValChangeDominant = oldDominantProminenceValue + oldProminenceValue - minValChange;

		effectTriggerClan = "Clan " + _triggerClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" Tribe (" + oldProminenceValue.ToString ("0.00") + ") decreases to: " + minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");

		effectDominantClan = "Clan " + dominantClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" Tribe (" + oldDominantProminenceValue.ToString ("0.00") + ") increases to: " + minValChangeDominant.ToString ("0.00") + " - " + maxValChangeDominant.ToString ("0.00");
	}

	private string GeneratePreventSplitResultEffectsString_Relationship () {

		Clan dominantClan = _tribe.DominantFaction as Clan;

		float charismaFactor = _triggerClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _triggerClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinRelationshipPercentChange * attributesFactor;
		float maxPercentChange = BaseMaxRelationshipPercentChange * attributesFactor;

		float originalValue = _triggerClan.GetRelationshipValue (dominantClan);

		float minValChange = MathUtility.IncreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _triggerClan.Name.BoldText + ": relationship with Clan " + dominantClan.Name.BoldText + " (" + originalValue.ToString ("0.00") + ") increases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	private string GeneratePreventSplitResultEffectsString () {

		string triggerClanProminenceChangeEffect;
		string dominantClanProminenceChangeEffect;

		GeneratePreventSplitResultEffectsString_Prominence (out triggerClanProminenceChangeEffect, out dominantClanProminenceChangeEffect);

		return 
			"\t• " + GeneratePreventSplitResultEffectsString_AuthorityPreference () + "\n" + 
			"\t• " + GeneratePreventSplitResultEffectsString_CohesivenessPreference () + "\n" + 
			"\t• " + GeneratePreventSplitResultEffectsString_Relationship () + "\n" + 
			"\t• " + triggerClanProminenceChangeEffect + "\n" + 
			"\t• " + dominantClanProminenceChangeEffect;
	}

	public static void LeaderPreventsSplit (Clan splitClan, Tribe tribe) {

		Clan dominantClan = splitClan.Polity.DominantFaction as Clan;

		float charismaFactor = splitClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = splitClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		int rngOffset = RngOffsets.TRIBE_SPLITTING_EVENT_SPLITCLAN_LEADER_PREVENTS_MODIFY_ATTRIBUTE;

		// Authority preference

		float randomFactor = splitClan.GetNextLocalRandomFloat (rngOffset++);
		float authorityPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
		authorityPreferencePercentChange /= attributesFactor;

		splitClan.DecreaseAuthorityPreferenceValue (authorityPreferencePercentChange);

		// Cohesiveness preference

		randomFactor = splitClan.GetNextLocalRandomFloat (rngOffset++);
		float cohesivenessPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
		cohesivenessPreferencePercentChange *= attributesFactor;

		splitClan.IncreaseCohesivenessPreferenceValue (cohesivenessPreferencePercentChange);

		// Prominence

		randomFactor = splitClan.GetNextLocalRandomFloat (rngOffset++);
		float prominencePercentChange = (BaseMaxProminencePercentChange - BaseMinProminencePercentChange) * randomFactor + BaseMinProminencePercentChange;
		prominencePercentChange /= attributesFactor;

		Polity.TransferProminence (splitClan, dominantClan, prominencePercentChange);

		// Relationship

		randomFactor = splitClan.GetNextLocalRandomFloat (rngOffset++);
		float relationshipPercentChange = (BaseMaxRelationshipPercentChange - BaseMinRelationshipPercentChange) * randomFactor + BaseMinRelationshipPercentChange;
		relationshipPercentChange *= attributesFactor;

		float newValue = MathUtility.IncreaseByPercent (splitClan.GetRelationshipValue (dominantClan), relationshipPercentChange);
		Faction.SetRelationship (splitClan, dominantClan, newValue);

		// Updates

		splitClan.World.AddFactionToUpdate (splitClan);
		splitClan.World.AddFactionToUpdate (dominantClan);

		splitClan.World.AddPolityToUpdate (tribe);

		tribe.AddEventMessage (new SplitClanPreventTribeSplitEventMessage (splitClan, tribe, splitClan.CurrentLeader, splitClan.World.CurrentDate));
	}

	private void PreventSplit () {

		LeaderPreventsSplit (_triggerClan, _tribe);
	}

	private string GenerateAllowSplitResultMessage () {

		string message = "\n\t• Clan " + _triggerClan.Name.BoldText + " will attempt to leave the " + _tribe.Name.BoldText + " Tribe and form a tribe of their own";

		return message;
	}

	public static void LeaderAllowsSplit (Clan splitClan, Tribe originalTribe) {

		Tribe newTribe = new Tribe (splitClan, originalTribe);
		newTribe.Initialize ();

		splitClan.World.AddPolity (newTribe);
		splitClan.World.AddPolityToUpdate (newTribe);
		splitClan.World.AddPolityToUpdate (originalTribe);

		originalTribe.AddEventMessage (new TribeSplitEventMessage (splitClan, originalTribe, newTribe, splitClan.World.CurrentDate));
	}

	private void AllowSplit () {

		LeaderAllowsSplit (_triggerClan, _tribe);
	}

	public override Option[] GetOptions () {

		if (_cantPrevent) {

			return new Option[] {
				new Option ("Oh well...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
			};
		}

		return new Option[] {
			new Option ("Allow clan to form a new tribe...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
			new Option ("Prevent clan from leaving tribe...", "Effects:\n" + GeneratePreventSplitResultEffectsString (), PreventSplit)
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

