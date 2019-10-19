using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(CulturalSkill))]
public class CulturalSkillInfo : IKeyedValue<string>, ISynchronizable
{
    [ProtoMember(1)]
    public string Id;

    public int RngOffset;

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
        if (Id.Contains(BiomeSurvivalSkill.SkillIdSuffix))
        {
            string idPrefix = BiomeSurvivalSkill.GetBiomeId(Id);
            Biome biome = Biome.Biomes[idPrefix];

            Name = BiomeSurvivalSkill.GenerateName(biome);
            RngOffset = BiomeSurvivalSkill.GenerateRngOffset(biome);
        }
        else
        {
            switch (Id)
            {
                case SeafaringSkill.SkillId:
                    Name = SeafaringSkill.SkillName;
                    RngOffset = SeafaringSkill.SkillRngOffset;
                    break;

                default:
                    throw new System.Exception("Unhandled Skill Id: " + Id);
            }
        }
    }
}
