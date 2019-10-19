using ProtoBuf;

public enum PolityType
{
    None,
    Tribe
}

[ProtoContract]
public class PolityInfo : ISynchronizable, IKeyedValue<long>
{
    [ProtoMember(1)]
    public string Type;

    [ProtoMember(2)]
    public long Id;

    [ProtoMember(3)]
    public Name Name;

    [ProtoMember(4)]
    public Polity Polity;

    private string _nameFormat;

    private PolityType _type;

    public PolityInfo()
    {
	
	}

	public PolityInfo(string type, long id, Polity polity)
    {
        Id = id;

        Polity = polity;

        SetType(type);
    }

    public static PolityType GetPolityType(string typeStr)
    {
        switch (typeStr)
        {
            case Tribe.PolityTypeStr:
                return PolityType.Tribe;
            default:
                throw new System.Exception("PolityInfo: Unrecognized polity type: " + typeStr);
        }
    }

    private void SetType(string typeStr)
    {
        Type = typeStr;

        _type = GetPolityType(typeStr);

        switch (_type)
        {
            case PolityType.Tribe:
                _type = PolityType.Tribe;
                _nameFormat = Tribe.PolityNameFormat;
                break;
            default:
                throw new System.Exception("PolityInfo: Unhandled polity type: " + _type);
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
        Polity?.FinalizeLoad();

        SetType(Type);
    }

    public void Synchronize()
    {
        Polity?.Synchronize();
    }
}
