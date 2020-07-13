using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[System.Obsolete]
public class RejectedClanInlfuenceDemandEventMessage : PolityEventMessage
{
    public Identifier AgentId;
    public Identifier DemandClanId;
    public Identifier DominantClanId;

    public RejectedClanInlfuenceDemandEventMessage()
    {

    }

    public RejectedClanInlfuenceDemandEventMessage(
        Tribe tribe, Clan dominantClan, Clan demandClan, Agent agent, long date) :
        base(tribe, WorldEvent.RejectInfluenceDemandDecisionEventId, date)
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

        return leader.Name.BoldText + ", leader of " + dominantClanInfo.GetNameAndTypeStringBold() +
            ", has rejected the demand for influence from " + demandClanInfo.GetNameAndTypeStringBold();
    }
}
