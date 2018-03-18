using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class SplitClanPreventTribeSplitEventMessage : FactionEventMessage {

	[XmlAttribute]
	public long AgentId;

	[XmlAttribute]
	public long TribeId;

	public SplitClanPreventTribeSplitEventMessage () {

	}

	public SplitClanPreventTribeSplitEventMessage (Faction faction, Tribe tribe, Agent agent, long date) : base (faction, WorldEvent.SplitingClanPreventTribeSplitEventId, date) {

		faction.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		TribeId = tribe.Id;
	}

	protected override string GenerateMessage ()
	{
		Agent leader = World.GetMemorableAgent (AgentId);
		Tribe tribe = World.GetPolity (TribeId) as Tribe;

		return leader.Name.BoldText + " has prevented clan " +  Faction.Name.BoldText + " from leaving the " + tribe.Name.BoldText + " Tribe";
	}
}
