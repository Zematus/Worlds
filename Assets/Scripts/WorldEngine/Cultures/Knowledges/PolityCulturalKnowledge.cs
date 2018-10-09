using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityCulturalKnowledge : CulturalKnowledge
{
    [XmlIgnore]
    public float AccValue;

    public PolityCulturalKnowledge()
    {
    }

    public PolityCulturalKnowledge(string id, string name, int value) : base(id, name, value)
    {
    }

    public void FinalizeUpdateFromFactions()
    {
        Value = Mathf.FloorToInt(AccValue);
    }

    public override void Reset()
    {
        AccValue = 0;

        base.Reset();
    }
}
