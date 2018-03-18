using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

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
