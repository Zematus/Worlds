using ProtoBuf;

[ProtoContract]
public class AcceptedFosterRelationshipAttemptEventMessage : PolityEventMessage {

	[ProtoMember(1)]
	public long AgentId;

	[ProtoMember(2)]
	public long SourceTribeId;

	[ProtoMember(3)]
	public long TargetTribeId;

	public AcceptedFosterRelationshipAttemptEventMessage()
    {

    }

    public AcceptedFosterRelationshipAttemptEventMessage(Tribe sourceTribe, Tribe targetTribe, Agent agent, long date) : base(sourceTribe, WorldEvent.AcceptFosterTribeRelationDecisionEventId, date)
    {
        sourceTribe.World.AddMemorableAgent(agent);

        AgentId = agent.Id;
        SourceTribeId = sourceTribe.Id;
        TargetTribeId = targetTribe.Id;
    }

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);
        PolityInfo sourceTribeInfo = World.GetPolityInfo(SourceTribeId);
        PolityInfo targetTribeInfo = World.GetPolityInfo(TargetTribeId);

        return leader.Name.BoldText + ", leader of " + targetTribeInfo.GetNameAndTypeStringBold() + ", has accepted the offer from " +
            sourceTribeInfo.GetNameAndTypeStringBold() + " to improve the relationship between the two tribes";
    }
}
