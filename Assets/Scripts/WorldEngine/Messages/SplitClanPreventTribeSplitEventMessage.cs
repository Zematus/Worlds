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

	public SplitClanPreventTribeSplitEventMessage (Clan clan, Tribe tribe, Agent agent, long date) : base (clan, WorldEvent.SplitClanPreventTribeSplitEventId, date) {

		clan.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		TribeId = tribe.Id;
	}

	protected override string GenerateMessage ()
	{
		Agent leader = World.GetMemorableAgent (AgentId);
		Tribe tribe = World.GetPolity (TribeId) as Tribe;

		return leader.Name.BoldText + ", leader of clan " + Faction.Name.BoldText + ", has prevented " + leader.PossessiveNoun + " clan from leaving the " + tribe.Name.BoldText + " Tribe";
	}
}
