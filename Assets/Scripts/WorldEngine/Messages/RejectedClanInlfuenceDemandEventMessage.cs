using ProtoBuf;

[ProtoContract]
public class RejectedClanInlfuenceDemandEventMessage : PolityEventMessage {

	[ProtoMember(1)]
	public long AgentId;

    [ProtoMember(2)]
    public long DemandClanId;

    [ProtoMember(3)]
    public long DominantClanId;

	public RejectedClanInlfuenceDemandEventMessage()
    {

    }

    public RejectedClanInlfuenceDemandEventMessage(Tribe tribe, Clan dominantClan, Clan demandClan, Agent agent, long date) : base(tribe, WorldEvent.RejectInfluenceDemandDecisionEventId, date)
    {
        tribe.World.AddMemorableAgent(agent);

        AgentId = agent.Id;
        DemandClanId = demandClan.Id;
        DominantClanId = dominantClan.Id;
    }

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);
        FactionInfo demandClanInfo = World.GetFactionInfo(DemandClanId);
        FactionInfo dominantClanInfo = World.GetFactionInfo(DominantClanId);

        return leader.Name.BoldText + ", leader of " + dominantClanInfo.GetNameAndTypeStringBold() + ", has rejected the demand for influence from " + demandClanInfo.GetNameAndTypeStringBold();
    }
}
