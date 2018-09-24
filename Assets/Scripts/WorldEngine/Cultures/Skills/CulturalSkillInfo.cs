using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalSkillInfo : IKeyedValue<string>
{
    [XmlAttribute]
    public string Id;

    [XmlAttribute("N")]
    public string Name;

    [XmlAttribute("RO")]
    public int RngOffset;

    public CulturalSkillInfo()
    {
    }

    public CulturalSkillInfo(string id, string name, int rngOffset)
    {
        Id = id;
        Name = name;
        RngOffset = rngOffset;
    }

    public CulturalSkillInfo(CulturalSkillInfo baseInfo)
    {
        Id = baseInfo.Id;
        Name = baseInfo.Name;
        RngOffset = baseInfo.RngOffset;
    }

    public string GetKey()
    {
        return Id;
    }
}
