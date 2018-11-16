using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class ClanSplitDecisionEvent : FactionEvent
{
    public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 10;

    public const float MinInfluenceTrigger = 0.3f;
    public const float MinCoreDistance = 1000f;
    public const float MinProminenceValue = 0.2f;
    public const int MinPopulation = 50;

    public const int MaxAdministrativeLoad = 400000;
    public const int MinAdministrativeLoad = 80000;
    public const int AdministrativeLoadSpan = MaxAdministrativeLoad - MinAdministrativeLoad;

    public const float MaxAdministrativeLoadChanceFactor = 0.05f;

    private Clan _clan;

    private CellGroup _newClanCoreGroup;

    private float _chanceOfSplitting;

    public ClanSplitDecisionEvent()
    {
        DoNotSerialize = true;
    }

    public ClanSplitDecisionEvent(Clan clan, FactionEventData data) : base(clan, data)
    {
        _clan = clan;

        DoNotSerialize = true;
    }

    public ClanSplitDecisionEvent(Clan clan, long triggerDate) : base(clan, triggerDate, ClanSplitDecisionEventId)
    {
        _clan = clan;

        DoNotSerialize = true;
    }

    public static long CalculateTriggerDate(Clan clan)
    {
        float randomFactor = clan.GetNextLocalRandomFloat(RngOffsets.CLAN_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE);
        randomFactor = Mathf.Pow(randomFactor, 2);

        float administrativeLoad = clan.CalculateAdministrativeLoad();

        float loadFactor = 1;

        if (administrativeLoad != Mathf.Infinity)
        {
            float modAdminLoad = Mathf.Max(0, administrativeLoad - MinAdministrativeLoad);

            loadFactor = AdministrativeLoadSpan / (modAdminLoad + AdministrativeLoadSpan);
        }

        float cohesionPreferenceValue = clan.GetPreferenceValue(CulturalPreference.CohesionPreferenceId);

        float cohesionPrefFactor = 2 * cohesionPreferenceValue;
        cohesionPrefFactor = Mathf.Pow(cohesionPrefFactor, 4);

        float dateSpan = (1 - randomFactor) * DateSpanFactorConstant * loadFactor * cohesionPrefFactor;

        long triggerDateSpan = (long)dateSpan + CellGroup.GenerationSpan;

        if (triggerDateSpan < 0)
        {
#if DEBUG
            Debug.LogWarning("updateSpan less than 0: " + triggerDateSpan);
#endif
            triggerDateSpan = CellGroup.MaxUpdateSpan;
        }

        long triggerDate = clan.World.CurrentDate + triggerDateSpan;

#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("ClanSplitDecisionEvent.CalculateTriggerDate - clan:" + clan.Id,
                "CurrentDate: " + clan.World.CurrentDate +
                ", administrativeLoad: " + administrativeLoad +
                ", cohesionPreferenceValue: " + cohesionPreferenceValue +
                ", triggerDate: " + triggerDate +
                "", clan.World.CurrentDate);

            Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        }
#endif

        return triggerDate;
    }

    public float GetGroupWeight(CellGroup group)
    {
        if (group == _clan.CoreGroup)
            return 0;

        PolityProminence pi = group.GetPolityProminence(_clan.Polity);

        if (group.HighestPolityProminence != pi)
            return 0;

        if (!Clan.CanBeClanCore(group))
            return 0;

        if (group.Population < Clan.MinCorePopulation)
            return 0;

        float coreDistance = pi.FactionCoreDistance - MinCoreDistance;

        if (coreDistance <= 0)
            return 0;

        float coreDistanceFactor = MinCoreDistance / (MinCoreDistance + coreDistance);

        float minCoreProminenceValue = Mathf.Max(coreDistanceFactor, Clan.MinCorePolityProminence);

        return pi.Value - minCoreProminenceValue;
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
        {
            return false;
        }

#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            if (Manager.TracingData.FactionId == Faction.Id)
            {
                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("ClanSplitDecisionEvent.CanTrigger 1 - Faction:" + Faction.Id,
                    "CurrentDate: " + World.CurrentDate +
                    ", _clan.Influence: " + _clan.Influence +
                    "", World.CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        if (_clan.Influence < MinInfluenceTrigger)
        {
            return false;
        }

        int rngOffset = unchecked((int)(RngOffsets.EVENT_CAN_TRIGGER + Id));

        //Profiler.BeginSample("CanTrigger - _clan.Polity.GetRandomGroup");
        
        _newClanCoreGroup = _clan.Polity.GetRandomGroup(rngOffset++);

        //Profiler.EndSample();

#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            if (Manager.TracingData.FactionId == Faction.Id)
            {
                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("ClanSplitDecisionEvent.CanTrigger 2 - Faction:" + Faction.Id,
                    "CurrentDate: " + World.CurrentDate +
                    ", _newClanCoreGroup.Id: " + _newClanCoreGroup.Id +
                    ", _clan.Influence: " + _clan.Influence +
                    "", World.CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        if (GetGroupWeight(_newClanCoreGroup) <= 0)
        {
            return false;
        }

        //Profiler.BeginSample("CanTrigger - _clan.PreUpdate");

        // We should use the latest cultural attribute values before calculating chances
        _clan.PreUpdate();

        //Profiler.EndSample();

        //Profiler.BeginSample("CanTrigger - CalculateChanceOfSplitting");

        _chanceOfSplitting = CalculateChanceOfSplitting();

        //Profiler.EndSample();

#if DEBUG
        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        {
            if (Manager.TracingData.FactionId == Faction.Id)
            {
                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage("ClanSplitDecisionEvent.CanTrigger 3 - Faction:" + Faction.Id,
                    "CurrentDate: " + World.CurrentDate +
                    ", _chanceOfSplitting: " + _chanceOfSplitting +
                    ", _newClanCoreGroup.Id: " + _newClanCoreGroup.Id +
                    ", _clan.Influence: " + _clan.Influence +
                    "", World.CurrentDate);

                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
            }
        }
#endif

        if (_chanceOfSplitting <= 0)
        {
            return false;
        }

        return true;
    }

    public float CalculateChanceOfSplitting()
    {
        float administrativeLoad = _clan.CalculateAdministrativeLoad();

        if (administrativeLoad == Mathf.Infinity)
            return 1;

        float cohesionPreferenceValue = _clan.GetPreferenceValue(CulturalPreference.CohesionPreferenceId);

        if (cohesionPreferenceValue <= 0)
            return 1;

        float cohesionPrefFactor = 2 * cohesionPreferenceValue;
        cohesionPrefFactor = Mathf.Pow(cohesionPrefFactor, 4);

        float authorityPreferenceValue = _clan.GetPreferenceValue(CulturalPreference.AuthorityPreferenceId);

        if (authorityPreferenceValue <= 0)
            return 1;

        float authorityPrefFactor = 2 * authorityPreferenceValue;
        authorityPrefFactor = Mathf.Pow(authorityPrefFactor, 4);

        float modMinAdministrativeLoad = MinAdministrativeLoad * cohesionPrefFactor;
        float modMaxAdministrativeLoad = modMinAdministrativeLoad + (AdministrativeLoadSpan * _clan.CurrentLeader.Wisdom * _clan.CurrentLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

        float chance = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);

        return Mathf.Clamp01(chance);
    }

    public override void Trigger()
    {
        _clan.CoreGroup.SetToUpdate();

        bool preferSplit = _clan.GetNextLocalRandomFloat(RngOffsets.CLAN_SPLITTING_EVENT_PREFER_SPLIT) < _chanceOfSplitting;

        if (_clan.Polity.IsUnderPlayerFocus || _clan.IsUnderPlayerGuidance)
        {
            Decision splitDecision;

            if (_chanceOfSplitting >= 1)
            {
                splitDecision = new ClanSplitDecision(_clan, _newClanCoreGroup, Id); // Player can't prevent splitting from happening
            }
            else
            {
                splitDecision = new ClanSplitDecision(_clan, _newClanCoreGroup, preferSplit, Id); // Give player options
            }

            if (_clan.IsUnderPlayerGuidance)
            {
                World.AddDecisionToResolve(splitDecision);
            }
            else
            {
                splitDecision.ExecutePreferredOption();
            }

        }
        else if (preferSplit)
        {
            ClanSplitDecision.LeaderAllowsSplit(_clan, _newClanCoreGroup, Id);
        }
        else
        {
            ClanSplitDecision.LeaderPreventsSplit(_clan);
        }
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        _clan = Faction as Clan;

        _clan.AddEvent(this);
    }

    protected override void DestroyInternal()
    {
        base.DestroyInternal();

        if ((Faction != null) && (Faction.StillPresent))
        {
            Clan clan = Faction as Clan;

            clan.ResetEvent(WorldEvent.ClanSplitDecisionEventId, CalculateTriggerDate(clan));
        }
    }

    public override void Reset(long newTriggerDate)
    {
        base.Reset(newTriggerDate);

        _clan = Faction as Clan;
    }
}
