using ProtoBuf;

[ProtoContract]
public class OpenTribeEventMessage : PolityEventMessage {

	[ProtoMember(1)]
	public long AgentId;

	[ProtoMember(2)]
	public long TribeId;

	public OpenTribeEventMessage () {

	}

	public OpenTribeEventMessage (Tribe tribe, Agent agent, long date) : base (tribe, WorldEvent.OpenTribeDecisionEventId, date) {

		tribe.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		TribeId = tribe.Id;
	}

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);
        PolityInfo sourceTribeInfo = World.GetPolityInfo(TribeId);

        return leader.Name.BoldText + ", leader of " + sourceTribeInfo.GetNameAndTypeStringBold() +
            ", has decided to open the tribe to external influences";
    }
}
