using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class OpenTribeDecisionEvent : PolityEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 50;

	private Tribe _tribe;

	private Clan _originalDominantClan;

	private float _chanceOfMakingAttempt;

	public OpenTribeDecisionEvent () {

		DoNotSerialize = true;
	}

	public OpenTribeDecisionEvent (Tribe tribe, PolityEventData data) : base (tribe, data) {

		_tribe = tribe;
		_originalDominantClan = World.GetFaction (data.OriginalDominantFactionId) as Clan;

		DoNotSerialize = true;
	}

	public OpenTribeDecisionEvent (Tribe tribe, long triggerDate) : base (tribe, triggerDate, OpenTribeDecisionEventId) {

		_tribe = tribe;
		_originalDominantClan = tribe.DominantFaction as Clan;

		DoNotSerialize = true;
	}

	public static long CalculateTriggerDate (Tribe tribe) {

		float randomFactor = tribe.GetNextLocalRandomFloat (RngOffsets.OPEN_TRIBE_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		float isolationPreferenceValue = tribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);

		float isoloationPrefFactor = 2 * (1 - isolationPreferenceValue);
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

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		if (_tribe.DominantFaction != OriginalDominantFaction)
			return false;

//		int rngOffset = (int)(RngOffsets.EVENT_CAN_TRIGGER + Id);

		// We should use the latest cultural attribute values before calculating chances
		_originalDominantClan.PreUpdate ();

		_chanceOfMakingAttempt = CalculateChanceOfMakingAttempt ();

		if (_chanceOfMakingAttempt <= 0f) {

			return false;
		}

		return true;
	}

	public float CalculateChanceOfMakingAttempt () {

		float numFactors = 0;

		float isolationPreferenceValue = _tribe.GetPreferenceValue (CulturalPreference.IsolationPreferenceId);
		numFactors++;

		// average factors
		float chance = (1 - isolationPreferenceValue) / numFactors;

		return Mathf.Clamp01 (chance);
	}

	public override void Trigger () {

		bool attemptToOpen = _tribe.GetNextLocalRandomFloat (RngOffsets.OPEN_TRIBE_EVENT_MAKE_ATTEMPT) < _chanceOfMakingAttempt;

		if (_tribe.IsUnderPlayerFocus || _originalDominantClan.IsUnderPlayerGuidance) {

			Decision openDecision = new OpenTribeDecision (_tribe, attemptToOpen, Id);

			if (_originalDominantClan.IsUnderPlayerGuidance) {

				World.AddDecisionToResolve (openDecision);

			} else {

				openDecision.ExecutePreferredOption ();
			}

		} else if (attemptToOpen) {

			OpenTribeDecision.LeaderOpensTribe (_tribe);

		} else {

			OpenTribeDecision.LeaderAvoidsOpeningTribe (_tribe);
		}
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		_tribe = Polity as Tribe;
		_originalDominantClan = OriginalDominantFaction as Clan;

		_tribe.AddEvent (this);
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Polity != null) && (Polity.StillPresent)) {

			Tribe tribe = Polity as Tribe;

			tribe.ResetEvent (WorldEvent.OpenTribeDecisionEventId, CalculateTriggerDate (tribe));
		}
	}

	public override void Reset (long newTriggerDate)
	{
		base.Reset (newTriggerDate);

		_tribe = Polity as Tribe;
		_originalDominantClan = Polity.DominantFaction as Clan;
	}
}
