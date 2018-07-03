using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class OpentribeEventMessage : PolityEventMessage {

	[XmlAttribute]
	public long AgentId;

	[XmlAttribute]
	public long TribeId;

	public OpentribeEventMessage () {

	}

	public OpentribeEventMessage (Tribe tribe, Agent agent, long date) : base (tribe, WorldEvent.OpenTribeDecisionEventId, date) {

		tribe.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		TribeId = tribe.Id;
	}

	protected override string GenerateMessage ()
	{
		Agent leader = World.GetMemorableAgent (AgentId);
		Tribe sourceTribe = World.GetPolity (TribeId) as Tribe;

		return leader.Name.BoldText + ", leader of the " + sourceTribe.Name.BoldText + " tribe, has decided to open the tribe to external influences";
	}
}
