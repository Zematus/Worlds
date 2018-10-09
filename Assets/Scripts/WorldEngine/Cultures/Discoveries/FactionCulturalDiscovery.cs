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
    public CellCulturalDiscovery CoreCulturalDiscovery;

    public FactionCulturalDiscovery(CellCulturalDiscovery coreDiscovery, PolityCulture polityCulture) : base(coreDiscovery)
    {
        CoreCulturalDiscovery = coreDiscovery;

        SetPolityCulturalDiscovery(polityCulture);
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
        if (IsPresent)
        {
            PolityCulturalDiscovery.Set(true);
        }
    }

    public void UpdateFromCoreDiscovery()
    {
        Set(true);
    }
}
