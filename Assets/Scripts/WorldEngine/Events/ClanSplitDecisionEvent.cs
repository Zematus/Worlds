using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ClanSplitDecisionEvent : FactionEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 10;
	
	public const float MinInfluenceTrigger = 0.3f;
	public const float MinCoreDistance = 1000f;
	public const float MinCoreProminenceValue = 0.5f;

	public const int MaxAdministrativeLoad = 500000;
	public const int MinAdministrativeLoad = 100000;
	public const int AdministrativeLoadSpan = MaxAdministrativeLoad - MinAdministrativeLoad;

	public const float MaxAdministrativeLoadChanceFactor = 0.05f;

	private Clan _clan;

	private CellGroup _newClanCoreGroup;

	private float _chanceOfSplitting;

	public ClanSplitDecisionEvent () {

		DoNotSerialize = true;
	}

	public ClanSplitDecisionEvent (Clan clan, long originalTribeId, long triggerDate) : base (clan, originalTribeId, triggerDate, ClanSplitDecisionEventId) {

		_clan = clan;

		DoNotSerialize = true;
	}

	public ClanSplitDecisionEvent (Clan clan, FactionEventData data) : base (clan, data) {

		_clan = clan;

		DoNotSerialize = true;
	}

	public ClanSplitDecisionEvent (Clan clan, long triggerDate) : base (clan, triggerDate, ClanSplitDecisionEventId) {

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

			loadFactor = AdministrativeLoadSpan / (modAdminLoad + AdministrativeLoadSpan);
		}

		float cohesionPreferenceValue = clan.GetPreferenceValue (CulturalPreference.CohesionPreferenceId);

		float cohesionPrefFactor = 2 * cohesionPreferenceValue;
		cohesionPrefFactor = Mathf.Pow (cohesionPrefFactor, 4);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant * loadFactor * cohesionPrefFactor;

		long triggerDateSpan = (long)dateSpan + CellGroup.GenerationSpan;

		if (triggerDateSpan < 0) {
			#if DEBUG
			Debug.LogWarning ("updateSpan less than 0: " + triggerDateSpan);
			#endif

			triggerDateSpan = CellGroup.MaxUpdateSpan;
		}

		return clan.World.CurrentDate + triggerDateSpan;
	}

	public float GetGroupWeight (CellGroup group) {

		if (group == _clan.CoreGroup)
			return 0;

		PolityProminence pi = group.GetPolityProminence (_clan.Polity);

		if (group.HighestPolityProminence != pi)
			return 0;

		if (!Clan.CanBeClanCore (group))
			return 0;

		float coreDistance = pi.FactionCoreDistance - MinCoreDistance;

		if (coreDistance <= 0)
			return 0;

		float coreDistanceFactor = MinCoreDistance / (MinCoreDistance + coreDistance);

		float minCoreProminenceValue = MinCoreProminenceValue * coreDistanceFactor;

		float value = pi.Value - minCoreProminenceValue;

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

		if (_clan.Influence < MinInfluenceTrigger) {
			
			return false;
		}

		int rngOffset = (int)(RngOffsets.EVENT_CAN_TRIGGER + Id);

		_newClanCoreGroup = _clan.Polity.GetRandomGroup (rngOffset++, GetGroupWeight, true);

		if (_newClanCoreGroup == null) {
			
			return false;
		}

		// We should use the latest cultural attribute values before calculating chances
		_clan.PreUpdate ();

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

		float cohesionPreferenceValue = _clan.GetPreferenceValue (CulturalPreference.CohesionPreferenceId);

		if (cohesionPreferenceValue <= 0)
			return 1;

		float cohesionPrefFactor = 2 * cohesionPreferenceValue;
		cohesionPrefFactor = Mathf.Pow (cohesionPrefFactor, 4);

		float authorityPreferenceValue = _clan.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

		if (authorityPreferenceValue <= 0)
			return 1;

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

		float modMinAdministrativeLoad = MinAdministrativeLoad * cohesionPrefFactor;
		float modMaxAdministrativeLoad = modMinAdministrativeLoad + (AdministrativeLoadSpan * _clan.CurrentLeader.Wisdom * _clan.CurrentLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

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
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		_clan = Faction as Clan;

		_clan.AddEvent (this);
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Faction != null) && (Faction.StillPresent)) {

			Clan clan = Faction as Clan;

			clan.ResetEvent (WorldEvent.ClanSplitDecisionEventId, CalculateTriggerDate (clan));
		}
	}
}
