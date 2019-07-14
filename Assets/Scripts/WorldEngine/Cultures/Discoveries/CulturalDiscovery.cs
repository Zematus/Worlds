using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

//[XmlInclude(typeof(BoatMakingDiscovery))]
//[XmlInclude(typeof(SailingDiscovery))]
[XmlInclude(typeof(TribalismDiscovery))]
//[XmlInclude(typeof(PlantCultivationDiscovery))]
public class CulturalDiscovery : CulturalDiscoveryInfo
{
    public CulturalDiscovery()
    {
    }

    public CulturalDiscovery(string id, string name) : base(id, name)
    {
    }

    public CulturalDiscovery(CulturalDiscovery baseDiscovery) : base(baseDiscovery)
    {
    }
}
