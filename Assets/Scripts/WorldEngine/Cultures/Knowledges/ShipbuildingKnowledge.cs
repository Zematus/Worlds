using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class ShipbuildingKnowledge : CellCulturalKnowledge
{
    public const string KnowledgeId = "shipbuilding";
    public const string KnowledgeName = "shipbuilding";

    public const int KnowledgeRngOffset = 0;

    public const float BaseLimit = KnowledgeLimit.MinLimitValue;

    public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;
    public const float NeighborhoodSeaPresenceModifier = 1.5f;

    private float _neighborhoodSeaPresence;

    public ShipbuildingKnowledge()
    {
    }

    public ShipbuildingKnowledge(CellGroup group, float initialValue, KnowledgeLimit limit) 
        : base(group, KnowledgeId, KnowledgeName, KnowledgeRngOffset, initialValue, limit)
    {
        CalculateNeighborhoodSeaPresence();
    }

    public static bool IsShipbuildingKnowledge(CulturalKnowledge knowledge)
    {
        return knowledge.Id.Contains(KnowledgeId);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        CalculateNeighborhoodSeaPresence();
    }

    public void CalculateNeighborhoodSeaPresence()
    {
        _neighborhoodSeaPresence = CalculateNeighborhoodWaterPresenceIn(Group);
    }

    public static float CalculateNeighborhoodWaterPresenceIn(CellGroup group)
    {
        float neighborhoodPresence;

        int groupCellBonus = 1;
        int cellCount = groupCellBonus;

        TerrainCell groupCell = group.Cell;

        float totalPresence = groupCell.WaterBiomePresence * groupCellBonus;

        foreach (TerrainCell c in groupCell.NeighborList)
        {
            totalPresence += c.WaterBiomePresence;
            cellCount++;
        }

        neighborhoodPresence = totalPresence / cellCount;

        if ((neighborhoodPresence < 0) || (neighborhoodPresence > 1))
        {
            throw new System.Exception("Neighborhood sea presence outside range: " + neighborhoodPresence);
        }

        return neighborhoodPresence;
    }

    protected override void UpdateInternal(long timeSpan)
    {
        UpdateValueInternal(timeSpan, TimeEffectConstant, _neighborhoodSeaPresence * NeighborhoodSeaPresenceModifier);
    }

    public override void AddPolityProminenceEffect(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan)
    {
        AddPolityProminenceEffectInternal(polityKnowledge, polityProminence, timeSpan, TimeEffectConstant);
    }

    public override float CalculateExpectedProgressLevel()
    {
        if (_neighborhoodSeaPresence <= 0)
            return 1;

        return Mathf.Clamp(ProgressLevel / _neighborhoodSeaPresence, MinProgressLevel, 1);
    }

    public override float CalculateTransferFactor()
    {
        return (_neighborhoodSeaPresence * 0.9f) + 0.1f;
    }
}
