using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[XmlInclude(typeof(BoatMakingDiscovery))]
[XmlInclude(typeof(SailingDiscovery))]
[XmlInclude(typeof(TribalismDiscovery))]
[XmlInclude(typeof(PlantCultivationDiscovery))]
public class CulturalDiscovery : CulturalDiscoveryInfo, IFilterableValue
{
    [XmlAttribute("P")]
    public bool IsPresent;

    [XmlIgnore]
    public bool WasPresent { get; private set; }

    public CulturalDiscovery()
    {
    }

    public CulturalDiscovery(string id, string name) : base(id, name)
    {
        IsPresent = false;
        WasPresent = false;
    }

    public CulturalDiscovery(CulturalDiscovery baseDiscovery) : base(baseDiscovery)
    {
        IsPresent = true;
        WasPresent = false;
    }

    public void Set()
    {
        IsPresent = true;
        WasPresent = false;
    }

    public void Reset()
    {
        IsPresent = false;
        WasPresent = true;
    }

    public bool ShouldFilter()
    {
        return !IsPresent;
    }
}
