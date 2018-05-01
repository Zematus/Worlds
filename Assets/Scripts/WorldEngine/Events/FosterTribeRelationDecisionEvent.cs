using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FosterTribeRelationDecisionEvent : PolityEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 5;

	public const int SourceTribeMaxContactStrength = 1000;
	public const int SourceTribeMinContactStrength = 10;
	public const int SourceTribeContactStrengthSpan = SourceTribeMaxContactStrength - SourceTribeMinContactStrength;

	public const int TargetTribeMaxContactStrength = 1000;
	public const int TargetTribeMinContactStrength = 10;
	public const int TargetTribeContactStrengthSpan = TargetTribeMaxContactStrength - TargetTribeMinContactStrength;

	public const float DecisionChanceFactor = 0.5f;

	private PolityContact _targetContact;

	private Tribe _targetTribe;
	private Tribe _sourceTribe;

	private Clan _originalSourceDominantClan;
	private Clan _targetDominantClan;

	private float _chanceOfMakingAttempt;
	private float _chanceOfRejectingOffer;

	public FosterTribeRelationDecisionEvent () {

		DoNotSerialize = true;
	}

	public FosterTribeRelationDecisionEvent (Tribe sourceTribe, PolityEventData data) : base (sourceTribe, data) {

		_sourceTribe = sourceTribe;
		_originalSourceDominantClan = World.GetFaction (data.OriginalDominantFactionId) as Clan;

		DoNotSerialize = true;
	}

	public FosterTribeRelationDecisionEvent (Tribe sourceTribe, long triggerDate) : base (sourceTribe, triggerDate, FosterTribeRelationDecisionEventId) {

		_sourceTribe = sourceTribe;
		_originalSourceDominantClan = sourceTribe.DominantFaction as Clan;

		DoNotSerialize = true;
	}

	public static long CalculateTriggerDate (Tribe tribe) {

		float randomFactor = tribe.GetNextLocalRandomFloat (RngOffsets.FOSTER_TRIBE_RELATION_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float isolationPreferenceValue = tribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);

		float isoloationPrefFactor = 2 * isolationPreferenceValue;
		isoloationPrefFactor = Mathf.Pow (isoloationPrefFactor, 4);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant * isoloationPrefFactor;

		long triggerDateSpan = (long)dateSpan + CellGroup.GenerationSpan;

		if (triggerDateSpan < 0) {
			#if DEBUG
			Debug.LogWarning ("updateSpan less than 0: " + triggerDateSpan);
			#endif

			triggerDateSpan = CellGroup.MaxUpdateSpan;
		}

		return tribe.World.CurrentDate + triggerDateSpan;
	}

	public float GetContactWeight (PolityContact contact) {

		if (contact.Polity is Tribe)
			return _sourceTribe.CalculateContactStrength (contact);

		return 0;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		if (_sourceTribe.DominantFaction != OriginalDominantFaction)
			return false;

		int rngOffset = (int)(RngOffsets.EVENT_CAN_TRIGGER + Id);

		_targetContact = _sourceTribe.GetRandomPolityContact (rngOffset++, GetContactWeight, true);

		if (_targetContact == null)
			return false;

		_targetTribe = _targetContact.Polity as Tribe;
		_targetDominantClan = _targetTribe.DominantFaction as Clan;

		// We should use the latest cultural attribute values before calculating chances
		_originalSourceDominantClan.PreUpdate ();
		_targetDominantClan.PreUpdate ();

		_chanceOfRejectingOffer = CalculateChanceOfRejectingAttempt ();

		_chanceOfMakingAttempt = CalculateChanceOfMakingAttempt ();

		if (_chanceOfMakingAttempt <= 0) {

			return false;
		}

		return true;
	}

	public float CalculateChanceOfRejectingAttempt () {

		float contactStrength = _targetTribe.CalculateContactStrength (_sourceTribe);

		if (contactStrength <= 0)
			return 0;

		float isolationPreferenceValue = _targetTribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);

		if (isolationPreferenceValue >= 1)
			return 1;

		float isolationPrefFactor = 2 * isolationPreferenceValue;
		isolationPrefFactor = Mathf.Pow (isolationPrefFactor, 4);

		float relationshipValue = _targetTribe.GetRelationshipValue (_sourceTribe);

		if (relationshipValue <= 0)
			return 1;

		float relationshipFactor = 2 * (1 - relationshipValue);
		relationshipFactor = Mathf.Pow (relationshipFactor, 4);

		float factors = isolationPrefFactor * relationshipFactor * DecisionChanceFactor;

		float modMinContactStrength = TargetTribeMinContactStrength * factors;
		float modMaxContactStrength = TargetTribeMaxContactStrength * factors;

		float chance = 1 - (contactStrength - modMinContactStrength) / (modMaxContactStrength - modMinContactStrength);

		return Mathf.Clamp01 (chance);
	}

	public float CalculateChanceOfMakingAttempt () {

		float contactStrength = _sourceTribe.CalculateContactStrength (_targetTribe);

		if (contactStrength <= 0)
			return 0;

		float isolationPreferenceValue = _sourceTribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);

		if (isolationPreferenceValue >= 1)
			return 1;

		float isolationPrefFactor = 2 * isolationPreferenceValue;
		isolationPrefFactor = Mathf.Pow (isolationPrefFactor, 4);

		float relationshipValue = _sourceTribe.GetRelationshipValue (_targetTribe);

		if (relationshipValue <= 0)
			return 1;

		float relationshipFactor = 2 * (1 - relationshipValue);
		relationshipFactor = Mathf.Pow (relationshipFactor, 4);

		float factors = isolationPrefFactor * relationshipFactor * DecisionChanceFactor;

		float modMinContactStrength = SourceTribeMinContactStrength * factors;
		float modMaxContactStrength = SourceTribeMaxContactStrength * factors;

		float chance = 1 - (contactStrength - modMinContactStrength) / (modMaxContactStrength - modMinContactStrength);

		return Mathf.Clamp01 (chance);
	}

	public override void Trigger () {

		bool attemptFoster = _targetTribe.GetNextLocalRandomFloat (RngOffsets.FOSTER_TRIBE_RELATION_EVENT_MAKE_ATTEMPT) < _chanceOfMakingAttempt;

		if (_sourceTribe.IsUnderPlayerFocus || _originalSourceDominantClan.IsUnderPlayerGuidance) {

			Decision fosterDecision = new FosterTribeRelationDecision (_targetTribe, _sourceTribe, attemptFoster, _chanceOfRejectingOffer);

			if (_originalSourceDominantClan.IsUnderPlayerGuidance) {

				World.AddDecisionToResolve (fosterDecision);

			} else {

				fosterDecision.ExecutePreferredOption ();
			}

		} else if (attemptFoster) {

			FosterTribeRelationDecision.LeaderAttemptsFosterRelationship (_targetTribe, _sourceTribe, _chanceOfRejectingOffer);

		} else {

			FosterTribeRelationDecision.LeaderAvoidsFosteringRelationship (_targetTribe, _sourceTribe);
		}
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		_sourceTribe = Polity as Tribe;
		_originalSourceDominantClan = OriginalDominantFaction as Clan;

		_sourceTribe.AddEvent (this);
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Polity != null) && (Polity.StillPresent)) {

			Tribe tribe = Polity as Tribe;

			tribe.ResetEvent (WorldEvent.FosterTribeRelationDecisionEventId, CalculateTriggerDate (tribe));
		}
	}
}
