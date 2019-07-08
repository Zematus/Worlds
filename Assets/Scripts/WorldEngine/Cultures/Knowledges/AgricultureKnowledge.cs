using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class AgricultureKnowledge : CellCulturalKnowledge
{
    public const string KnowledgeId = "AgricultureKnowledge";
    public const string KnowledgeName = "Agriculture";

    public const int InitialValue = 100;

    public const int KnowledgeRngOffset = 1;

    public const float TimeEffectConstant = CellGroup.GenerationSpan * 2000;
    public const float TerrainFactorModifier = 1.5f;
    public const float MinAccesibility = 0.2f;

    public static int HighestLimit = 0;

    private float _terrainFactor;

    public AgricultureKnowledge()
    {
        if (Limit > HighestLimit)
        {
            HighestLimit = Limit;
        }
    }

    public AgricultureKnowledge(CellGroup group, int initialValue, List<string> levelLimitIds) 
        : base(group, KnowledgeId, KnowledgeName, KnowledgeRngOffset, initialValue, levelLimitIds)
    {
        CalculateTerrainFactor();
    }

    public static bool IsAgricultureKnowledge(CulturalKnowledge knowledge)
    {
        return knowledge.Id.Contains(KnowledgeId);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        CalculateTerrainFactor();
    }

    public void CalculateTerrainFactor()
    {
        _terrainFactor = CalculateTerrainFactorIn(Group.Cell);
    }

    public static float CalculateTerrainFactorIn(TerrainCell cell)
    {
        float accesibilityFactor = (cell.Accessibility - MinAccesibility) / (1f - MinAccesibility);

        return Mathf.Clamp01(cell.Arability * cell.Accessibility * accesibilityFactor);
    }

    protected override void UpdateInternal(long timeSpan)
    {
        UpdateValueInternal(timeSpan, TimeEffectConstant, _terrainFactor * TerrainFactorModifier);
    }

    public override void PolityCulturalProminence(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan)
    {
        PolityCulturalProminenceInternal(polityKnowledge, polityProminence, timeSpan, TimeEffectConstant);
    }

    protected override int CalculateLimitInternal(CulturalDiscovery discovery)
    {
        switch (discovery.Id)
        {
            case PlantCultivationDiscovery.DiscoveryId:
                return 1000;
        }

        return 0;
    }

    public override float CalculateExpectedProgressLevel()
    {
        if (_terrainFactor <= 0)
            return 1;

//#if DEBUG
//        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//        {
//            if (Group.Id == Manager.TracingData.GroupId)
//            {
//                SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                    "AgricultureKnowledge.CalculateExpectedProgressLevel -  Group.Id:" + Group.Id,
//                    "CurrentDate: " + Group.World.CurrentDate +
//                    ", _terrainFactor: " + _terrainFactor +
//                    ", ProgressLevel: " + ProgressLevel +
//                    "");

//                Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//            }
//        }
//#endif

        return Mathf.Clamp(ProgressLevel / _terrainFactor, MinProgressLevel, 1);
    }

    public override float CalculateTransferFactor()
    {
        return (_terrainFactor * 0.9f) + 0.1f;
    }

    public override bool WillBeLost()
    {
        if (Value <= 0)
        {
            bool polityHasKnowledge = Group.InfluencingPolityHasKnowledge(Id);

//#if DEBUG
//            if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
//            {
//                if (Group.Id == Manager.TracingData.GroupId)
//                {
//                    string groupId = "Id:" + Group.Id + "|Long:" + Group.Longitude + "|Lat:" + Group.Latitude;

//                    SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
//                        "AgricultureKnowledge.WillBeLost - Group:" + groupId,
//                        "CurrentDate: " + Group.World.CurrentDate +
//                        ", Id: " + Id +
//                        ", IsPresent: " + IsPresent +
//                        ", Value: " + Value +
//                        ", polityHasKnowledge: " + polityHasKnowledge +
//                        "");

//                    Manager.RegisterDebugEvent("DebugMessage", debugMessage);
//                }
//            }
//#endif

            return !polityHasKnowledge;
        }

        return false;
    }

    public override void LossConsequences()
    {
        Profiler.BeginSample("RemoveActivity: FarmingActivity");

        Group.Culture.RemoveActivity(CellCulturalActivity.FarmingActivityId);

        Profiler.EndSample();

        Profiler.BeginSample("PlantCultivationDiscoveryEvent.CanSpawnIn");

        if (PlantCultivationDiscoveryEvent.CanSpawnIn(Group))
        {
            Profiler.BeginSample("PlantCultivationDiscoveryEvent.CalculateTriggerDate");

            long triggerDate = PlantCultivationDiscoveryEvent.CalculateTriggerDate(Group);

            Profiler.EndSample();
            
            if (triggerDate.IsInsideRange(Group.World.CurrentDate + 1, World.MaxSupportedDate))
            {
                Profiler.BeginSample("new PlantCultivationDiscoveryEvent");

                PlantCultivationDiscoveryEvent plantCultivationDiscoveryEvent = new PlantCultivationDiscoveryEvent(Group, triggerDate);

                Profiler.EndSample();

                Profiler.BeginSample("InsertEventToHappen: PlantCultivationDiscoveryEvent");

                Group.World.InsertEventToHappen(plantCultivationDiscoveryEvent);

                Profiler.EndSample();
            }
        }

        Profiler.EndSample();

        Group.Cell.FarmlandPercentage = 0;
    }

    protected override int GetBaseLimit()
    {
        return 0;
    }
}
