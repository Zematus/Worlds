using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class SailingDiscovery : CellCulturalDiscovery
{
    public const string SailingDiscoveryId = "SailingDiscovery";
    public const string SailingDiscoveryName = "Sailing";

    public SailingDiscovery() : base(SailingDiscoveryId, SailingDiscoveryName)
    {

    }

    public override bool CanBeHeld(CellGroup group)
    {
        int value = 0;

        if (!group.Culture.TryGetKnowledgeValue(ShipbuildingKnowledge.ShipbuildingKnowledgeId, out value))
            return false;

        return value >= ShipbuildingKnowledge.MinKnowledgeValueForSailing;
    }
}
