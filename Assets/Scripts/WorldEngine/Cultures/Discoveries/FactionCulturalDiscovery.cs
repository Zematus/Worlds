using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class FactionCulturalDiscovery : CulturalDiscovery
{
    [XmlIgnore]
    public Faction Faction;

    [XmlIgnore]
    public CulturalDiscovery PolityCulturalDiscovery;

    [XmlIgnore]
    public CellCulturalDiscovery CoreCulturalDiscovery;

    public FactionCulturalDiscovery()
    {
    }

    public FactionCulturalDiscovery(Faction faction, CellCulturalDiscovery coreDiscovery, PolityCulture polityCulture) : base(coreDiscovery)
    {
        Faction = faction;

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
            Profiler.BeginSample("PolityCulturalDiscovery.Set(true)");

            PolityCulturalDiscovery.Set(true);

            Profiler.EndSample();
        }
    }

    public void UpdateFromCoreDiscovery()
    {
        Set(true);
    }
}
