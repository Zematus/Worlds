using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(CellCulturalActivity))]
public class CulturalActivity : CulturalActivityInfo
{
    [ProtoMember(1)]
    public float Value;

    [ProtoMember(2)]
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
