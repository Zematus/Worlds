using ProtoBuf;

[ProtoContract]
public class AcceptedClanInlfuenceDemandEventMessage : PolityEventMessage {

	[ProtoMember(1)]
	public long AgentId;

    [ProtoMember(2)]
    public long DemandClanId;

    [ProtoMember(3)]
    public long DominantClanId;

    public AcceptedClanInlfuenceDemandEventMessage()
    {

    }

    public AcceptedClanInlfuenceDemandEventMessage(Tribe tribe, Clan dominantClan, Clan demandClan, Agent agent, long date) : base(tribe, WorldEvent.AcceptInfluenceDemandDecisionEventId, date)
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

        return leader.Name.BoldText + ", leader of " + dominantClanInfo.GetNameAndTypeStringBold() + ", has accepted the demand for influence from " + demandClanInfo.GetNameAndTypeStringBold();
    }
}
