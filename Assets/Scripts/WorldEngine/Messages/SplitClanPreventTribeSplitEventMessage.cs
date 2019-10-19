using ProtoBuf;

[ProtoContract]
public class SplitClanPreventTribeSplitEventMessage : FactionEventMessage {

    [ProtoMember(1)]
    public long AgentId;

    [ProtoMember(2)]
    public long TribeId;

	public SplitClanPreventTribeSplitEventMessage () {

	}

	public SplitClanPreventTribeSplitEventMessage (Clan clan, Tribe tribe, Agent agent, long date) : base (clan, WorldEvent.SplitClanPreventTribeSplitEventId, date) {

		clan.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		TribeId = tribe.Id;
	}

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);
        PolityInfo tribeInfo = World.GetPolityInfo(TribeId);

        return leader.Name.BoldText + ", leader of " + FactionInfo.GetNameAndTypeStringBold() + ", has prevented " +
            leader.PossessiveNoun + " clan from leaving " + tribeInfo.GetNameAndTypeStringBold();
    }
}
