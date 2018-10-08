using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class FactionCulturalDiscovery : CulturalDiscovery
{
    [XmlIgnore]
    public CulturalDiscovery PolityCulturalDiscovery;

    [XmlIgnore]
    public CulturalDiscovery CoreCulturalDiscovery;

    public FactionCulturalDiscovery(CulturalDiscovery baseDiscovery, CellCulture coreCulture, PolityCulture polityCulture) : base(baseDiscovery)
    {
        SetCoreCulturalDiscovery(coreCulture);
        SetPolityCulturalDiscovery(polityCulture);
    }

    public void SetCoreCulturalDiscovery(CellCulture culture)
    {
        CoreCulturalDiscovery = culture.GetDiscovery(Id);
    }

    public void SetPolityCulturalDiscovery(PolityCulture culture)
    {
        PolityCulturalDiscovery = culture.GetDiscovery(Id);

        if (PolityCulturalDiscovery == null)
        {
            PolityCulturalDiscovery = new CulturalDiscovery(Id, Name);

            culture.AddDiscovery(PolityCulturalDiscovery);
        }
    }

    public void UpdatePolityDiscovery()
    {
        PolityCulturalDiscovery.Set(true);
    }

    public void UpdateFromCoreDiscovery()
    {
        Set(true);
    }
}
