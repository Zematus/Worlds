using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class RejectedClanInlfuenceDemandEventMessage : PolityEventMessage {

	[XmlAttribute]
	public long AgentId;

	[XmlAttribute]
	public long DemandClanId;

	[XmlAttribute]
	public long DominantClanId;

	public RejectedClanInlfuenceDemandEventMessage () {

	}

	public RejectedClanInlfuenceDemandEventMessage (Tribe tribe, Clan dominantClan, Clan demandClan, Agent agent, long date) : base (tribe, WorldEvent.PreventTribeSplitEventId, date) {

		tribe.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		DemandClanId = demandClan.Id;
		DominantClanId = dominantClan.Id;
	}

	protected override string GenerateMessage ()
	{
		Agent leader = World.GetMemorableAgent (AgentId);
		Clan demandClan = World.GetFaction (DemandClanId) as Clan;
		Clan dominantClan = World.GetFaction (DominantClanId) as Clan;

		return leader.Name.BoldText + ", leader of clan " + dominantClan.Name.BoldText + ", has rejected the demand for influence from clan " + demandClan.Name.BoldText + "";
	}
}
