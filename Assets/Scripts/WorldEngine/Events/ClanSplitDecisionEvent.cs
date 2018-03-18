using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

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
