using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalActivityInfo : IKeyedValue<string>, ISynchronizable
{
    [XmlAttribute]
    public string Id;

    [XmlIgnore]
    public string Name;

    [XmlIgnore]
    public int RngOffset;

    public CulturalActivityInfo()
    {
    }

    public CulturalActivityInfo(string id, string name, int rngOffset)
    {
        Id = id;
        Name = name;
        RngOffset = rngOffset;
    }

    public CulturalActivityInfo(CulturalActivityInfo baseInfo)
    {
        Id = baseInfo.Id;
        Name = baseInfo.Name;
        RngOffset = baseInfo.RngOffset;
    }

    public string GetKey()
    {
        return Id;
    }

    public virtual void Synchronize()
    {
    }

    public virtual void FinalizeLoad()
    {
        switch (Id)
        {
            case CellCulturalActivity.FarmingActivityId:
                Name = CellCulturalActivity.FarmingActivityName;
                RngOffset = CellCulturalActivity.FarmingActivityRngOffset;
                break;

            case CellCulturalActivity.ForagingActivityId:
                Name = CellCulturalActivity.ForagingActivityName;
                RngOffset = CellCulturalActivity.ForagingActivityRngOffset;
                break;

            case CellCulturalActivity.FishingActivityId:
                Name = CellCulturalActivity.FishingActivityName;
                RngOffset = CellCulturalActivity.FishingActivityRngOffset;
                break;

            default:
                throw new System.Exception("Unhandled Activity Id: " + Id);
        }
    }
}
