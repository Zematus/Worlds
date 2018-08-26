using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class OpenTribeEventMessage : PolityEventMessage {

	[XmlAttribute]
	public long AgentId;

	[XmlAttribute]
	public long TribeId;

	public OpenTribeEventMessage () {

	}

	public OpenTribeEventMessage (Tribe tribe, Agent agent, long date) : base (tribe, WorldEvent.OpenTribeDecisionEventId, date) {

		tribe.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		TribeId = tribe.Id;
	}

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);
        PolityInfo sourceTribeInfo = World.GetPolityInfo(TribeId);

        return leader.Name.BoldText + ", leader of " + sourceTribeInfo.GetNameAndTypeStringBold() +
            ", has decided to open the tribe to external influences";
    }
}
