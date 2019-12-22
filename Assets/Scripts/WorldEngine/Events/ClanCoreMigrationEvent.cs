using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class ClanCoreMigrationEvent : FactionEvent
{
    public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 500;

    private CellGroup _targetGroup;

    public ClanCoreMigrationEvent()
    {
        DoNotSerialize = true;
    }

    public ClanCoreMigrationEvent(Clan clan, FactionEventData data) : base(clan, data)
    {
        DoNotSerialize = true;
    }

    public ClanCoreMigrationEvent(Clan clan, long triggerDate) : base(clan, triggerDate, ClanCoreMigrationEventId)
    {
        DoNotSerialize = true;
    }

    public static long CalculateTriggerDate(Clan clan)
    {
        float randomFactor = clan.GetNextLocalRandomFloat(RngOffsets.CLAN_CORE_MIGRATION_EVENT_CALCULATE_TRIGGER_DATE);
        randomFactor = Mathf.Pow(randomFactor, 2);

        float dateSpan = (1 - randomFactor) * DateSpanFactorConstant;

        long triggerDate = (long)(clan.World.CurrentDate + dateSpan) + CellGroup.GenerationSpan;

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
        if (!base.CanTrigger())
            return false;

        Clan clan = Faction as Clan;

        _targetGroup = clan.GetCoreGroupMigrationTarget();

        if (_targetGroup == null)
            return false;

        return Faction.ShouldMigrateFactionCore(Faction.CoreGroup, _targetGroup);
    }

    public override void Trigger()
    {
        World.AddGroupToUpdate(_targetGroup);

        Faction.SetToUpdate();

        Faction.PrepareNewCoreGroup(_targetGroup);
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

            clan.ResetEvent(ClanCoreMigrationEventId, triggerDate);
        }
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        Clan clan = Faction as Clan;

        clan.AddEvent(this);
    }
}
