using ProtoBuf;

[ProtoContract]
public class PolityContact :IKeyedValue<long>
{
	[ProtoMember(1)]
	public long Id;

	[ProtoMember(2)]
	public int GroupCount;

	public Polity Polity;

	public PolityContact () {
	}

	public PolityContact (Polity polity, int initialGroupCount = 0) {

		Polity = polity;

		Id = polity.Id;

		GroupCount = initialGroupCount;
	}

    public long GetKey()
    {
        return Id;
    }
}
