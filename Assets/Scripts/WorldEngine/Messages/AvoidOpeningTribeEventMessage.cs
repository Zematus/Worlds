using ProtoBuf;

[ProtoContract]
public class AvoidOpeningTribeEventMessage : PolityEventMessage {

	[ProtoMember(1)]
	public long AgentId;

	[ProtoMember(2)]
	public long TribeId;

	public AvoidOpeningTribeEventMessage () {

	}

	public AvoidOpeningTribeEventMessage (Tribe tribe, Agent agent, long date) : base (tribe, WorldEvent.AvoidOpenTribeDecisionEventId, date) {

		tribe.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		TribeId = tribe.Id;
	}

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);
        PolityInfo sourceTribeInfo = World.GetPolityInfo(TribeId);

        return leader.Name.BoldText + ", leader of " + sourceTribeInfo.GetNameAndTypeStringBold() + 
            ", has decided not to open the tribe to external influences";
    }
}
