using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PlantCultivationDiscoveryEvent : DiscoveryEvent
{
    public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 600000;

    public const string EventSetFlag = "PlantCultivationDiscoveryEvent_Set";

    public PlantCultivationDiscoveryEvent()
    {

    }

    public PlantCultivationDiscoveryEvent(CellGroup group, long triggerDate) : base(group, triggerDate, PlantCultivationDiscoveryEventId)
    {
        Group.SetFlag(EventSetFlag);
    }

    public static long CalculateTriggerDate(CellGroup group)
    {
        float terrainFactor = AgricultureKnowledge.CalculateTerrainFactorIn(group.Cell);

        float randomFactor = group.Cell.GetNextLocalRandomFloat(RngOffsets.PLANT_CULTIVATION_DISCOVERY_EVENT_CALCULATE_TRIGGER_DATE);
        randomFactor = randomFactor * randomFactor;

        float dateSpan = (1 - randomFactor) * DateSpanFactorConstant;

        if (terrainFactor > 0)
        {
            dateSpan /= terrainFactor;
        }
        else
        {
            throw new System.Exception("Can't calculate valid trigger date");
        }

        long targetDate = (long)(group.World.CurrentDate + dateSpan) + 1;

        if (targetDate <= group.World.CurrentDate)
            targetDate = int.MinValue;

        return targetDate;
    }

    public static bool CanSpawnIn(CellGroup group)
    {
        if (group.IsFlagSet(EventSetFlag))
            return false;

        if (group.Culture.HasKnowledgeOrWillHave(AgricultureKnowledge.AgricultureKnowledgeId))
            return false;

        float terrainFactor = AgricultureKnowledge.CalculateTerrainFactorIn(group.Cell);

        return (terrainFactor > 0);
    }

    public override bool CanTrigger()
    {
        if (!base.CanTrigger())
            return false;

        if (Group.Culture.HasKnowledgeOrWillHave(AgricultureKnowledge.AgricultureKnowledgeId))
            return false;

        return true;
    }

    public override void Trigger()
    {
        Group.Culture.AddActivityToPerform(CellCulturalActivity.CreateFarmingActivity(Group));
        Group.Culture.TryAddDiscoveryToFind(PlantCultivationDiscovery.PlantCultivationDiscoveryId);
        Group.Culture.TryAddKnowledgeToLearn(AgricultureKnowledge.AgricultureKnowledgeId, Group, AgricultureKnowledge.InitialValue);
        World.AddGroupToUpdate(Group);

        TryGenerateEventMessage(PlantCultivationDiscoveryEventId, PlantCultivationDiscovery.PlantCultivationDiscoveryId);
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
