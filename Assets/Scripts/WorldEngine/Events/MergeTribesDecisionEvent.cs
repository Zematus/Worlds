using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class MergeTribesDecisionEvent : PolityEvent {

	public const int MaxAdministrativeLoad = 600000;
	public const int MinAdministrativeLoad = 60000;
	public const int DeltaAdministrativeLoad = MaxAdministrativeLoad - MinAdministrativeLoad;

	public const float MaxAdministrativeLoadChanceFactor = 0.05f;

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 25;

	public const float ContactStrengthFactor = 2f;

	private PolityContact _targetContact;

	private Tribe _targetTribe;
	private Tribe _sourceTribe;

	private Clan _originalSourceDominantClan;
	private Clan _targetDominantClan;

	private float _chanceOfMakingAttempt;
	private float _chanceOfRejectingOffer;

	public MergeTribesDecisionEvent () {

		DoNotSerialize = true;
	}

	public MergeTribesDecisionEvent (Tribe sourceTribe, PolityEventData data) : base (sourceTribe, data) {

		_sourceTribe = sourceTribe;
		_originalSourceDominantClan = World.GetFaction (data.OriginalDominantFactionId) as Clan;

		DoNotSerialize = true;
	}

	public MergeTribesDecisionEvent (Tribe sourceTribe, long triggerDate) : base (sourceTribe, triggerDate, MergeTribesDecisionEventId) {

		_sourceTribe = sourceTribe;
		_originalSourceDominantClan = sourceTribe.DominantFaction as Clan;

		DoNotSerialize = true;
	}

	public static long CalculateTriggerDate (Tribe tribe) {

		float randomFactor = tribe.GetNextLocalRandomFloat (RngOffsets.MERGE_TRIBES_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float isolationPreferenceValue = tribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);

		float isolationPrefFactor = 2 * isolationPreferenceValue;
		isolationPrefFactor = Mathf.Pow (isolationPrefFactor, 4);

		float cohesionPreferenceValue = tribe.GetPreferenceValue (CulturalPreference.CohesionPreferenceId);

		float cohesionPrefFactor = 2 * (1 - cohesionPreferenceValue);
		cohesionPrefFactor = Mathf.Pow (cohesionPrefFactor, 4);

		float dateSpan = (1 - randomFactor) * DateSpanFactorConstant * isolationPrefFactor * cohesionPrefFactor;

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

		_chanceOfRejectingOffer = CalculateChanceOfRejectingOffer ();

		if (_chanceOfMakingAttempt <= 0.0f) {

			return false;
		}

		if (_chanceOfRejectingOffer >= 1.0f) {

			return false;
		}

		return true;
	}

	public float CalculateChanceOfRejectingOffer () {

		float administrativeLoad = _targetTribe.CalculateAdministrativeLoad ();

		if (administrativeLoad == Mathf.Infinity)
			return 1;

		float numFactors = 0;

		float contactStrength = _targetTribe.CalculateContactStrength (_sourceTribe) * ContactStrengthFactor;
		numFactors++;

		float isolationPreferenceValue = _targetTribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);
		numFactors++;

		float cohesionPreferenceValue = _targetTribe.GetPreferenceValue (CulturalPreference.CohesionPreferenceId);
		numFactors++;

		float relationshipValue = _targetTribe.GetRelationshipValue (_sourceTribe);
		numFactors++;

		float modIsolationPreferencValue = isolationPreferenceValue * 2;
		float modCohesionPreferenceValue = (cohesionPreferenceValue - 0.5f) * 2;
		float modRelationshipValue = (relationshipValue - 0.5f) * 2;

		/// NOTE: Move administrative load stuff to a separate general function

		float authorityPreferenceValue = _targetTribe.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

		float cohesionPrefFactor = 2 * cohesionPreferenceValue;
		cohesionPrefFactor = Mathf.Pow (cohesionPrefFactor, 4);

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

		float modMinAdministrativeLoad = MinAdministrativeLoad * cohesionPrefFactor;
		float modMaxAdministrativeLoad = modMinAdministrativeLoad + 
			(DeltaAdministrativeLoad * _targetTribe.CurrentLeader.Wisdom * _targetTribe.CurrentLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

		float administrativeLoadFactor = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);
		numFactors++;

		/// End of NOTE relevant code

		float chance = 1 - ((1 - modIsolationPreferencValue) + modCohesionPreferenceValue + modRelationshipValue + contactStrength + (1 - administrativeLoadFactor)) / numFactors;

		return Mathf.Clamp01 (chance);
	}

	public float CalculateChanceOfMakingAttempt () {

		float administrativeLoad = _sourceTribe.CalculateAdministrativeLoad ();

		if (administrativeLoad == Mathf.Infinity)
			return 0;

		float numFactors = 0;

		float contactStrength = _sourceTribe.CalculateContactStrength (_targetTribe) * ContactStrengthFactor;
		numFactors++;

		float isolationPreferenceValue = _sourceTribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);
		numFactors++;

		float cohesionPreferenceValue = _sourceTribe.GetPreferenceValue (CulturalPreference.CohesionPreferenceId);
		numFactors++;

		float relationshipValue = _sourceTribe.GetRelationshipValue (_targetTribe);
		numFactors++;

		float modIsolationPreferencValue = isolationPreferenceValue * 2;
		float modCohesionPreferenceValue = (cohesionPreferenceValue - 0.5f) * 2;
		float modRelationshipValue = (relationshipValue - 0.5f) * 2;

		/// NOTE: Move administrative load stuff to a separate general function

		float authorityPreferenceValue = _targetTribe.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

		float cohesionPrefFactor = 2 * cohesionPreferenceValue;
		cohesionPrefFactor = Mathf.Pow (cohesionPrefFactor, 4);

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

		float modMinAdministrativeLoad = MinAdministrativeLoad * cohesionPrefFactor;
		float modMaxAdministrativeLoad = modMinAdministrativeLoad + 
			(DeltaAdministrativeLoad * _sourceTribe.CurrentLeader.Wisdom * _sourceTribe.CurrentLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

		float administrativeLoadFactor = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);
		numFactors++;

		/// End of NOTE relevant code
		
		float chance = ((1 - modIsolationPreferencValue) + modCohesionPreferenceValue + modRelationshipValue + contactStrength + (1 - administrativeLoadFactor)) / numFactors;

		return Mathf.Clamp01 (chance);
	}

	public override void Trigger () {

		bool attemptFoster = _targetTribe.GetNextLocalRandomFloat (RngOffsets.MERGE_TRIBES_EVENT_MAKE_ATTEMPT) < _chanceOfMakingAttempt;

		if (_sourceTribe.IsUnderPlayerFocus || _originalSourceDominantClan.IsUnderPlayerGuidance) {

			Decision mergeDecision = new MergeTribesDecision (_sourceTribe, _targetTribe, attemptFoster, _chanceOfRejectingOffer);

			if (_originalSourceDominantClan.IsUnderPlayerGuidance) {

				World.AddDecisionToResolve (mergeDecision);

			} else {

				mergeDecision.ExecutePreferredOption ();
			}

		} else if (attemptFoster) {

			MergeTribesDecision.LeaderAttemptsMergeTribes (_sourceTribe, _targetTribe, _chanceOfRejectingOffer);

		} else {

			MergeTribesDecision.LeaderAvoidsMergeTribesAttempt (_sourceTribe, _targetTribe);
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

			tribe.ResetEvent (WorldEvent.MergeTribesDecisionEventId, CalculateTriggerDate (tribe));
		}
	}

	public override void Reset (long newTriggerDate)
	{
		base.Reset (newTriggerDate);

		_sourceTribe = Polity as Tribe;
		_originalSourceDominantClan = Polity.DominantFaction as Clan;
	}
}
