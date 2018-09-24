using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class CulturalKnowledgeInfo : IKeyedValue<string>
{
    [XmlAttribute]
    public string Id;

    [XmlAttribute]
    public string Name;

    public CulturalKnowledgeInfo()
    {
    }

    public CulturalKnowledgeInfo(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public CulturalKnowledgeInfo(CulturalKnowledgeInfo baseInfo)
    {
        Id = baseInfo.Id;
        Name = baseInfo.Name;
    }

    public string GetKey()
    {
        return Id;
    }
}
