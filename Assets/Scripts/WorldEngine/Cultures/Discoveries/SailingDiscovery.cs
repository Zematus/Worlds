using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class SailingDiscovery : CellCulturalDiscovery
{
    public const string DiscoveryId = "SailingDiscovery";
    public const string DiscoveryName = "Sailing";

    public SailingDiscovery() : base(DiscoveryId, DiscoveryName)
    {

    }

    public override bool CanBeHeld(CellGroup group)
    {
        int value = 0;

        if (!group.Culture.TryGetKnowledgeValue(ShipbuildingKnowledge.KnowledgeId, out value))
            return false;

        return value >= ShipbuildingKnowledge.MinKnowledgeValueForSailing;
    }
}
