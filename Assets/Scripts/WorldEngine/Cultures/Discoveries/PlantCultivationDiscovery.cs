using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PlantCultivationDiscovery : CellCulturalDiscovery
{
    public const string PlantCultivationDiscoveryId = "PlantCultivationDiscovery";
    public const string PlantCultivationDiscoveryName = "Plant Cultivation";

    public PlantCultivationDiscovery() : base(PlantCultivationDiscoveryId, PlantCultivationDiscoveryName)
    {

    }

    public override bool CanBeHeld(CellGroup group)
    {
        if (group.Culture.HasOrWillHaveKnowledge(AgricultureKnowledge.AgricultureKnowledgeId))
            return true;

        return false;
    }
}
