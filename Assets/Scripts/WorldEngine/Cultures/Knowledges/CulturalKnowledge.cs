using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

[XmlInclude(typeof(ShipbuildingKnowledge))]
[XmlInclude(typeof(AgricultureKnowledge))]
[XmlInclude(typeof(SocialOrganizationKnowledge))]
[XmlInclude(typeof(FactionCulturalKnowledge))]
public class CulturalKnowledge : CulturalKnowledgeInfo, IFilterableValue
{
    public const float ValueScaleFactor = 0.01f;

    [XmlAttribute("P")]
    public bool IsPresent;

    [XmlAttribute("V")]
    public int Value;

    [XmlIgnore]
    public bool WasPresent { get; private set; }

    public CulturalKnowledge()
    {
    }

    public CulturalKnowledge(string id, string name, int value) : base(id, name)
    {
        Value = value;

        IsPresent = false;
        WasPresent = false;
    }

    public CulturalKnowledge(CulturalKnowledge baseKnowledge) : base(baseKnowledge)
    {
        Value = baseKnowledge.Value;

        IsPresent = true;
        WasPresent = false;
    }

    public float ScaledValue
    {
        get { return Value * ValueScaleFactor; }
    }

    public virtual void Reset()
    {
        Value = 0;

        IsPresent = false;
        WasPresent = true;
    }

    public void Set()
    {
        IsPresent = true;
        WasPresent = false;
    }

    public bool ShouldFilter()
    {
        return IsPresent;
    }
}
