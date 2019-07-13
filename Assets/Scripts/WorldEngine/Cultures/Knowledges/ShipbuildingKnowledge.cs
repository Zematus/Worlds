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

    public const int MinKnowledgeValueForSailingSpawnEvent = 500;
    public const int MinKnowledgeValueForSailing = 300;
    public const int OptimalKnowledgeValueForSailing = 1000;

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

        //TryGenerateSailingDiscoveryEvent();
    }

    public override void PolityCulturalProminence(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan)
    {
        PolityCulturalProminenceInternal(polityKnowledge, polityProminence, timeSpan, TimeEffectConstant);

        //TryGenerateSailingDiscoveryEvent();
    }

    //private void TryGenerateSailingDiscoveryEvent()
    //{
    //    if (Value < SailingDiscoveryEvent.MinShipBuildingKnowledgeSpawnEventValue)
    //        return;

    //    if (Value > SailingDiscoveryEvent.OptimalShipBuildingKnowledgeValue)
    //        return;

    //    if (SailingDiscoveryEvent.CanSpawnIn(Group))
    //    {
    //        long triggerDate = SailingDiscoveryEvent.CalculateTriggerDate(Group);

    //        if (triggerDate > World.MaxSupportedDate)
    //            return;

    //        if (triggerDate == long.MinValue)
    //            return;

    //        Group.World.InsertEventToHappen(new SailingDiscoveryEvent(Group, triggerDate));
    //    }
    //}

    protected override int CalculateLimitInternal(CulturalDiscovery discovery)
    {
        //switch (discovery.Id)
        //{
        //    case BoatMakingDiscovery.DiscoveryId:
        //        return 1000;
        //    case SailingDiscovery.DiscoveryId:
        //        return 3000;
        //}

        return 0;
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

    public override bool WillBeLost()
    {
        if (Value <= 0)
        {
            return !Group.InfluencingPolityHasKnowledge(Id);
        }

        return false;
    }

    //public override void LossConsequences()
    //{
    //    Profiler.BeginSample("BoatMakingDiscoveryEvent.CanSpawnIn");

    //    if (BoatMakingDiscoveryEvent.CanSpawnIn(Group))
    //    {
    //        Profiler.BeginSample("BoatMakingDiscoveryEvent.CalculateTriggerDate");

    //        long triggerDate = BoatMakingDiscoveryEvent.CalculateTriggerDate(Group);

    //        Profiler.EndSample();

    //        if ((triggerDate <= World.MaxSupportedDate) && (triggerDate > long.MinValue))
    //        {
    //            Profiler.BeginSample("InsertEventToHappen: BoatMakingDiscoveryEvent");

    //            Group.World.InsertEventToHappen(new BoatMakingDiscoveryEvent(Group, triggerDate));

    //            Profiler.EndSample();
    //        }
    //    }

    //    Profiler.EndSample();
    //}

    protected override int GetBaseLimit()
    {
        return 0;
    }
}
