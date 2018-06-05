using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FosterTribeRelationDecisionEvent : PolityEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 5;

	public const float ContactStrengthFactor = 1f;

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

		_chanceOfMakingAttempt = CalculateChanceOfMakingAttempt ();

		if (_chanceOfMakingAttempt <= 0.15f) {

			return false;
		}

//		#if DEBUG
//		if (_targetTribe.Id == 6993753500213400) {
//			bool debug = true;
//		}
//		#endif

//		#if DEBUG
//		if (_sourceTribe.Id == 6993753500213400) {
//			bool debug = true;
//		}
//		#endif

		_chanceOfRejectingOffer = CalculateChanceOfRejectingOffer ();

		if (_chanceOfRejectingOffer >= 0.15f) {

			return false;
		}

		return true;
	}

	public float CalculateChanceOfRejectingOffer () {

		float numFactors = 0;

		float contactStrength = _targetTribe.CalculateContactStrength (_sourceTribe) * ContactStrengthFactor;
		numFactors++;

		float isolationPreferenceValue = _targetTribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);
		numFactors++;

		float relationshipValue = _targetTribe.GetRelationshipValue (_sourceTribe);
		numFactors++;

		// average factors
		float chance = 1 - ((1 - isolationPreferenceValue) + (1 - relationshipValue) + contactStrength) / numFactors;

		return Mathf.Clamp01 (chance);
	}

	public float CalculateChanceOfMakingAttempt () {

		float numFactors = 0;

		float contactStrength = _sourceTribe.CalculateContactStrength (_targetTribe) * ContactStrengthFactor;
		numFactors++;

		float isolationPreferenceValue = _sourceTribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);
		numFactors++;

		float relationshipValue = _sourceTribe.GetRelationshipValue (_targetTribe);
		numFactors++;

		// average factors
		float chance = ((1- isolationPreferenceValue) + (1 - relationshipValue) + contactStrength) / numFactors;

		return Mathf.Clamp01 (chance);
	}

	public override void Trigger () {

//		#if DEBUG
//		if (_targetTribe.Id == 6993753500213400) {
//			bool debug = true;
//		}
//		#endif

		bool attemptFoster = _sourceTribe.GetNextLocalRandomFloat (RngOffsets.FOSTER_TRIBE_RELATION_EVENT_MAKE_ATTEMPT) < _chanceOfMakingAttempt;

		if (_sourceTribe.IsUnderPlayerFocus || _originalSourceDominantClan.IsUnderPlayerGuidance) {

			Decision fosterDecision = new FosterTribeRelationDecision (_sourceTribe, _targetTribe, attemptFoster, _chanceOfRejectingOffer);

			if (_originalSourceDominantClan.IsUnderPlayerGuidance) {

				World.AddDecisionToResolve (fosterDecision);

			} else {

				fosterDecision.ExecutePreferredOption ();
			}

		} else if (attemptFoster) {

			FosterTribeRelationDecision.LeaderAttemptsFosterRelationship (_sourceTribe, _targetTribe, _chanceOfRejectingOffer);

		} else {

			FosterTribeRelationDecision.LeaderAvoidsFosteringRelationship (_sourceTribe, _targetTribe);
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

//			#if DEBUG
//			if (tribe.Id == 6993753500213400) {
//				bool debug = true;
//			}
//			#endif

			tribe.ResetEvent (WorldEvent.FosterTribeRelationDecisionEventId, CalculateTriggerDate (tribe));
		}
	}

	public override void Reset (long newTriggerDate)
	{
		base.Reset (newTriggerDate);

		_sourceTribe = Polity as Tribe;
		_originalSourceDominantClan = Polity.DominantFaction as Clan;
	}
}
