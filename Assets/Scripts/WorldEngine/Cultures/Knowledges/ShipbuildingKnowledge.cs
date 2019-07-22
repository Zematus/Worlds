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

    public const int InitialValue = 100;

    public const int BaseLimit = 0;

    public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;
    public const float NeighborhoodSeaPresenceModifier = 1.5f;

    public static int HighestLimit = 0;

    private float _neighborhoodSeaPresence;

    public ShipbuildingKnowledge()
    {
        if (Limit > HighestLimit)
        {
            HighestLimit = Limit;
        }
    }

    public ShipbuildingKnowledge(CellGroup group, int initialValue, int initialLimit) 
        : base(group, KnowledgeId, KnowledgeName, KnowledgeRngOffset, initialValue, initialLimit)
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
        _neighborhoodSeaPresence = CalculateNeighborhoodSeaPresenceIn(Group);
    }

    public static float CalculateNeighborhoodSeaPresenceIn(CellGroup group)
    {
        float neighborhoodPresence;

        int groupCellBonus = 1;
        int cellCount = groupCellBonus;

        TerrainCell groupCell = group.Cell;

        float totalPresence = groupCell.SeaBiomePresence * groupCellBonus;

        foreach (TerrainCell c in groupCell.Neighbors.Values)
        {
            totalPresence += c.SeaBiomePresence;
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
