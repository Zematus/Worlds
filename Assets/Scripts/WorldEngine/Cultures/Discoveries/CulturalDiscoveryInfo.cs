using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalDiscoveryInfo : IKeyedValue<string>, ISynchronizable
{
    [XmlAttribute]
    public string Id;

    [XmlIgnore]
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

    public void Synchronize()
    {
    }

    public void FinalizeLoad()
    {
        switch (Id)
        {
            //case BoatMakingDiscovery.DiscoveryId:
            //    Name = BoatMakingDiscovery.DiscoveryName;
            //    break;

            //case PlantCultivationDiscovery.DiscoveryId:
            //    Name = PlantCultivationDiscovery.DiscoveryName;
            //    break;
                
            //case SailingDiscovery.DiscoveryId:
            //    Name = SailingDiscovery.DiscoveryName;
            //    break;

            case TribalismDiscovery.DiscoveryId:
                Name = TribalismDiscovery.DiscoveryName;
                break;

            default:
                throw new System.Exception("Unhandled Discovery Id: " + Id);
        }
    }
}
