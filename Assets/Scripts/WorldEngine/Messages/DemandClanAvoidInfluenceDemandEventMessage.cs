﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class DemandClanAvoidInfluenceDemandEventMessage : FactionEventMessage {

	[XmlAttribute]
	public long AgentId;

	[XmlAttribute]
	public long DemandClanId;

	[XmlAttribute]
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
