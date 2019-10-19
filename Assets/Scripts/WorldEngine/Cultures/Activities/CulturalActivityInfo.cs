using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(CulturalActivity))]
public class CulturalActivityInfo : IKeyedValue<string>, ISynchronizable
{
    [ProtoMember(1)]
    public string Id;

    public string Name;

    public int RngOffset;

    public CulturalActivityInfo()
    {
    }

    public CulturalActivityInfo(string id, string name, int rngOffset)
    {
        Id = id;
        Name = name;
        RngOffset = rngOffset;
    }

    public CulturalActivityInfo(CulturalActivityInfo baseInfo)
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
        switch (Id)
        {
            case CellCulturalActivity.FarmingActivityId:
                Name = CellCulturalActivity.FarmingActivityName;
                RngOffset = CellCulturalActivity.FarmingActivityRngOffset;
                break;

            case CellCulturalActivity.ForagingActivityId:
                Name = CellCulturalActivity.ForagingActivityName;
                RngOffset = CellCulturalActivity.ForagingActivityRngOffset;
                break;

            case CellCulturalActivity.FishingActivityId:
                Name = CellCulturalActivity.FishingActivityName;
                RngOffset = CellCulturalActivity.FishingActivityRngOffset;
                break;

            default:
                throw new System.Exception("Unhandled Activity Id: " + Id);
        }
    }
}
