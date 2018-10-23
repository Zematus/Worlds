using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PlantCultivationDiscovery : CellCulturalDiscovery
{
    public const string DiscoveryId = "PlantCultivationDiscovery";
    public const string DiscoveryName = "Plant Cultivation";

    public PlantCultivationDiscovery() : base(DiscoveryId, DiscoveryName)
    {

    }

    public override bool CanBeHeld(CellGroup group)
    {
        if (group.Culture.HasOrWillHaveKnowledge(AgricultureKnowledge.KnowledgeId))
            return true;

        return false;
    }
}
