using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class AvoidOpeningTribeEventMessage : PolityEventMessage {

	public Identifier AgentId;

	public Identifier TribeId;

	public AvoidOpeningTribeEventMessage () {

	}

	public AvoidOpeningTribeEventMessage (Tribe tribe, Agent agent, long date) :
		base (tribe, WorldEvent.AvoidOpenTribeDecisionEventId, date) {

		tribe.World.AddMemorableAgent (agent);

		AgentId = agent.Id;
		TribeId = tribe.Id;
	}

    protected override string GenerateMessage()
    {
        Agent leader = World.GetMemorableAgent(AgentId);
        PolityInfo sourceTribeInfo = World.GetPolityInfo(TribeId);

        return leader.Name.BoldText + ", leader of " + sourceTribeInfo.GetNameAndTypeStringBold() + 
            ", has decided not to open the tribe to external influences";
    }
}
