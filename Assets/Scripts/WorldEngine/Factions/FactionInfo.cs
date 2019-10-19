using ProtoBuf;

[ProtoContract]
public class FactionInfo : ISynchronizable, IKeyedValue<long>
{
    [ProtoMember(1)]
    public string Type;

    [ProtoMember(2)]
    public long Id;

    [ProtoMember(3)]
    public Name Name = null;

    [ProtoMember(4)]
    public Faction Faction;

    private string _nameFormat;

    public FactionInfo()
    {

    }

    public FactionInfo(string type, long id, Faction faction)
    {
        Type = type;
        Id = id;

        Faction = faction;

        switch (Type)
        {
            case Clan.FactionType:
                _nameFormat = Clan.FactionNameFormat;
                break;
            default:
                throw new System.Exception("Unhandled Faction type: " + Type);
        }
    }

    public string GetNameAndTypeString()
    {
        return string.Format(_nameFormat, Name);
    }

    public string GetNameAndTypeStringBold()
    {
        return string.Format(_nameFormat, Name.BoldText);
    }

    public long GetKey()
    {
        return Id;
    }

    public void FinalizeLoad()
    {
        if (Faction != null)
            Faction.FinalizeLoad();

        switch (Type)
        {
            case Clan.FactionType:
                _nameFormat = Clan.FactionNameFormat;
                break;
            default:
                throw new System.Exception("Unhandled Faction type: " + Type);
        }
    }

    public void Synchronize()
    {
        if (Faction != null)
            Faction.Synchronize();
    }
}
