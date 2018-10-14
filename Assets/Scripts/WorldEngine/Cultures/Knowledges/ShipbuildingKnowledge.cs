using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class ShipbuildingKnowledge : CellCulturalKnowledge
{
    public const string ShipbuildingKnowledgeId = "ShipbuildingKnowledge";
    public const string ShipbuildingKnowledgeName = "Shipbuilding";

    public const int ShipbuildingKnowledgeRngOffset = 0;

    public const int InitialValue = 100;

    public const int MinKnowledgeValueForSailingSpawnEvent = 500;
    public const int MinKnowledgeValueForSailing = 300;
    public const int OptimalKnowledgeValueForSailing = 1000;

    public const float TimeEffectConstant = CellGroup.GenerationSpan * 500;
    public const float NeighborhoodOceanPresenceModifier = 1.5f;

    public static int HighestAsymptote = 0;

    private float _neighborhoodOceanPresence;

    public ShipbuildingKnowledge()
    {
        if (Asymptote > HighestAsymptote)
        {
            HighestAsymptote = Asymptote;
        }
    }

    public ShipbuildingKnowledge(CellGroup group, int initialValue) : base(group, ShipbuildingKnowledgeId, ShipbuildingKnowledgeName, ShipbuildingKnowledgeRngOffset, initialValue)
    {
        CalculateNeighborhoodOceanPresence();
    }

    public static bool IsShipbuildingKnowledge(CulturalKnowledge knowledge)
    {
        return knowledge.Id.Contains(ShipbuildingKnowledgeId);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        CalculateNeighborhoodOceanPresence();
    }

    public void CalculateNeighborhoodOceanPresence()
    {
        _neighborhoodOceanPresence = CalculateNeighborhoodOceanPresenceIn(Group);
    }

    public static float CalculateNeighborhoodOceanPresenceIn(CellGroup group)
    {
        float neighborhoodPresence;

        int groupCellBonus = 1;
        int cellCount = groupCellBonus;

        TerrainCell groupCell = group.Cell;

        float totalPresence = groupCell.GetBiomePresence("Ocean") * groupCellBonus;

        foreach (TerrainCell c in groupCell.Neighbors.Values)
        {
            totalPresence += c.GetBiomePresence("Ocean");
            cellCount++;
        }

        neighborhoodPresence = totalPresence / cellCount;

        if ((neighborhoodPresence < 0) || (neighborhoodPresence > 1))
        {
            throw new System.Exception("Neighborhood Ocean Presence outside range: " + neighborhoodPresence);
        }

        return neighborhoodPresence;
    }

    protected override void UpdateInternal(long timeSpan)
    {
        UpdateValueInternal(timeSpan, TimeEffectConstant, _neighborhoodOceanPresence * NeighborhoodOceanPresenceModifier);

        TryGenerateSailingDiscoveryEvent();
    }

    public override void PolityCulturalProminence(CulturalKnowledge polityKnowledge, PolityProminence polityProminence, long timeSpan)
    {
        PolityCulturalProminenceInternal(polityKnowledge, polityProminence, timeSpan, TimeEffectConstant);

        TryGenerateSailingDiscoveryEvent();
    }

    private void TryGenerateSailingDiscoveryEvent()
    {
        if (Value < SailingDiscoveryEvent.MinShipBuildingKnowledgeSpawnEventValue)
            return;

        if (Value > SailingDiscoveryEvent.OptimalShipBuildingKnowledgeValue)
            return;

        if (SailingDiscoveryEvent.CanSpawnIn(Group))
        {
            long triggerDate = SailingDiscoveryEvent.CalculateTriggerDate(Group);

            if (triggerDate > World.MaxSupportedDate)
                return;

            if (triggerDate == long.MinValue)
                return;

            Group.World.InsertEventToHappen(new SailingDiscoveryEvent(Group, triggerDate));
        }
    }

    protected override int CalculateAsymptoteInternal(CulturalDiscovery discovery)
    {
        switch (discovery.Id)
        {

            case BoatMakingDiscovery.BoatMakingDiscoveryId:
                return 1000;
            case SailingDiscovery.SailingDiscoveryId:
                return 3000;
        }

        return 0;
    }

    public override float CalculateExpectedProgressLevel()
    {
        if (_neighborhoodOceanPresence <= 0)
            return 1;

        return Mathf.Clamp(ProgressLevel / _neighborhoodOceanPresence, MinProgressLevel, 1);
    }

    public override float CalculateTransferFactor()
    {
        return (_neighborhoodOceanPresence * 0.9f) + 0.1f;
    }

    public override bool WillBeLost()
    {
        if (Value <= 0)
        {
            return !Group.InfluencingPolityHasKnowledge(Id);
        }

        return false;
    }

    public override void LossConsequences()
    {
        Profiler.BeginSample("BoatMakingDiscoveryEvent.CanSpawnIn");

        if (BoatMakingDiscoveryEvent.CanSpawnIn(Group))
        {
            Profiler.BeginSample("BoatMakingDiscoveryEvent.CalculateTriggerDate");

            long triggerDate = BoatMakingDiscoveryEvent.CalculateTriggerDate(Group);

            Profiler.EndSample();

            if ((triggerDate <= World.MaxSupportedDate) && (triggerDate > long.MinValue))
            {
                Profiler.BeginSample("InsertEventToHappen: BoatMakingDiscoveryEvent");

                Group.World.InsertEventToHappen(new BoatMakingDiscoveryEvent(Group, triggerDate));

                Profiler.EndSample();
            }
        }

        Profiler.EndSample();
    }

    protected override int GetBaseAsymptote()
    {
        return 0;
    }
}
