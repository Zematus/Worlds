using ProtoBuf;

[ProtoContract]
public class RejectedFosterRelationshipAttemptEventMessage : PolityEventMessage {

    [ProtoMember(1)]
    public long AgentId;

    [ProtoMember(2)]
    public long SourceTribeId;

    [ProtoMember(3)]
    public long TargetTribeId;

	public RejectedFosterRelationshipAttemptEventMessage () {

	}

	public RejectedFosterRelationshipAttemptEventMessage (Tribe sourceTribe, Tribe targetTribe, Agent agent, long date) : base (sourceTribe, WorldEvent.RejectInfluenceDemandDecisionEventId, date) {

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

        return leader.Name.BoldText + ", leader of " + targetTribeInfo.GetNameAndTypeStringBold() + ", has rejected the attempt from " +
            sourceTribeInfo.GetNameAndTypeStringBold() + " to improve the relationship between the two tribes";
    }
}
