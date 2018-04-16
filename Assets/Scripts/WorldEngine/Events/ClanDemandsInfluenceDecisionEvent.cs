using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ClanDemandsInfluenceDecisionEvent : FactionEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 5;

	public const int MaxAdministrativeLoad = 5000000;
	public const int MinAdministrativeLoad = 100000;
	public const int AdministrativeLoadSpan = MaxAdministrativeLoad - MinAdministrativeLoad;

	public const float MaxAdministrativeLoadChanceFactor = 0.05f;

	private Clan _clan;

	private float _chanceOfTriggering;

	public ClanDemandsInfluenceDecisionEvent () {

		DoNotSerialize = true;
	}

	public ClanDemandsInfluenceDecisionEvent (Clan clan, long originalTribeId, long triggerDate) : base (clan, originalTribeId, triggerDate, ClanDemandsInflenceDecisionEventId) {

		_clan = clan;

		DoNotSerialize = true;
	}

	public ClanDemandsInfluenceDecisionEvent (Clan clan, FactionEventData data) : base (clan, data) {

		_clan = clan;

		DoNotSerialize = true;
	}

	public ClanDemandsInfluenceDecisionEvent (Clan clan, long triggerDate) : base (clan, triggerDate, ClanDemandsInflenceDecisionEventId) {

		_clan = clan;

		DoNotSerialize = true;
	}

	public static long CalculateTriggerDate (Clan clan) {

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.CLAN_DEMANDS_INFLUENCE_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float administrativeLoad = clan.CalculateAdministrativeLoad ();

		float loadFactor = 1;

		if (administrativeLoad != Mathf.Infinity) {

			float modAdminLoad = Mathf.Max (0, administrativeLoad - MinAdministrativeLoad);

			loadFactor = 1 - AdministrativeLoadSpan / (modAdminLoad + AdministrativeLoadSpan);
		}

		float authorityPreferenceValue = clan.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

		float authorityPrefFactor = 2 * (1 - authorityPreferenceValue);
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant * loadFactor * authorityPrefFactor;

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

		if (!base.CanTrigger ()) {

			return false;
		}

		Faction dominantFaction = _clan.Polity.DominantFaction;

		if (dominantFaction == _clan)
			return false;

		if (!(dominantFaction is Clan))
			return false;

//		int rngOffset = (int)(RngOffsets.EVENT_CAN_TRIGGER + Id);

		// We should use the latest cultural attribute values before calculating chances
		_clan.PreUpdate ();

		_chanceOfTriggering = CalculateChanceOfTriggering ();

		if (_chanceOfTriggering <= 0) {

			return false;
		}

		return true;
	}

	public float CalculateChanceOfTriggering () {

		Faction dominantFaction = _clan.Polity.DominantFaction;

		float administrativeLoad = _clan.CalculateAdministrativeLoad ();

		if (administrativeLoad == Mathf.Infinity)
			return 0;

		float cohesionPreferenceValue = _clan.GetPreferenceValue (CulturalPreference.CohesionPreferenceId);

		if (cohesionPreferenceValue <= 0)
			return 0;

		float cohesionPrefFactor = 2 * cohesionPreferenceValue;
		cohesionPrefFactor = Mathf.Pow (cohesionPrefFactor, 4);

		float authorityPreferenceValue = _clan.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

		if (authorityPreferenceValue <= 0)
			return 0;

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

		float relationshipValue = _clan.GetRelationshipValue (dominantFaction);

		if (relationshipValue >= 1)
			return 0;

		float relationshipFactor = 2 * (1 - relationshipValue);
		relationshipFactor = Mathf.Pow (relationshipFactor, 4);

		float influenceDeltaValue = dominantFaction.Influence - _clan.Influence;

		if (influenceDeltaValue <= 0)
			return 0;

		if (influenceDeltaValue >= 1)
			return 1;

		float influenceFactor = 2 * influenceDeltaValue;
		influenceFactor = Mathf.Pow (influenceFactor, 4);

		float factors = cohesionPrefFactor * authorityPrefFactor * relationshipFactor * influenceFactor * MaxAdministrativeLoadChanceFactor;

		float modMinAdministrativeLoad = MinAdministrativeLoad * factors;
		float modMaxAdministrativeLoad = MaxAdministrativeLoad * factors;

		float chance = 1 - (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);

		return Mathf.Clamp01 (chance);
	}

	public override void Trigger () {

		bool preferSplit = _clan.GetNextLocalRandomFloat (RngOffsets.CLAN_SPLITTING_EVENT_PREFER_SPLIT) < _chanceOfTriggering;

		if (_clan.Polity.IsUnderPlayerFocus || _clan.IsUnderPlayerGuidance) {

			Decision splitDecision;

			if (_chanceOfTriggering >= 1) {
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
