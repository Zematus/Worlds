using ProtoBuf;

[ProtoContract]
public class AvoidFosterRelationshipEventMessage : PolityEventMessage {

	[ProtoMember(1)]
	public long AgentId;

	[ProtoMember(2)]
	public long SourceTribeId;

	[ProtoMember(3)]
	public long TargetTribeId;

	public AvoidFosterRelationshipEventMessage () {

	}

	public AvoidFosterRelationshipEventMessage (Tribe sourceTribe, Tribe targetTribe, Agent agent, long date) : base (sourceTribe, WorldEvent.AvoidFosterTribeRelationDecisionEventId, date) {

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

        return leader.Name.BoldText + ", leader of " + sourceTribeInfo.GetNameAndTypeStringBold() + 
            ", has avoided fostering the relationship with " + targetTribeInfo.GetNameAndTypeStringBold();
    }
}
