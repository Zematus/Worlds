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

    public static bool IsSailingDiscovery(CulturalDiscovery discovery)
    {
        return discovery.Id.Contains(SailingDiscoveryId);
    }

    public override bool CanBeHeld(CellGroup group)
    {
        CulturalKnowledge knowledge = group.Culture.GetKnowledge(ShipbuildingKnowledge.ShipbuildingKnowledgeId);

        if (knowledge == null)
            return false;

        if (knowledge.Value < ShipbuildingKnowledge.MinKnowledgeValueForSailing)
            return false;

        return true;
    }
}
