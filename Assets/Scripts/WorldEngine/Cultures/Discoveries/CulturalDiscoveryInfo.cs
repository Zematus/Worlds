using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalDiscoveryInfo : IKeyedValue<string>
{
    [XmlAttribute]
    public string Id;

    [XmlAttribute("N")]
    public string Name;

    public CulturalDiscoveryInfo()
    {
    }

    public CulturalDiscoveryInfo(string id, string name)
    {
        Id = id;

        Name = name;
    }

    public CulturalDiscoveryInfo(CulturalDiscoveryInfo baseInfo)
    {
        Id = baseInfo.Id;

        Name = baseInfo.Name;
    }

    public string GetKey()
    {
        return Id;
    }
}
