using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[XmlInclude(typeof(CellCulturalActivity))]
public class CulturalActivity : CulturalActivityInfo
{
    [XmlAttribute("V")]
    public float Value;

    [XmlAttribute("C")]
    public float Contribution = 0;

    public CulturalActivity()
    {
    }

    public CulturalActivity(string id, string name, int rngOffset, float value, float contribution) : base(id, name, rngOffset)
    {
        Value = value;
        Contribution = contribution;
    }

    public CulturalActivity(CulturalActivity baseActivity) : base(baseActivity)
    {
        Value = baseActivity.Value;
        Contribution = baseActivity.Contribution;
    }

    public void Reset()
    {
        Value = 0;
        Contribution = 0;
    }
}
