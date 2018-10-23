using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class BoatMakingDiscovery : CellCulturalDiscovery
{
    public const string DiscoveryId = "BoatMakingDiscovery";
    public const string DiscoveryName = "Boat Making";

    public BoatMakingDiscovery() : base(DiscoveryId, DiscoveryName)
    {

    }

    public override bool CanBeHeld(CellGroup group)
    {
        if (group.Culture.HasOrWillHaveKnowledge(ShipbuildingKnowledge.KnowledgeId))
        {
            return true;
        }

        return false;
    }
}
