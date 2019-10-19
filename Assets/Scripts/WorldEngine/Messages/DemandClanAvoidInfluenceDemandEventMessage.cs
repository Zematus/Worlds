using ProtoBuf;

[ProtoContract]
public class DemandClanAvoidInfluenceDemandEventMessage : FactionEventMessage {

	[ProtoMember(1)]
	public long AgentId;

	[ProtoMember(2)]
	public long DemandClanId;

	[ProtoMember(3)]
	public long DominantClanId;

	public DemandClanAvoidInfluenceDemandEventMessage()
    {

    }

    public DemandClanAvoidInfluenceDemandEventMessage(Clan demandClan, Clan dominantClan, Tribe tribe, Agent agent, long date) : base(demandClan, WorldEvent.ClanAvoidsInfluenceDemandDecisionEventId, date)
    {
        demandClan.World.AddMemorableAgent(agent);

        AgentId = agent.Id;
        DemandClanId = demandClan.Id;
        DominantClanId = dominantClan.Id;
    }

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);
        FactionInfo demandClanInfo = World.GetFactionInfo(DemandClanId);
        FactionInfo dominantClanInfo = World.GetFactionInfo(DominantClanId);

        return leader.Name.BoldText + ", leader of " + demandClanInfo.GetNameAndTypeStringBold() + ", has avoided making a demand for more influence from " + dominantClanInfo.GetNameAndTypeStringBold();
    }
}
