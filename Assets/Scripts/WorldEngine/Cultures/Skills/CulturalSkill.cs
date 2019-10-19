using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(CellCulturalSkill))]
public class CulturalSkill : CulturalSkillInfo
{
    [ProtoMember(1)]
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
