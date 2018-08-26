using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class AcceptedClanInlfuenceDemandEventMessage : PolityEventMessage {

	[XmlAttribute]
	public long AgentId;

	[XmlAttribute]
	public long DemandClanId;

	[XmlAttribute]
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
