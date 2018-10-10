using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class SailingDiscoveryEvent : DiscoveryEvent
{
    public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 10000;

    public const int MinShipBuildingKnowledgeSpawnEventValue = ShipbuildingKnowledge.MinKnowledgeValueForSailingSpawnEvent;
    public const int MinShipBuildingKnowledgeValue = ShipbuildingKnowledge.MinKnowledgeValueForSailing;
    public const int OptimalShipBuildingKnowledgeValue = ShipbuildingKnowledge.OptimalKnowledgeValueForSailing;

    public const string EventSetFlag = "SailingDiscoveryEvent_Set";

    public SailingDiscoveryEvent()
    {

    }

    public SailingDiscoveryEvent(CellGroup group, long triggerDate) : base(group, triggerDate, SailingDiscoveryEventId)
    {
        Group.SetFlag(EventSetFlag);
    }

    public static long CalculateTriggerDate(CellGroup group)
    {
        int shipBuildingValue = 0;

        group.Culture.TryGetKnowledgeValue(ShipbuildingKnowledge.ShipbuildingKnowledgeId, out shipBuildingValue);

        float randomFactor = group.Cell.GetNextLocalRandomFloat(RngOffsets.SAILING_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE);
        randomFactor = randomFactor * randomFactor;

        float shipBuildingFactor = (shipBuildingValue - MinShipBuildingKnowledgeValue) / (float)(OptimalShipBuildingKnowledgeValue - MinShipBuildingKnowledgeValue);
        shipBuildingFactor = Mathf.Clamp01(shipBuildingFactor) + 0.001f;

        float dateSpan = (1 - randomFactor) * DateSpanFactorConstant / shipBuildingFactor;

        long targetDate = (long)(group.World.CurrentDate + dateSpan) + 1;

        return targetDate;
    }

    public static bool CanSpawnIn(CellGroup group)
    {
        if (group.IsFlagSet(EventSetFlag))
            return false;

        if (group.Culture.HasDiscoveryOrWillHave(SailingDiscovery.SailingDiscoveryId))
            return false;

        return true;
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
            return false;
        
        if (Group.Culture.HasDiscoveryOrWillHave(SailingDiscovery.SailingDiscoveryId))
            return false;

        int value = 0;

        if (!Group.Culture.TryGetKnowledgeValue(ShipbuildingKnowledge.ShipbuildingKnowledgeId, out value))
            return false;

        return value >= MinShipBuildingKnowledgeValue;
    }

    public override void Trigger()
    {
        Group.Culture.TryAddDiscoveryToFind(SailingDiscovery.SailingDiscoveryId);
        World.AddGroupToUpdate(Group);

        TryGenerateEventMessage(SailingDiscoveryEventId, SailingDiscovery.SailingDiscoveryId);
    }

    protected override void DestroyInternal()
    {
        if (Group != null)
        {
            Group.UnsetFlag(EventSetFlag);
        }

        base.DestroyInternal();
    }
}
