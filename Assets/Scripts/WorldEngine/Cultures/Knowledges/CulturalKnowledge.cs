using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

[XmlInclude(typeof(ShipbuildingKnowledge))]
[XmlInclude(typeof(AgricultureKnowledge))]
[XmlInclude(typeof(SocialOrganizationKnowledge))]
public class CulturalKnowledge : CulturalKnowledgeInfo
{
    public const float ValueScaleFactor = 0.01f;

    [XmlAttribute]
    public int Value;

    public CulturalKnowledge()
    {
    }

    public CulturalKnowledge(string id, string name, int value) : base(id, name)
    {
        Value = value;
    }

    public CulturalKnowledge(CulturalKnowledge baseKnowledge) : base(baseKnowledge)
    {
        Value = baseKnowledge.Value;
    }

    public float ScaledValue
    {
        get { return Value * ValueScaleFactor; }
    }
}
