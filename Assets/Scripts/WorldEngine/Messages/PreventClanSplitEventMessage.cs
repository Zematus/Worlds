using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class PreventClanSplitEventMessage : FactionEventMessage {

	[XmlAttribute]
	public long AgentId;

	public PreventClanSplitEventMessage () {

	}

	public PreventClanSplitEventMessage (Faction faction, Agent agent, long date) : base (faction, WorldEvent.PreventClanSplitEventId, date) {

		faction.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
	}

	protected override string GenerateMessage ()
	{
		Agent leader = World.GetMemorableAgent (AgentId);

		return leader.Name.BoldText + " has prevented clan " +  Faction.Name.BoldText + " from splitting";
	}
}
