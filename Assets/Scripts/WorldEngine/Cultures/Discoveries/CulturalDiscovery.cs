using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[XmlInclude(typeof(BoatMakingDiscovery))]
[XmlInclude(typeof(SailingDiscovery))]
[XmlInclude(typeof(TribalismDiscovery))]
[XmlInclude(typeof(PlantCultivationDiscovery))]
public class CulturalDiscovery : IKeyedValue<string>
{
    [XmlAttribute]
    public string Id;

    [XmlAttribute]
    public string Name;

    public CulturalDiscovery()
    {
    }

    public CulturalDiscovery(string id, string name)
    {
        Id = id;

        Name = name;
    }

    public CulturalDiscovery(CulturalDiscovery baseDiscovery)
    {
        Id = baseDiscovery.Id;

        Name = baseDiscovery.Name;
    }

    public string GetKey()
    {
        return Id;
    }
}
