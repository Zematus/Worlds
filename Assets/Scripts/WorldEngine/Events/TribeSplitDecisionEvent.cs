using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TribeSplitDecisionEvent : FactionEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 20;

	public const int SplitClanMaxAdministrativeLoad = 500000;
	public const int SplitClanMinAdministrativeLoad = 100000;

	public const int TribeMaxAdministrativeLoad = 500000;
	public const int TribeMinAdministrativeLoad = 100000;

	public const float MaxAdministrativeLoadChanceFactor = 0.05f;

	public const float MinCoreDistance = 200f;

	private Clan _splitClan;

	private Tribe _tribe;

	private float _splitClanChanceOfSplitting;
	private float _tribeChanceOfSplitting;

	public TribeSplitDecisionEvent () {

		DoNotSerialize = true;
	}

	public TribeSplitDecisionEvent (Clan splitClan, long originalTribeId, long triggerDate) : base (splitClan, originalTribeId, triggerDate, TribeSplitEventId) {

		_splitClan = splitClan;
		_tribe = World.GetPolity (originalTribeId) as Tribe;

		DoNotSerialize = true;
	}

	public TribeSplitDecisionEvent (Clan splitClan, long triggerDate) : base (splitClan, triggerDate, TribeSplitEventId) {

		_splitClan = splitClan;
		_tribe = splitClan.Polity as Tribe;

		DoNotSerialize = true;
	}

	public static long CalculateTriggerDate (Clan clan) {

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.TRIBE_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		Clan dominantClan = clan.Polity.DominantFaction as Clan;

		float administrativeLoad = dominantClan.CalculateAdministrativeLoad ();

		float loadFactor = 1;

		if (administrativeLoad != Mathf.Infinity) {

			float modAdminLoad = Mathf.Max (0, administrativeLoad - SplitClanMinAdministrativeLoad);
			float modHalfFactorAdminLoad = SplitClanMaxAdministrativeLoad - SplitClanMinAdministrativeLoad;

			loadFactor = modHalfFactorAdminLoad / (modAdminLoad + modHalfFactorAdminLoad);
		}

		float cohesivenessPreferenceValue = clan.GetPreferenceValue (CulturalPreference.CohesivenessPreferenceId);

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

		if (_splitClan.Polity != OriginalPolity)
			return false;

		if (_splitClan.IsDominant)
			return false;

		CellGroup clanCoreGroup = _splitClan.CoreGroup;

		PolityInfluence polityInfluence = clanCoreGroup.GetPolityInfluence (OriginalPolity);

		if (clanCoreGroup.HighestPolityInfluence != polityInfluence)
			return false;

		float prominence = _splitClan.Prominence;

//		if (prominence < MinProminenceTrigger) {
//
//			return false;
//		}

		float polityCoreDistance = (polityInfluence.PolityCoreDistance * prominence) - MinCoreDistance;

		if (polityCoreDistance <= 0)
			return false;

		_tribeChanceOfSplitting = CalculateChanceOfSplittingForTribe ();

		if (_tribeChanceOfSplitting <= 0) {

			return false;
		}

		_splitClanChanceOfSplitting = CalculateChanceOfSplittingForSplitClan ();

		if (_splitClan.IsUnderPlayerGuidance && _splitClanChanceOfSplitting < 0.5f) {
		
			return false;
		}

		if (_splitClanChanceOfSplitting <= 0) {

			return false;
		}

		return true;
	}

	public float CalculateChanceOfSplittingForSplitClan () {

		Clan dominantClan = _splitClan.Polity.DominantFaction as Clan;

		float administrativeLoad = dominantClan.CalculateAdministrativeLoad ();

		if (administrativeLoad == Mathf.Infinity)
			return 1;

		float cohesivenessPreferenceValue = _splitClan.GetPreferenceValue (CulturalPreference.CohesivenessPreferenceId);

		if (cohesivenessPreferenceValue <= 0)
			return 1;

		float cohesivenessPrefFactor = 2 * cohesivenessPreferenceValue;
		cohesivenessPrefFactor = Mathf.Pow (cohesivenessPrefFactor, 4);

		float authorityPreferenceValue = _splitClan.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

		if (authorityPreferenceValue <= 0)
			return 1;

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

		float relationshipFactor = 2 * _splitClan.GetRelationshipValue (dominantClan);
		relationshipFactor = Mathf.Pow (relationshipFactor, 4);

		float diffLimitsAdministrativeLoad = SplitClanMaxAdministrativeLoad - SplitClanMinAdministrativeLoad;

		float modMinAdministrativeLoad = SplitClanMinAdministrativeLoad * cohesivenessPrefFactor * relationshipFactor;
		float modMaxAdministrativeLoad = modMinAdministrativeLoad + 
			(diffLimitsAdministrativeLoad * dominantClan.CurrentLeader.Wisdom * dominantClan.CurrentLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

		float chance = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);

		return Mathf.Clamp01 (chance);
	}

	public float CalculateChanceOfSplittingForTribe () {

		Clan dominantClan = _splitClan.Polity.DominantFaction as Clan;

		float administrativeLoad = dominantClan.CalculateAdministrativeLoad ();

		if (administrativeLoad == Mathf.Infinity)
			return 1;

		float cohesivenessPreferenceValue = _tribe.GetPreferenceValue (CulturalPreference.CohesivenessPreferenceId);

		if (cohesivenessPreferenceValue <= 0)
			return 1;

		float cohesivenessPrefFactor = 2 * cohesivenessPreferenceValue;
		cohesivenessPrefFactor = Mathf.Pow (cohesivenessPrefFactor, 4);

		float authorityPreferenceValue = _tribe.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

		if (authorityPreferenceValue <= 0)
			return 1;

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

		float relationshipFactor = 2 * dominantClan.GetRelationshipValue (_splitClan);
		relationshipFactor = Mathf.Pow (relationshipFactor, 4);

		float diffLimitsAdministrativeLoad = TribeMaxAdministrativeLoad - TribeMinAdministrativeLoad;

		float modMinAdministrativeLoad = TribeMinAdministrativeLoad * cohesivenessPrefFactor * relationshipFactor;
		float modMaxAdministrativeLoad = modMinAdministrativeLoad + 
			(diffLimitsAdministrativeLoad * dominantClan.CurrentLeader.Wisdom * dominantClan.CurrentLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

		float chance = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);

		return Mathf.Clamp01 (chance);
	}

	private void SplitClanAllowedSplitEffect () {

		Clan dominantClan = _splitClan.Polity.DominantFaction as Clan;

		bool tribePreferSplit = _tribe.GetNextLocalRandomFloat (RngOffsets.TRIBE_SPLITTING_EVENT_TRIBE_PREFER_SPLIT) < _tribeChanceOfSplitting;

		if (_tribe.IsUnderPlayerFocus || dominantClan.IsUnderPlayerGuidance) {

			Decision tribeDecision;

			if (_splitClanChanceOfSplitting >= 1) {
				tribeDecision = new TribeSplitDecision (_tribe, _splitClan); // Player that controls dominant clan can't prevent splitting from happening
			} else {
				tribeDecision = new TribeSplitDecision (_tribe, _splitClan, tribePreferSplit); // Give player options
			}

			if (dominantClan.IsUnderPlayerGuidance) {

				World.AddDecisionToResolve (tribeDecision);

			} else {

				tribeDecision.ExecutePreferredOption ();
			}

		} else if (tribePreferSplit) {

			TribeSplitDecision.LeaderAllowsSplit (_splitClan, _tribe);

		} else {

			TribeSplitDecision.LeaderPreventsSplit (_splitClan, _tribe);
		}

		World.AddFactionToUpdate (dominantClan);

		World.AddPolityToUpdate (_tribe);
	}

	public override void Trigger () {

		bool splitClanPreferSplit = _splitClan.GetNextLocalRandomFloat (RngOffsets.TRIBE_SPLITTING_EVENT_SPLITCLAN_PREFER_SPLIT) < _splitClanChanceOfSplitting;

		if (_tribe.IsUnderPlayerFocus || _splitClan.IsUnderPlayerGuidance) {

			Decision splitClanDecision;

			if (_splitClanChanceOfSplitting >= 1) {
				splitClanDecision = new ClanSplitFromTribeDecision (_tribe, _splitClan, SplitClanAllowedSplitEffect); // Player that controls split clan can't prevent splitting from happening
			} else {
				splitClanDecision = new ClanSplitFromTribeDecision (_tribe, _splitClan, splitClanPreferSplit, SplitClanAllowedSplitEffect); // Give player options
			}

			if (_splitClan.IsUnderPlayerGuidance) {

				World.AddDecisionToResolve (splitClanDecision);

			} else {

				splitClanDecision.ExecutePreferredOption ();
			}

		} else if (splitClanPreferSplit) {

			ClanSplitFromTribeDecision.LeaderAllowsSplit (_splitClan, _tribe, SplitClanAllowedSplitEffect);

		} else {

			ClanSplitFromTribeDecision.LeaderPreventsSplit (_splitClan, _tribe);
		}

		World.AddFactionToUpdate (_splitClan);
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Faction != null) && (Faction.StillPresent)) {
			
			_splitClan.TribeSplitDecisionEvent = this;

			_splitClan.TribeSplitDecisionEventDate = CalculateTriggerDate (_splitClan);
			_splitClan.TribeSplitDecisionOriginalTribeId = OriginalPolityId;

			Reset (_splitClan.TribeSplitDecisionEventDate);

			World.InsertEventToHappen (this);
		}
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		_splitClan = Faction as Clan;

		_splitClan.TribeSplitDecisionEvent = this;
	}
}
