using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(FactionEventMessage))]
[ProtoInclude(200, typeof(DiscoveryEventMessage))]
[ProtoInclude(300, typeof(PolityFormationEventMessage))]
public abstract class CellEventMessage : WorldEventMessage {

    [ProtoMember(1)]
	public WorldPosition Position;

	public CellEventMessage () {
	
	}

	public CellEventMessage (TerrainCell cell, long id, long date) : base (cell.World, id, date) {

		Position = cell.Position;
	}
}
