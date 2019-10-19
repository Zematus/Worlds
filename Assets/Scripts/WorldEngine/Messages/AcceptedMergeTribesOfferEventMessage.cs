using ProtoBuf;

[ProtoContract]
public class AcceptedMergeTribesOfferEventMessage : PolityEventMessage {
    
    [ProtoMember(1)]
    public long TargetTribeLeaderId;

    [ProtoMember(2)]
	public long SourceTribeId;

	[ProtoMember(3)]
	public long TargetTribeId;

	public AcceptedMergeTribesOfferEventMessage () {

	}

	public AcceptedMergeTribesOfferEventMessage (Tribe sourceTribe, Tribe targetTribe, Agent targetTribeLeader, long date) : base (sourceTribe, WorldEvent.AcceptFosterTribeRelationDecisionEventId, date) {

		sourceTribe.World.AddMemorableAgent (targetTribeLeader);

		TargetTribeLeaderId = targetTribeLeader.Id;
		SourceTribeId = sourceTribe.Id;
		TargetTribeId = targetTribe.Id;
	}

    protected override string GenerateMessage()
    {
        Agent targetTribeLeader = World.GetMemorableAgent(TargetTribeLeaderId);
        PolityInfo sourceTribeInfo = World.GetPolityInfo(SourceTribeId);
        PolityInfo targetTribeInfo = World.GetPolityInfo(TargetTribeId);

        return targetTribeLeader.Name.BoldText + ", leader of " + targetTribeInfo.GetNameAndTypeStringBold() + ", has accepted the offer to merge " +
            targetTribeLeader.PossessiveNoun + " tribe into " + sourceTribeInfo.GetNameAndTypeStringBold();
    }
}
