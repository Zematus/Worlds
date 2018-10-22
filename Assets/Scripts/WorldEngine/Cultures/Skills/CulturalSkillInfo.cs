using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalSkillInfo : IKeyedValue<string>, ISynchronizable
{
    [XmlAttribute]
    public string Id;

    [XmlAttribute("RO")]
    public int RngOffset;

    [XmlIgnore]
    public string Name;

    public CulturalSkillInfo()
    {
    }

    public CulturalSkillInfo(string id, string name, int rngOffset)
    {
        Id = id;
        Name = name;
        RngOffset = rngOffset;
    }

    public CulturalSkillInfo(CulturalSkillInfo baseInfo)
    {
        Id = baseInfo.Id;
        Name = baseInfo.Name;
        RngOffset = baseInfo.RngOffset;
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
        if (Id.Contains(BiomeSurvivalSkill.BiomeSurvivalSkillIdPrefix))
        {
            string idSuffix = Id.Substring(BiomeSurvivalSkill.BiomeSurvivalSkillIdPrefix.Length);

            Name = BiomeSurvivalSkill.GenerateName(Biome.Biomes[idSuffix.Replace('_', ' ')]);
        }
        else
        {
            switch (Id)
            {
                case SeafaringSkill.SeafaringSkillId:
                    Name = SeafaringSkill.SeafaringSkillName;
                    break;

                default:
                    throw new System.Exception("Unhandled Skill Id: " + Id);
            }
        }
    }
}
