using ProtoBuf;

[ProtoContract]
[ProtoInclude(100, typeof(ClanSplitEventMessage))]
[ProtoInclude(200, typeof(DemandClanAvoidInfluenceDemandEventMessage))]
[ProtoInclude(300, typeof(PreventClanSplitEventMessage))]
[ProtoInclude(400, typeof(SplitClanPreventTribeSplitEventMessage))]
[ProtoInclude(500, typeof(TribeSplitEventMessage))]
public abstract class FactionEventMessage : CellEventMessage {

	[ProtoMember(1)]
	public long FactionId;

    public FactionInfo FactionInfo => World.GetFactionInfo(FactionId);

    public FactionEventMessage()
    {

    }

    public FactionEventMessage(Faction faction, long id, long date) : base(faction.CoreGroup.Cell, id, date)
    {
        FactionId = faction.Id;
    }
}
