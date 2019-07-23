using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class AgricultureKnowledge : CellCulturalKnowledge
{
    public const string KnowledgeId = "agriculture";
    public const string KnowledgeName = "agriculture";

    public const int InitialValue = 100;

    public const int BaseLimit = MinLimitValue;

    public const int KnowledgeRngOffset = 1;

    public const float TimeEffectConstant = CellGroup.GenerationSpan * 2000;
    public const float TerrainFactorModifier = 1.5f;
    public const float MinAccesibility = 0.2f;

    public static int HighestLimit = 0;

    [XmlIgnore]
    public float TerrainFactor;

    public AgricultureKnowledge()
    {
        if (Limit > HighestLimit)
        {
            HighestLimit = Limit;
        }
    }

    public AgricultureKnowledge(CellGroup group, int initialValue, int initialLimit) 
        : base(group, KnowledgeId, KnowledgeName, KnowledgeRngOffset, initialValue, initialLimit)
    {
        UpdateTerrainFactor();
    }

    public static bool IsAgricultureKnowledge(CulturalKnowledge knowledge)
    {
        return knowledge.Id.Contains(KnowledgeId);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        UpdateTerrainFactor();
    }

    public void UpdateTerrainFactor()
    {
        TerrainFactor = CalculateTerrainFactorIn(Group.Cell);
    }

    public static float CalculateTerrainFactorIn(TerrainCell cell)
    {
        float accesibilityFactor = (cell.ModifiedAccessibility - MinAccesibility) / (1f - MinAccesibility);

        return Mathf.Clamp01(cell.ModifiedArability * cell.ModifiedAccessibility * accesibilityFactor);
    }

    protected override void UpdateInternal(long timeSpan)
    {
        UpdateTerrainFactor();

        UpdateValueInternal(timeSpan, TimeEffectConstant, TerrainFactor * TerrainFactorModifier);
    }

    public override void AddPolityProminenceEffect(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan)
    {
        AddPolityProminenceEffectInternal(polityKnowledge, polityProminence, timeSpan, TimeEffectConstant);
    }

    public override float CalculateExpectedProgressLevel()
    {
        if (TerrainFactor <= 0)
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

        return Mathf.Clamp(ProgressLevel / TerrainFactor, MinProgressLevel, 1);
    }

    public override float CalculateTransferFactor()
    {
        return (TerrainFactor * 0.9f) + 0.1f;
    }
}
