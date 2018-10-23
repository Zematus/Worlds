using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class CulturalKnowledgeInfo : IKeyedValue<string>, ISynchronizable
{
    [XmlAttribute]
    public string Id;

    [XmlIgnore]
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

    public virtual void Synchronize()
    {
    }

    public virtual void FinalizeLoad()
    {
        switch (Id)
        {
            case AgricultureKnowledge.KnowledgeId:
                Name = AgricultureKnowledge.KnowledgeName;
                break;

            case ShipbuildingKnowledge.KnowledgeId:
                Name = ShipbuildingKnowledge.KnowledgeName;
                break;

            case SocialOrganizationKnowledge.KnowledgeId:
                Name = SocialOrganizationKnowledge.KnowledgeName;
                break;

            default:
                throw new System.Exception("Unhandled Knowledge Id: " + Id);
        }
    }
}
