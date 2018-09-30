using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class BoatMakingDiscovery : CellCulturalDiscovery
{
    public const string BoatMakingDiscoveryId = "BoatMakingDiscovery";
    public const string BoatMakingDiscoveryName = "Boat Making";

    public BoatMakingDiscovery() : base(BoatMakingDiscoveryId, BoatMakingDiscoveryName)
    {

    }

    public override bool CanBeHeld(CellGroup group)
    {
        CulturalKnowledge knowledge = group.Culture.GetKnowledge(ShipbuildingKnowledge.ShipbuildingKnowledgeId);

        if (knowledge == null)
            return false;

        return true;
    }
}
