using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(PolityEventMessage))]
[ProtoInclude(200, typeof(CellEventMessage))]
public abstract class WorldEventMessage {

	[ProtoMember(1)]
	public long Id;

    [ProtoMember(1)]
    public long Date;

	public World World;

	public string Message => GenerateMessage ();

    public WorldEventMessage () {
	
	}

	public WorldEventMessage (World world, long id, long date) {
	
		World = world;
		Id = id;
		Date = date;
	}

	protected abstract string GenerateMessage ();
}
