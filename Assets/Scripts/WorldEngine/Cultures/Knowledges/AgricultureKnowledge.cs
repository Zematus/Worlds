using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class AgricultureKnowledge : CellCulturalKnowledge
{
    public const string AgricultureKnowledgeId = "AgricultureKnowledge";
    public const string AgricultureKnowledgeName = "Agriculture";

    public const int AgricultureKnowledgeRngOffset = 1;

    public const float TimeEffectConstant = CellGroup.GenerationSpan * 2000;
    public const float TerrainFactorModifier = 1.5f;
    public const float MinAccesibility = 0.2f;

    public static int HighestAsymptote = 0;

    private float _terrainFactor;

    public AgricultureKnowledge()
    {
        if (Asymptote > HighestAsymptote)
        {
            HighestAsymptote = Asymptote;
        }
    }

    public AgricultureKnowledge(CellGroup group, int value = 100) : base(group, AgricultureKnowledgeId, AgricultureKnowledgeName, AgricultureKnowledgeRngOffset, value)
    {
        CalculateTerrainFactor();
    }

    public AgricultureKnowledge(CellGroup group, AgricultureKnowledge baseKnowledge) : base(group, baseKnowledge.Id, baseKnowledge.Name, AgricultureKnowledgeRngOffset, baseKnowledge.Value, baseKnowledge.Asymptote)
    {
        CalculateTerrainFactor();
    }

    public AgricultureKnowledge(CellGroup group, AgricultureKnowledge baseKnowledge, int initialValue) : base(group, baseKnowledge.Id, baseKnowledge.Name, AgricultureKnowledgeRngOffset, initialValue)
    {
        CalculateTerrainFactor();
    }

    public AgricultureKnowledge(CellGroup group, CulturalKnowledge baseKnowledge, int initialValue) : base(group, baseKnowledge.Id, baseKnowledge.Name, AgricultureKnowledgeRngOffset, initialValue)
    {
        CalculateTerrainFactor();
    }

    public static bool IsAgricultureKnowledge(CulturalKnowledge knowledge)
    {
        return knowledge.Id.Contains(AgricultureKnowledgeId);
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

    protected override int CalculateAsymptoteInternal(CulturalDiscovery discovery)
    {
        switch (discovery.Id)
        {
            case PlantCultivationDiscovery.PlantCultivationDiscoveryId:
                return 1000;
        }

        return 0;
    }

    public override float CalculateExpectedProgressLevel()
    {
        if (_terrainFactor <= 0)
            return 1;

        return Mathf.Clamp(ProgressLevel / _terrainFactor, MinProgressLevel, 1);
    }

    public override float CalculateTransferFactor()
    {
        return (_terrainFactor * 0.9f) + 0.1f;
    }

    public override bool WillBeLost()
    {
        if (Value < 100)
        {
            return !Group.InfluencingPolityHasKnowledge(Id);
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

            if ((triggerDate <= World.MaxSupportedDate) && (triggerDate > int.MinValue))
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

    protected override int CalculateBaseAsymptote()
    {
        return 0;
    }
}
