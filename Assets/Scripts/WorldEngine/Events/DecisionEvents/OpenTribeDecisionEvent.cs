using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class OpenTribeDecisionEvent : PolityEvent
{
    public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 50;

    private Tribe _tribe;

    private Clan _originalDominantClan;

    private float _chanceOfMakingAttempt;

    public OpenTribeDecisionEvent()
    {
        DoNotSerialize = true;
    }

    public OpenTribeDecisionEvent(Tribe tribe, PolityEventData data) : base(tribe, data)
    {
        _tribe = tribe;
        _originalDominantClan = World.GetFaction(data.OriginalDominantFactionId) as Clan;

        DoNotSerialize = true;
    }

    public OpenTribeDecisionEvent(Tribe tribe, long triggerDate) : base(tribe, triggerDate, OpenTribeDecisionEventId)
    {
        _tribe = tribe;
        _originalDominantClan = tribe.DominantFaction as Clan;

        DoNotSerialize = true;
    }

    public static long CalculateTriggerDate(Tribe tribe)
    {
        float randomFactor = tribe.GetNextLocalRandomFloat(RngOffsets.OPEN_TRIBE_EVENT_CALCULATE_TRIGGER_DATE);
        randomFactor = Mathf.Pow(randomFactor, 2);

        float isolationPreferenceValue = tribe.GetPreferenceValue(CulturalPreference.IsolationPreferenceId);

        float isolationPrefFactor = 2 * (1 - isolationPreferenceValue);
        isolationPrefFactor = Mathf.Pow(isolationPrefFactor, 4);

        float dateSpan = (1 - randomFactor) * DateSpanFactorConstant * isolationPrefFactor;

        long triggerDateSpan = (long)dateSpan + CellGroup.GenerationSpan;

        if (triggerDateSpan < 0)
        {
            Debug.LogWarning("updateSpan less than 0: " + triggerDateSpan);

            triggerDateSpan = CellGroup.MaxUpdateSpan;
        }

        long triggerDate = tribe.World.CurrentDate + triggerDateSpan;

        if (triggerDate > World.MaxSupportedDate)
        {
            // nextDate is invalid, generate report
            Debug.LogWarning(
                "OpenTribeDecisionEvent.CalculateTriggerDate - triggerDate (" + triggerDate +
                ") greater than MaxSupportedDate (" + World.MaxSupportedDate +
                "). triggerDateSpan: " + triggerDateSpan + ", randomFactor: " + randomFactor +
                ", isolationPrefFactor: " + isolationPrefFactor);

            triggerDate = int.MinValue;
        }

        return triggerDate;
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
            return false;

        if (_tribe.DominantFaction != OriginalDominantFaction)
            return false;

        // We should use the latest cultural attribute values before calculating chances
        _originalDominantClan.PreUpdate();

        _chanceOfMakingAttempt = CalculateChanceOfMakingAttempt();

        if (_chanceOfMakingAttempt <= 0f)
        {
            return false;
        }

        return true;
    }

    public float CalculateChanceOfMakingAttempt()
    {
        float numFactors = 0;

        float isolationPreferenceValue = _tribe.GetPreferenceValue(CulturalPreference.IsolationPreferenceId);
        numFactors++;

        // average factors
        float chance = (1 - isolationPreferenceValue) / numFactors;

        return Mathf.Clamp01(chance);
    }

    public override void Trigger()
    {
        bool attemptToOpen = _tribe.GetNextLocalRandomFloat(RngOffsets.OPEN_TRIBE_EVENT_MAKE_ATTEMPT) < _chanceOfMakingAttempt;

        if (_tribe.IsUnderPlayerFocus || _originalDominantClan.IsUnderPlayerGuidance)
        {
            Decision openDecision = new OpenTribeDecision(_tribe, attemptToOpen, Id);

            if (_originalDominantClan.IsUnderPlayerGuidance)
            {
                World.AddDecisionToResolve(openDecision);
            }
            else
            {
                openDecision.ExecutePreferredOption();
            }
        }
        else if (attemptToOpen)
        {
            OpenTribeDecision.LeaderOpensTribe(_tribe);
        }
        else
        {
            OpenTribeDecision.LeaderAvoidsOpeningTribe(_tribe);
        }
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _tribe = Polity as Tribe;
        _originalDominantClan = OriginalDominantFaction as Clan;

        _tribe.AddEvent(this);
    }

    protected override void DestroyInternal()
    {
        base.DestroyInternal();

        if ((Polity != null) && Polity.StillPresent)
        {
            Tribe tribe = Polity as Tribe;

            long triggerDate = CalculateTriggerDate(tribe);
            if (triggerDate < 0)
            {
                // skip reseting the event since the trigger date is invalid
                return;
            }

            if (triggerDate <= tribe.World.CurrentDate)
            {
                throw new System.Exception(
                    "Trigger Date (" + triggerDate +
                    ") less or equal to current date: " + tribe.World.CurrentDate);
            }

            tribe.ResetEvent(OpenTribeDecisionEventId, triggerDate);
        }
    }

    public override void Reset(long newTriggerDate)
    {
        base.Reset(newTriggerDate);

        _tribe = Polity as Tribe;
        _originalDominantClan = Polity.DominantFaction as Clan;
    }
}
