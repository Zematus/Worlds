using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[XmlInclude(typeof(BiomeSurvivalSkill))]
[XmlInclude(typeof(SeafaringSkill))]
public class CulturalSkill : CulturalSkillInfo
{
    [XmlAttribute("V")]
    public float Value;

    public CulturalSkill()
    {
    }

    public CulturalSkill(string id, string name, int rngOffset, float value) : base(id, name, rngOffset)
    {
        Value = value;
    }

    public CulturalSkill(CulturalSkill baseSkill) : base(baseSkill)
    {
        Value = baseSkill.Value;
    }

    public void Reset()
    {
        Value = 0;
    }
}
