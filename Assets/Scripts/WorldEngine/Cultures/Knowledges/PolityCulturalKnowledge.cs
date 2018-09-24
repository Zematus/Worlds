using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityCulturalKnowledge : CulturalKnowledge
{
    [XmlIgnore]
    public float AggregateValue;

    public PolityCulturalKnowledge()
    {
    }

    public PolityCulturalKnowledge(string id, string name, int value) : base(id, name, value)
    {
    }

    public PolityCulturalKnowledge(CulturalKnowledge baseKnowledge) : base(baseKnowledge)
    {
    }
}
