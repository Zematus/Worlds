using ProtoBuf;

[ProtoContract]
public class RejectedMergeTribesOfferEventMessage : PolityEventMessage {

    [ProtoMember(1)]
    public long AgentId;

    [ProtoMember(2)]
    public long SourceTribeId;

    [ProtoMember(3)]
    public long TargetTribeId;

	public RejectedMergeTribesOfferEventMessage () {

	}

	public RejectedMergeTribesOfferEventMessage (Tribe sourceTribe, Tribe targetTribe, Agent agent, long date) : base (sourceTribe, WorldEvent.RejectInfluenceDemandDecisionEventId, date) {

		sourceTribe.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		SourceTribeId = sourceTribe.Id;
		TargetTribeId = targetTribe.Id;
	}

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);
        PolityInfo sourceTribeInfo = World.GetPolityInfo(SourceTribeId);
        PolityInfo targetTribeInfo = World.GetPolityInfo(TargetTribeId);

        return leader.Name.BoldText + ", leader of " + targetTribeInfo.GetNameAndTypeStringBold() + ", has rejected the offer from " +
            sourceTribeInfo.GetNameAndTypeStringBold() + " for " + leader.PossessiveNoun + " tribe to merge into theirs";
    }
}
