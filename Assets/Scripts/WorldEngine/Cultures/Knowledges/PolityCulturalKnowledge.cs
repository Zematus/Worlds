using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityCulturalKnowledge : CulturalKnowledge
{
    [XmlIgnore]
    public float AccValue = 0;
    
    public PolityCulturalKnowledge()
    {
    }

    public PolityCulturalKnowledge(CulturalKnowledge baseKnowledge) : base(baseKnowledge)
    {
        Value = 0; // this should be set by calling FinalizeUpdateFromFactions afterwards
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
