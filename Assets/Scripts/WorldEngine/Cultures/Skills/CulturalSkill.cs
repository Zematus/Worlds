using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[XmlInclude(typeof(BiomeSurvivalSkill))]
[XmlInclude(typeof(SeafaringSkill))]
public class CulturalSkill : CulturalSkillInfo
{
    public static HashSet<string> ValidSkillIds;

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

    public static void AddValidSkillId(string id)
    {
        ValidSkillIds.Add(id);
    }

    public static void ResetSkills()
    {
        ValidSkillIds = new HashSet<string>();
    }

    public static void InitializeBaseSkills()
    {
        AddValidSkillId(SeafaringSkill.SkillId);
    }
}
