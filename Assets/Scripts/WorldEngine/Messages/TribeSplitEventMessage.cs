using ProtoBuf;

[ProtoContract]
public class TribeSplitEventMessage : FactionEventMessage {

    [ProtoMember(1)]
    public long TribeId;

    [ProtoMember(2)]
    public long NewTribeId;

	public TribeSplitEventMessage () {

	}

	public TribeSplitEventMessage (Clan splitClan, Tribe tribe, Tribe newTribe, long date) : base (splitClan, WorldEvent.TribeSplitDecisionEventId, date) {

		TribeId = tribe.Id;
		NewTribeId = newTribe.Id;
	}

	protected override string GenerateMessage()
    {
        PolityInfo tribeInfo = World.GetPolityInfo(TribeId);
        PolityInfo newTribeInfo = World.GetPolityInfo(NewTribeId);

        return "A new tribe, " + newTribeInfo.Name.BoldText + ", formed by " + 
            FactionInfo.GetNameAndTypeStringBold() + ", has split from " + tribeInfo.GetNameAndTypeStringBold();
    }
}
