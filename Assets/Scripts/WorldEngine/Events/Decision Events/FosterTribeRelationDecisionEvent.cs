using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

[System.Obsolete]
public class FosterTribeRelationDecisionEvent : PolityEvent
{
    public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 5;

    public const float ContactStrengthFactor = 1f;

    private PolityContact _targetContact;

    private Tribe _targetTribe;
    private Tribe _sourceTribe;

    private Clan _originalSourceDominantClan;
    private Clan _targetDominantClan;

    private float _chanceOfMakingAttempt;
    private float _chanceOfRejectingOffer;

    public FosterTribeRelationDecisionEvent()
    {
        DoNotSerialize = true;
    }

    public FosterTribeRelationDecisionEvent(Tribe sourceTribe, PolityEventData data) : base(sourceTribe, data)
    {
        _sourceTribe = sourceTribe;
        _originalSourceDominantClan = World.GetFaction(data.OriginalDominantFactionId) as Clan;

        DoNotSerialize = true;
    }

    public FosterTribeRelationDecisionEvent(Tribe sourceTribe, long triggerDate) : base(sourceTribe, triggerDate, FosterTribeRelationDecisionEventId)
    {
        _sourceTribe = sourceTribe;
        _originalSourceDominantClan = sourceTribe.DominantFaction as Clan;

        DoNotSerialize = true;
    }

    public static long CalculateTriggerDate(Tribe tribe)
    {
        float randomFactor = tribe.GetNextLocalRandomFloat(RngOffsets.FOSTER_TRIBE_RELATION_EVENT_CALCULATE_TRIGGER_DATE);
        randomFactor = Mathf.Pow(randomFactor, 2);

        float isolationPreferenceValue = tribe.GetPreferenceValue(CulturalPreference.IsolationPreferenceId);

        float isolationPrefFactor = 2 * isolationPreferenceValue;
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
                "FosterTribeRelationDecisionEvent.CalculateTriggerDate - triggerDate (" + triggerDate +
                ") greater than MaxSupportedDate (" + World.MaxSupportedDate +
                "). triggerDateSpan: " + triggerDateSpan + ", randomFactor: " + randomFactor +
                ", isolationPrefFactor: " + isolationPrefFactor);

            triggerDate = int.MinValue;
        }

        return triggerDate;
    }

    public float GetContactWeight(PolityContact contact)
    {
        if (contact.Polity is Tribe)
            return _sourceTribe.CalculateContactStrength(contact);

        return 0;
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
            return false;

        if (_sourceTribe.DominantFaction != OriginalDominantFaction)
            return false;

        int rngOffset = unchecked((int)(RngOffsets.EVENT_CAN_TRIGGER + Id));

        _targetContact = _sourceTribe.GetRandomPolityContact(rngOffset++, GetContactWeight);

        if (_targetContact == null)
            return false;

        _targetTribe = _targetContact.Polity as Tribe;
        _targetDominantClan = _targetTribe.DominantFaction as Clan;

        // We should use the latest cultural attribute values before calculating chances
        _originalSourceDominantClan.PreUpdate();
        _targetDominantClan.PreUpdate();

        _chanceOfMakingAttempt = CalculateChanceOfMakingAttempt();

        if (_chanceOfMakingAttempt <= 0.15f)
        {
            return false;
        }

        _chanceOfRejectingOffer = CalculateChanceOfRejectingOffer();

        if (_chanceOfRejectingOffer >= 0.15f)
        {
            return false;
        }

        return true;
    }

    public float CalculateChanceOfRejectingOffer()
    {
        float numFactors = 0;

        float contactStrength = _targetTribe.CalculateContactStrength(_sourceTribe) * ContactStrengthFactor;
        numFactors++;

        float isolationPreferenceValue = _targetTribe.GetPreferenceValue(CulturalPreference.IsolationPreferenceId);
        numFactors++;

        float relationshipValue = _targetTribe.GetRelationshipValue(_sourceTribe);
        numFactors++;

        // average factors
        float chance = 1 - ((1 - isolationPreferenceValue) + (1 - relationshipValue) + contactStrength) / numFactors;

        return Mathf.Clamp01(chance);
    }

    public float CalculateChanceOfMakingAttempt()
    {
        float numFactors = 0;

        float contactStrength = _sourceTribe.CalculateContactStrength(_targetTribe) * ContactStrengthFactor;
        numFactors++;

        float isolationPreferenceValue = _sourceTribe.GetPreferenceValue(CulturalPreference.IsolationPreferenceId);
        numFactors++;

        float relationshipValue = _sourceTribe.GetRelationshipValue(_targetTribe);
        numFactors++;

        // average factors
        float chance = ((1 - isolationPreferenceValue) + (1 - relationshipValue) + contactStrength) / numFactors;

        return Mathf.Clamp01(chance);
    }

    public override void Trigger()
    {
        bool attemptFoster = _sourceTribe.GetNextLocalRandomFloat(RngOffsets.FOSTER_TRIBE_RELATION_EVENT_MAKE_ATTEMPT) < _chanceOfMakingAttempt;

        if (_sourceTribe.IsUnderPlayerFocus || _originalSourceDominantClan.IsUnderPlayerGuidance)
        {
            Decision fosterDecision = new FosterTribeRelationDecision(_sourceTribe, _targetTribe, attemptFoster, _chanceOfRejectingOffer, Id);

            if (_originalSourceDominantClan.IsUnderPlayerGuidance)
            {
                World.AddDecisionToResolve(fosterDecision);
            }
            else
            {
                fosterDecision.ExecutePreferredOption();
            }
        }
        else if (attemptFoster)
        {
            FosterTribeRelationDecision.LeaderAttemptsFosterRelationship(_sourceTribe, _targetTribe, _chanceOfRejectingOffer, Id);
        }
        else
        {
            FosterTribeRelationDecision.LeaderAvoidsFosteringRelationship(_sourceTribe, _targetTribe);
        }
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _sourceTribe = Polity as Tribe;
        _originalSourceDominantClan = OriginalDominantFaction as Clan;

        _sourceTribe.AddEvent(this);
    }

    protected override void DestroyInternal()
    {
        base.DestroyInternal();

        if ((Polity != null) && Polity.StillPresent)
        {
            Tribe tribe = Polity as Tribe;

            long triggerDate = CalculateTriggerDate(tribe);
            if (triggerDate  < 0)
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

            tribe.ResetEvent(FosterTribeRelationDecisionEventId, triggerDate);
        }
    }

    public override void Reset(long newTriggerDate)
    {
        base.Reset(newTriggerDate);

        _sourceTribe = Polity as Tribe;
        _originalSourceDominantClan = Polity.DominantFaction as Clan;
    }
}
