﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class ClanDemandsInfluenceDecisionEvent : FactionEvent
{
    public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 5;

    public const int DemandClanMaxAdministrativeLoad = 5000000;
    public const int DemandClanMinAdministrativeLoad = 100000;
    public const int DemandClanAdministrativeLoadSpan = DemandClanMaxAdministrativeLoad - DemandClanMinAdministrativeLoad;

    public const int DominantClanMaxAdministrativeLoad = 4000000;
    public const int DominantClanMinAdministrativeLoad = 100000;
    public const int DominantClanAdministrativeLoadSpan = DominantClanMaxAdministrativeLoad - DominantClanMinAdministrativeLoad;

    public const float DecisionChanceFactor = 0.5f;

    private Clan _demandClan;
    private Clan _dominantClan;

    private Tribe _originalTribe;

    private float _chanceOfMakingDemand;
    private float _chanceOfRejectingDemand;

    public ClanDemandsInfluenceDecisionEvent()
    {
        DoNotSerialize = true;
    }

    public ClanDemandsInfluenceDecisionEvent(Clan demandClan, FactionEventData data) : base(demandClan, data)
    {
        _demandClan = demandClan;
        _originalTribe = World.GetPolity(data.OriginalPolityId) as Tribe;

        DoNotSerialize = true;
    }

    public ClanDemandsInfluenceDecisionEvent(Clan demandClan, long triggerDate) : base(demandClan, triggerDate, ClanDemandsInfluenceDecisionEventId)
    {
        _demandClan = demandClan;
        _originalTribe = demandClan.Polity as Tribe;

        DoNotSerialize = true;
    }

    public static long CalculateTriggerDate(Clan clan)
    {
        float randomFactor = clan.GetNextLocalRandomFloat(RngOffsets.CLAN_DEMANDS_INFLUENCE_EVENT_CALCULATE_TRIGGER_DATE + unchecked((int)clan.Id));
        randomFactor = Mathf.Pow(randomFactor, 2);

        float administrativeLoad = clan.CalculateAdministrativeLoad();

        float loadFactor = 1;

        if (!float.IsPositiveInfinity(administrativeLoad))
        {
            float modAdminLoad = Mathf.Max(0, administrativeLoad - DemandClanMinAdministrativeLoad);

            loadFactor = 1 - DemandClanAdministrativeLoadSpan / (modAdminLoad + DemandClanAdministrativeLoadSpan);
        }

        float authorityPreferenceValue = clan.GetPreferenceValue(CulturalPreference.AuthorityPreferenceId);

        float authorityPrefFactor = 2 * (1 - authorityPreferenceValue);
        authorityPrefFactor = Mathf.Pow(authorityPrefFactor, 4);

        float dateSpan = (1 - randomFactor) * DateSpanFactorConstant * loadFactor * authorityPrefFactor;

        long triggerDateSpan = (long)dateSpan + CellGroup.GenerationSpan;

        if (triggerDateSpan < 0)
        {
            Debug.LogWarning("updateSpan less than 0: " + triggerDateSpan);

            triggerDateSpan = CellGroup.MaxUpdateSpan;
        }

        long triggerDate = clan.World.CurrentDate + triggerDateSpan;

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                "ClanDemandsInfluenceDecisionEvent:CalculateTriggerDate - clan.Id:" + clan.Id,
        //                "triggerDate: " + triggerDate +
        //                ", triggerDateSpan: " + triggerDateSpan +
        //                ", dateSpan: " + dateSpan +
        //                ", randomFactor: " + randomFactor +
        //                ", loadFactor: " + loadFactor +
        //                ", authorityPrefFactor: " + authorityPrefFactor +
        //                ", clan.LastUpdateDate: " + clan.LastUpdateDate +
        //                ", clan.World.CurrentDate: " + clan.World.CurrentDate +
        //                "");

        //            Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //        }
        //#endif

        if (triggerDate > World.MaxSupportedDate)
        {
            // nextDate is invalid, generate report
            Debug.LogWarning(
                "CalculateTriggerDate - triggerDate (" + triggerDate +
                ") greater than MaxSupportedDate (" + World.MaxSupportedDate +
                "). dateSpan: " + dateSpan + ", randomFactor: " + randomFactor);

            triggerDate = int.MinValue;
        }

        return triggerDate;
    }

    public override bool CanTrigger()
    {
        //#if DEBUG
        //        if ((Id == 160349336613603015L) || (Id == 160349354613603010L))
        //        {
        //            string logMsg = "Event Id: " + Id + " CanTrigger. base.CanTrigger(): " + base.CanTrigger() +
        //                ", _demandClan.Polity == OriginalPolity: " + (_demandClan.Polity == OriginalPolity) +
        //                ", !_demandClan.IsDominant: " + (!_demandClan.IsDominant);

        //            if (base.CanTrigger() && (_demandClan.Polity == OriginalPolity) && !_demandClan.IsDominant)
        //            {
        //                _dominantClan = _originalTribe.DominantFaction as Clan;

        //                // We should use the latest cultural attribute values before calculating chances
        //                _demandClan.PreUpdate();
        //                _dominantClan.PreUpdate();

        //                _chanceOfRejectingDemand = CalculateChanceOfRefusingDemand();
        //                _chanceOfMakingDemand = CalculateChanceOfMakingDemand();

        //                logMsg += ", _chanceOfRejectingDemand: " + _chanceOfRejectingDemand + ", _chanceOfMakingDemand: " + _chanceOfMakingDemand;
        //            }

        //            logMsg += ", current date: " + World.CurrentDate + ", loaded world: " + Manager.Debug_IsLoadedWorld;

        //            Debug.LogWarning(logMsg);
        //        }
        //#endif

        if (!base.CanTrigger())
            return false;

        if (_demandClan.Polity != OriginalPolity)
            return false;

        if (_demandClan.IsDominant)
            return false;

        _dominantClan = _originalTribe.DominantFaction as Clan;

        ///// We should use the latest cultural attribute values before calculating chances

        _demandClan.PreUpdate();
        _dominantClan.PreUpdate();

        /////

        _chanceOfRejectingDemand = CalculateChanceOfRefusingDemand();
        _chanceOfMakingDemand = CalculateChanceOfMakingDemand();

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                "ClanDemandsInfluenceDecisionEvent:CanTrigger - DemandClanId:" + _demandClan.Id + ", DominantClan: " + _dominantClan.Id,
        //                "TriggerDate: " + TriggerDate +
        //                ", _chanceOfRejectingDemand: " + _chanceOfRejectingDemand +
        //                ", _chanceOfMakingDemand: " + _chanceOfMakingDemand +
        //                "");

        //            Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //        }
        //#endif

        if (_chanceOfRejectingDemand >= 1)
        {
            return false;
        }

        if (_chanceOfMakingDemand <= 0)
        {
            return false;
        }

        return true;
    }

    public float CalculateChanceOfRefusingDemand()
    {
        float administrativeLoad = _dominantClan.CalculateAdministrativeLoad();

        if (float.IsPositiveInfinity(administrativeLoad))
            return 0;

        float authorityPreferenceValue = _dominantClan.GetPreferenceValue(CulturalPreference.AuthorityPreferenceId);

        if (authorityPreferenceValue <= 0)
            return 0;

        float authorityPrefFactor = 2 * authorityPreferenceValue;
        authorityPrefFactor = Mathf.Pow(authorityPrefFactor, 4);

        float relationshipValue = _dominantClan.GetRelationshipValue(_demandClan);

        if (relationshipValue <= 0)
            return 1;

        float relationshipFactor = 2 * (1 - relationshipValue);
        relationshipFactor = Mathf.Pow(relationshipFactor, 4);

        float influenceDeltaValue = _dominantClan.Influence - _demandClan.Influence;

        if (influenceDeltaValue <= 0)
            return 0;

        if (influenceDeltaValue >= 1)
            return 1;

        float influenceFactor = 2 * influenceDeltaValue;
        influenceFactor = Mathf.Pow(influenceFactor, 4);

        float factors = authorityPrefFactor * relationshipFactor * influenceFactor * DecisionChanceFactor;

        float modMinAdministrativeLoad = DominantClanMinAdministrativeLoad * factors;
        float modMaxAdministrativeLoad = DominantClanMaxAdministrativeLoad * factors;

        float chance = 1 - (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if ((Manager.TracingData.FactionId == _dominantClan.Id) ||
        //                (Manager.TracingData.FactionId == _demandClan.Id))
        //            {
        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ClanDemandsInfluenceDecisionEvent:CalculateChanceOfRefusingDemand - DemandClanId:" + _demandClan.Id + ", DominantClan: " + _dominantClan.Id,
        //                    "TriggerDate: " + TriggerDate +
        //                    ", administrativeLoad: " + administrativeLoad +
        //                    ", authorityPreferenceValue: " + authorityPreferenceValue +
        //                    ", relationshipValue: " + relationshipValue +
        //                    ", influenceDeltaValue: " + influenceDeltaValue +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        return Mathf.Clamp01(chance);
    }

    public float CalculateChanceOfMakingDemand()
    {
        float administrativeLoad = _demandClan.CalculateAdministrativeLoad();

        if (float.IsPositiveInfinity(administrativeLoad))
            return 0;

        float authorityPreferenceValue = _demandClan.GetPreferenceValue(CulturalPreference.AuthorityPreferenceId);

        if (authorityPreferenceValue <= 0)
            return 0;

        float authorityPrefFactor = 2 * authorityPreferenceValue;
        authorityPrefFactor = Mathf.Pow(authorityPrefFactor, 4);

        float relationshipValue = _demandClan.GetRelationshipValue(_dominantClan);

        if (relationshipValue >= 1)
            return 0;

        float relationshipFactor = 2 * (1 - relationshipValue);
        relationshipFactor = Mathf.Pow(relationshipFactor, 4);

        float influenceDeltaValue = _dominantClan.Influence - _demandClan.Influence;

        if (influenceDeltaValue <= 0)
            return 0;

        if (influenceDeltaValue >= 1)
            return 1;

        float influenceFactor = 2 * influenceDeltaValue;
        influenceFactor = Mathf.Pow(influenceFactor, 4);

        float factors = authorityPrefFactor * relationshipFactor * influenceFactor * DecisionChanceFactor;

        float modMinAdministrativeLoad = DemandClanMinAdministrativeLoad * factors;
        float modMaxAdministrativeLoad = DemandClanMaxAdministrativeLoad * factors;

        float chance = 1 - (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            if ((Manager.TracingData.FactionId == _dominantClan.Id) ||
        //                (Manager.TracingData.FactionId == _demandClan.Id))
        //            {
        //                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                    "ClanDemandsInfluenceDecisionEvent:CalculateChanceOfMakingDemand - DemandClanId:" + _demandClan.Id + ", DominantClan: " + _dominantClan.Id,
        //                    "TriggerDate: " + TriggerDate +
        //                    ", administrativeLoad: " + administrativeLoad +
        //                    ", authorityPreferenceValue: " + authorityPreferenceValue +
        //                    ", relationshipValue: " + relationshipValue +
        //                    ", influenceDeltaValue: " + influenceDeltaValue +
        //                    "");

        //                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            }
        //        }
        //#endif

        return Mathf.Clamp01(chance);
    }

    public override void Trigger()
    {
//#if DEBUG
//        if ((Id == 160349336613603015L) || (Id == 160349354613603010L))
//        {
//            Debug.LogWarning("Event Id: " + Id + " has been triggered, current date: " + World.CurrentDate + ", loaded world: " + Manager.Debug_IsLoadedWorld);
//        }
//#endif

        bool performDemand = _demandClan.GetNextLocalRandomFloat(RngOffsets.CLAN_DEMANDS_INFLUENCE_EVENT_PERFORM_DEMAND) < _chanceOfMakingDemand;

        Tribe tribe = _demandClan.Polity as Tribe;

        if (tribe.IsUnderPlayerFocus || _demandClan.IsUnderPlayerGuidance)
        {
            Decision demandDecision;

            demandDecision = new ClanDemandsInfluenceDecision(tribe, _demandClan, _dominantClan, performDemand, _chanceOfRejectingDemand, Id);

            if (_demandClan.IsUnderPlayerGuidance)
            {
                World.AddDecisionToResolve(demandDecision);
            }
            else
            {
                demandDecision.ExecutePreferredOption();
            }

        }
        else if (performDemand)
        {
            ClanDemandsInfluenceDecision.LeaderDemandsInfluence(_demandClan, _dominantClan, tribe, _chanceOfRejectingDemand, Id);
        }
        else
        {
            ClanDemandsInfluenceDecision.LeaderAvoidsDemandingInfluence(_demandClan, _dominantClan, tribe);
        }
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _demandClan = Faction as Clan;
        _originalTribe = OriginalPolity as Tribe;

        _demandClan.AddEvent(this);
    }

    protected override void DestroyInternal()
    {
        base.DestroyInternal();

        if ((Faction != null) && Faction.StillPresent)
        {
            Clan clan = Faction as Clan;

            long triggerDate = CalculateTriggerDate(clan);
            if (triggerDate < 0)
            {
                // skip reseting the event since the trigger date is invalid
                return;
            }

            if (triggerDate <= clan.World.CurrentDate)
            {
                throw new System.Exception(
                    "Trigger Date (" + triggerDate +
                    ") less or equal to current date: " + clan.World.CurrentDate);
            }

            clan.ResetEvent(ClanDemandsInfluenceDecisionEventId, triggerDate);
        }
    }

    public override void Reset(long newTriggerDate)
    {
        base.Reset(newTriggerDate);

        _demandClan = Faction as Clan;
        _originalTribe = OriginalPolity as Tribe;
    }
}
