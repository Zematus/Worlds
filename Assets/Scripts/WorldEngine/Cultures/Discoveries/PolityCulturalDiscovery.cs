using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PolityCulturalDiscovery : CulturalDiscovery
{
    [XmlAttribute]
    public int PresenceCount = 0;

    public PolityCulturalDiscovery()
    {

    }

    public PolityCulturalDiscovery(string id, string name) : base(id, name)
    {

    }

    public PolityCulturalDiscovery(CulturalDiscovery baseDiscovery) : base(baseDiscovery)
    {

    }
}
