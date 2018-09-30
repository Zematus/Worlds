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
        CulturalKnowledge knowledge = group.Culture.GetKnowledge(AgricultureKnowledge.AgricultureKnowledgeId);

        if (knowledge == null)
            return false;

        return true;
    }
}
