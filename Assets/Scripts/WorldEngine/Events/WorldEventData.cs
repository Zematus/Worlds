using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(PolityEventData))]
[ProtoInclude(200, typeof(FactionEventData))]
public class WorldEventData {

	[ProtoMember(1)]
	public long TypeId;

    [ProtoMember(2)]
    public long SpawnDate;

    [ProtoMember(3)]
    public long TriggerDate;

	public WorldEventData () {
	
	}

	public WorldEventData (WorldEvent e) {

		TypeId = e.TypeId;
        SpawnDate = e.SpawnDate;
		TriggerDate = e.TriggerDate;
	}
}
