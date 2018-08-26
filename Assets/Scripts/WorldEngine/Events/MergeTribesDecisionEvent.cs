using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

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

    public MergeTribesDecisionEvent()
    {
        DoNotSerialize = true;
    }

    public MergeTribesDecisionEvent(Tribe sourceTribe, PolityEventData data) : base(sourceTribe, data)
    {
        _sourceTribe = sourceTribe;
        _originalSourceDominantClan = World.GetFaction(data.OriginalDominantFactionId) as Clan;

        DoNotSerialize = true;
    }

    public MergeTribesDecisionEvent(Tribe sourceTribe, long triggerDate) : base(sourceTribe, triggerDate, MergeTribesDecisionEventId)
    {
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

	public override bool CanTrigger ()
    {

//#if DEBUG
//        if (Manager.RegisterDebugEvent != null)
//        {
//            if (Polity.Id == 11267065402613603L)
//            {
//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "MergeTribesDecisionEvent:CanTrigger - Polity.Id:" + Polity.Id,
//                    "TriggerDate: " + TriggerDate +
//                    ", SpawnDate: " + SpawnDate +
//                    ", base.CanTrigger(): " + base.CanTrigger() +
//                    ", _sourceTribe.DominantFaction == OriginalDominantFaction: " + (_sourceTribe.DominantFaction == OriginalDominantFaction) +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif

        if (!base.CanTrigger ())
			return false;

		if (_sourceTribe.DominantFaction != OriginalDominantFaction)
			return false;

		int rngOffset = (int)(RngOffsets.EVENT_CAN_TRIGGER + Id);

//		Profiler.BeginSample ("MergeTribesDecisionEvent - _sourceTribe.GetRandomPolityContact");

		_targetContact = _sourceTribe.GetRandomPolityContact (rngOffset++, GetContactWeight, true);

        //		Profiler.EndSample ();

        if (_targetContact == null)
			return false;

		_targetTribe = _targetContact.Polity as Tribe;
		_targetDominantClan = _targetTribe.DominantFaction as Clan;

#if DEBUG
        if (Manager.RegisterDebugEvent != null)
        {
            //if (_targetTribe.Id == Manager.TracingData.PolityId)
            //{
                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
                "MergeTribesDecisionEvent:CanTrigger - Polity.Id:" + Polity.Id,
                "TriggerDate: " + TriggerDate +
                ", _targetTribe.Id: " + _targetTribe.Id +
                ", _targetDominantClan.Id: " + _targetDominantClan.Id +
                "");

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            //}
        }
#endif

        //		Profiler.BeginSample ("MergeTribesDecisionEvent - clan preUpdates");

        // We should use the latest cultural attribute values before calculating chances
        _originalSourceDominantClan.PreUpdate ();
		_targetDominantClan.PreUpdate ();

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - CalculateChanceOfMakingAttempt");

		_chanceOfMakingAttempt = CalculateChanceOfMakingAttempt ();

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - CalculateChanceOfRejectingOffer");

		_chanceOfRejectingOffer = CalculateChanceOfRejectingOffer ();

//		Profiler.EndSample ();

		if (_chanceOfMakingAttempt <= 0.0f) {

			return false;
		}

		if (_chanceOfRejectingOffer >= 1.0f) {

			return false;
		}

		return true;
	}

	public float CalculateChanceOfRejectingOffer () {

//		Profiler.BeginSample ("MergeTribesDecisionEvent - CalculateAdministrativeLoad");

		float administrativeLoad = _targetTribe.CalculateAdministrativeLoad ();

//		Profiler.EndSample ();

		if (administrativeLoad == Mathf.Infinity)
			return 1;

		float numFactors = 0;

//		Profiler.BeginSample ("MergeTribesDecisionEvent - CalculateContactStrength");

		float contactStrength = _targetTribe.CalculateContactStrength (_sourceTribe) * ContactStrengthFactor;
		numFactors++;

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - GetPreferenceValue");

		float isolationPreferenceValue = _targetTribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);
		numFactors++;

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - GetPreferenceValue");

		float cohesionPreferenceValue = _targetTribe.GetPreferenceValue (CulturalPreference.CohesionPreferenceId);
		numFactors++;

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - GetRelationshipValue");

		float relationshipValue = _targetTribe.GetRelationshipValue (_sourceTribe);
		numFactors++;

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - value modifications");

		float modIsolationPreferencValue = isolationPreferenceValue * 2;
		float modCohesionPreferenceValue = (cohesionPreferenceValue - 0.5f) * 2;
		float modRelationshipValue = (relationshipValue - 0.5f) * 2;

//		Profiler.EndSample ();

		/// NOTE: Move administrative load stuff to a separate general function

//		Profiler.BeginSample ("MergeTribesDecisionEvent - GetPreferenceValue");

		float authorityPreferenceValue = _targetTribe.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - factor calculation");

		float cohesionPrefFactor = 2 * cohesionPreferenceValue;
		cohesionPrefFactor = Mathf.Pow (cohesionPrefFactor, 4);

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - factor calculation");

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - admin factor calculation");

		Profiler.BeginSample ("MergeTribesDecisionEvent - get currentLeader");

		Agent targetTribeLeader = _targetTribe.CurrentLeader;

		Profiler.EndSample ();

		float modMinAdministrativeLoad = MinAdministrativeLoad * cohesionPrefFactor;
		float modMaxAdministrativeLoad = modMinAdministrativeLoad + 
			(DeltaAdministrativeLoad * targetTribeLeader.Wisdom * targetTribeLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

		float administrativeLoadFactor = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);
		numFactors++;

//		Profiler.EndSample ();

		/// End of NOTE relevant code

//		Profiler.BeginSample ("MergeTribesDecisionEvent - chance calculation");

		float chance = 1 - ((1 - modIsolationPreferencValue) + modCohesionPreferenceValue + modRelationshipValue + contactStrength + (1 - administrativeLoadFactor)) / numFactors;

//		Profiler.EndSample ();

		return Mathf.Clamp01 (chance);
	}

	public float CalculateChanceOfMakingAttempt () {

//		Profiler.BeginSample ("MergeTribesDecisionEvent - CalculateAdministrativeLoad");

		float administrativeLoad = _sourceTribe.CalculateAdministrativeLoad ();

//		Profiler.EndSample ();

		if (administrativeLoad == Mathf.Infinity)
			return 0;

		float numFactors = 0;

//		Profiler.BeginSample ("MergeTribesDecisionEvent - CalculateContactStrength");

		float contactStrength = _sourceTribe.CalculateContactStrength (_targetTribe) * ContactStrengthFactor;
		numFactors++;

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - GetPreferenceValue");

		float isolationPreferenceValue = _sourceTribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);
		numFactors++;

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - GetPreferenceValue");

		float cohesionPreferenceValue = _sourceTribe.GetPreferenceValue (CulturalPreference.CohesionPreferenceId);
		numFactors++;

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - GetRelationshipValue");

		float relationshipValue = _sourceTribe.GetRelationshipValue (_targetTribe);
		numFactors++;

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - value modifications");

		float modIsolationPreferencValue = isolationPreferenceValue * 2;
		float modCohesionPreferenceValue = (cohesionPreferenceValue - 0.5f) * 2;
		float modRelationshipValue = (relationshipValue - 0.5f) * 2;

//		Profiler.EndSample ();

		/// NOTE: Move administrative load stuff to a separate general function

//		Profiler.BeginSample ("MergeTribesDecisionEvent - GetPreferenceValue");

		float authorityPreferenceValue = _targetTribe.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - factor calculation");

		float cohesionPrefFactor = 2 * cohesionPreferenceValue;
		cohesionPrefFactor = Mathf.Pow (cohesionPrefFactor, 4);

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - factor calculation");

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

//		Profiler.EndSample ();

//		Profiler.BeginSample ("MergeTribesDecisionEvent - admin factor calculation");

		Profiler.BeginSample ("MergeTribesDecisionEvent - get currentLeader");

		Agent sourceTribeLeader = _sourceTribe.CurrentLeader;

		Profiler.EndSample ();

		float modMinAdministrativeLoad = MinAdministrativeLoad * cohesionPrefFactor;
		float modMaxAdministrativeLoad = modMinAdministrativeLoad + 
			(DeltaAdministrativeLoad * sourceTribeLeader.Wisdom * sourceTribeLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

		float administrativeLoadFactor = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);
		numFactors++;

//		Profiler.EndSample ();

		/// End of NOTE relevant code

//		Profiler.BeginSample ("MergeTribesDecisionEvent - chance calculation");
		
		float chance = ((1 - modIsolationPreferencValue) + modCohesionPreferenceValue + modRelationshipValue + contactStrength + (1 - administrativeLoadFactor)) / numFactors;

//		Profiler.EndSample ();

		return Mathf.Clamp01 (chance);
	}

	public override void Trigger () {

		bool attemptFoster = _sourceTribe.GetNextLocalRandomFloat (RngOffsets.MERGE_TRIBES_EVENT_MAKE_ATTEMPT) < _chanceOfMakingAttempt;

		if (_sourceTribe.IsUnderPlayerFocus || _originalSourceDominantClan.IsUnderPlayerGuidance) {

			Decision mergeDecision = new MergeTribesDecision (_sourceTribe, _targetTribe, attemptFoster, _chanceOfRejectingOffer, Id);

			if (_originalSourceDominantClan.IsUnderPlayerGuidance) {

				World.AddDecisionToResolve (mergeDecision);

			} else {

				mergeDecision.ExecutePreferredOption ();
			}

		} else if (attemptFoster) {

			MergeTribesDecision.LeaderAttemptsMergeTribes (_sourceTribe, _targetTribe, _chanceOfRejectingOffer, Id);

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
